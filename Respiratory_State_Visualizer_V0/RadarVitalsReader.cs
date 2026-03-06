using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Respiratory_State_Visualizer_V0
{
    /// <summary>
    /// Launches the Python breathing-state script as a child process and
    /// receives vital-signs data through its standard-output pipe.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Python script handles all serial communication, frame parsing,
    /// state-machine logic and CSV logging.  This class starts the process,
    /// reads its stdout line-by-line, and translates the received lines
    /// into events.
    /// </para>
    /// <para>
    /// The script is launched with python's -u flag (unbuffered stdout) so
    /// that every print() is delivered immediately to the pipe reader.
    /// </para>
    /// </remarks>
    internal sealed class RadarVitalsReader : IDisposable
    {
        private Process pythonProcess;
        private CancellationTokenSource cancellation;
        private Task readerTask;

        /// <summary>Raised when a valid vitals line is parsed from the Python script.</summary>
        /// <remarks>Args: heartRate, breathRate, breathDeviation, state. Raised on background thread.</remarks>
        internal event Action<float, float, float, RespiratoryState> VitalsReceived;

        /// <summary>Raised when the Python script reports a status update.</summary>
        internal event Action<string> StatusChanged;

        /// <summary>Raised when the Python script reports an error.</summary>
        internal event Action<string> ErrorOccurred;

        internal bool IsRunning => readerTask != null && !readerTask.IsCompleted;

        internal void Start(string cliPort, string dataPort, string configFilePath, string pythonScriptPath)
        {
            if (IsRunning)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(cliPort) || string.IsNullOrWhiteSpace(dataPort))
            {
                throw new ArgumentException("Both CLI and DATA COM ports are required.");
            }

            if (string.IsNullOrWhiteSpace(configFilePath) || !File.Exists(configFilePath))
            {
                throw new FileNotFoundException("Configuration file not found.", configFilePath);
            }

            if (string.IsNullOrWhiteSpace(pythonScriptPath) || !File.Exists(pythonScriptPath))
            {
                throw new FileNotFoundException("Python script not found.", pythonScriptPath);
            }

            // Put logs next to the executable
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            cancellation = new CancellationTokenSource();

            readerTask = Task.Run(() =>
            {
                try
                {
                    StatusChanged?.Invoke("Launching Python script...");

                    string arguments = string.Format(
                        "-u \"{0}\" --cli-port \"{1}\" --data-port \"{2}\" --config-file \"{3}\"",
                        pythonScriptPath, cliPort, dataPort, configFilePath);

                    pythonProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "python",
                            Arguments = arguments,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        }
                    };

                    pythonProcess.Start();

                    // Read stderr on a background thread so Python errors are visible
                    Task.Run(() =>
                    {
                        try
                        {
                            string stderrLine;
                            while ((stderrLine = pythonProcess.StandardError.ReadLine()) != null)
                            {
                                if (!string.IsNullOrWhiteSpace(stderrLine))
                                {
                                    ErrorOccurred?.Invoke($"Python stderr: {stderrLine}");
                                }
                            }
                        }
                        catch { }
                    });

                    StatusChanged?.Invoke("Reading vitals from Python stdout...");

                    ReadStdoutLoop(cancellation.Token);
                }
                catch (OperationCanceledException)
                {
                    // Normal cancellation, ignore
                }
                catch (Exception ex)
                {
                    if (cancellation == null || !cancellation.Token.IsCancellationRequested)
                    {
                        ErrorOccurred?.Invoke(ex.Message);
                    }
                }
            }, cancellation.Token);
        }

        internal void Stop()
        {
            if (cancellation != null)
            {
                cancellation.Cancel();
            }

            KillProcess();

            try
            {
                readerTask?.Wait(2000);
            }
            catch (AggregateException)
            {
            }

            if (cancellation != null)
            {
                cancellation.Dispose();
                cancellation = null;
            }

            readerTask = null;
            StatusChanged?.Invoke("Stopped.");
        }

        /// <summary>
        /// Reads lines from the Python process's redirected stdout until the
        /// stream ends or cancellation is requested.
        /// </summary>
        private void ReadStdoutLoop(CancellationToken token)
        {
            try
            {
                string line;
                while (!token.IsCancellationRequested &&
                       (line = pythonProcess.StandardOutput.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    ParseLine(line);
                }

                if (!token.IsCancellationRequested)
                {
                    StatusChanged?.Invoke("Python script exited.");
                }
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    ErrorOccurred?.Invoke(ex.Message);
                }
            }
        }

        private void ParseLine(string line)
        {
            // Expected formats:
            //   VITALS|72.50|15.20|0.0032|Neutral
            //   STATUS|Sending config...
            //   ERROR|Could not open port

            int firstPipe = line.IndexOf('|');
            if (firstPipe <= 0)
            {
                return;
            }

            string prefix = line.Substring(0, firstPipe);
            string payload = line.Substring(firstPipe + 1);

            switch (prefix)
            {
                case "VITALS":
                    ParseVitalsPayload(payload);
                    break;

                case "STATUS":
                    StatusChanged?.Invoke(payload);
                    break;

                case "ERROR":
                    ErrorOccurred?.Invoke(payload);
                    break;
            }
        }

        private void ParseVitalsPayload(string payload)
        {
            // payload: "72.50|15.20|0.0032|Neutral"
            string[] parts = payload.Split('|');
            if (parts.Length < 4)
            {
                return;
            }

            if (!float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float heartRate))
            {
                return;
            }

            if (!float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float breathRate))
            {
                return;
            }

            if (!float.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float breathDeviation))
            {
                return;
            }

            RespiratoryState state;
            switch (parts[3])
            {
                case "Neutral":
                    state = RespiratoryState.Neutral;
                    break;
                case "Strained":
                    state = RespiratoryState.Strained;
                    break;
                case "HoldingBreath":
                    state = RespiratoryState.HoldingBreath;
                    break;
                case "Recovering":
                    state = RespiratoryState.Recovering;
                    break;
                case "Alert":
                    state = RespiratoryState.Alert;
                    break;
                default:
                    return;
            }

            VitalsReceived?.Invoke(heartRate, breathRate, breathDeviation, state);
        }

        private void KillProcess()
        {
            if (pythonProcess == null)
            {
                return;
            }

            try
            {
                if (!pythonProcess.HasExited)
                {
                    pythonProcess.Kill();
                    pythonProcess.WaitForExit(2000);
                }
            }
            catch
            {
                // Best-effort kill
            }
            finally
            {
                pythonProcess.Dispose();
                pythonProcess = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
