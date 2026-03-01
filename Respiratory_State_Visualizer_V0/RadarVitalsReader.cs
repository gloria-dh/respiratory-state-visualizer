using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Respiratory_State_Visualizer_V0
{
    /// <summary>
    /// Reads vital-signs frames from a TI mmWave radar over a serial port.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The reader spawns a background <see cref="System.Threading.Tasks.Task"/> that
    /// continuously reads from the DATA serial port. All events
    /// (<see cref="VitalsReceived"/>, <see cref="StatusChanged"/>,
    /// <see cref="ErrorOccurred"/>) are raised on that background thread.
    /// </para>
    /// <para>
    /// Subscribers that update UI controls <b>must</b> marshal calls to the
    /// UI thread, for example via <c>Control.BeginInvoke</c>.
    /// </para>
    /// </remarks>
    internal sealed class RadarVitalsReader : IDisposable
    {
        private const int CliBaudRate = 115200;
        private const int DataBaudRate = 921600;
        private const int VitalsTlvType = 1040;
        private const int VitalsStructSize = 136;

        // --- State-machine thresholds (from breathingState.py) ---
        private const float BreathDeviationThreshold = 0.02f;
        private const float AlertBreathRateThreshold = 23.0f;
        private const int MaxGracePackets = 1;
        private const float HeartRateOffset = 30.0f;

        private static readonly byte[] MagicWord = { 0x02, 0x01, 0x04, 0x03, 0x06, 0x05, 0x08, 0x07 };

        private readonly object portLock = new object();
        private SerialPort dataPort;
        private CancellationTokenSource cancellation;
        private Task readerTask;
        private string lastCliPortName;

        // State-machine variables (reset each session)
        private RespiratoryState previousStatus;
        private int neutralGraceCounter;

        /// <summary>Raised when a valid vitals frame is parsed.</summary>
        /// <remarks>Args: heartRate, breathRate, breathDeviation, state. Raised on background thread.</remarks>
        internal event Action<float, float, float, RespiratoryState> VitalsReceived;

        /// <summary>Raised when the reader status changes.</summary>
        internal event Action<string> StatusChanged;

        /// <summary>Raised when an unrecoverable error occurs during reading.</summary>
        internal event Action<string> ErrorOccurred;

        internal bool IsRunning => readerTask != null && !readerTask.IsCompleted;

        internal void Start(string cliPortName, string dataPortName, string configFilePath)
        {
            if (IsRunning)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(cliPortName) || string.IsNullOrWhiteSpace(dataPortName))
            {
                throw new ArgumentException("Both CLI and DATA COM ports are required.");
            }

            if (string.IsNullOrWhiteSpace(configFilePath) || !File.Exists(configFilePath))
            {
                throw new FileNotFoundException("Configuration file not found.", configFilePath);
            }

            lastCliPortName = cliPortName;
            cancellation = new CancellationTokenSource();

            // Reset state machine for new session
            previousStatus = RespiratoryState.Neutral;
            neutralGraceCounter = 0;

            readerTask = Task.Run(() =>
            {
                try
                {
                    // Send sensorStop first to ensure clean slate (fixes replug issue)
                    StatusChanged?.Invoke("Sending sensorStop to reset sensor...");
                    SendSensorStop();
                    Thread.Sleep(150);

                    StatusChanged?.Invoke("Sending config...");
                    SendConfig(cliPortName, configFilePath);

                    dataPort = new SerialPort(dataPortName, DataBaudRate)
                    {
                        ReadTimeout = 200,
                        WriteTimeout = 500
                    };

                    dataPort.Open();

                    // Flush any stale data left in the serial buffer
                    dataPort.DiscardInBuffer();
                    dataPort.DiscardOutBuffer();

                    StatusChanged?.Invoke($"Listening on {dataPortName}.");
                    ListenLoop(cancellation.Token);
                }
                catch (Exception ex)
                {
                    if (!cancellation.Token.IsCancellationRequested)
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

            try
            {
                readerTask?.Wait(1000);
            }
            catch (AggregateException)
            {
            }

            CloseDataPort();
            SendSensorStop();

            // Small delay to let the sensor fully stop before any restart
            Thread.Sleep(100);

            if (cancellation != null)
            {
                cancellation.Dispose();
                cancellation = null;
            }

            readerTask = null;
            StatusChanged?.Invoke("Stopped.");
        }

        private void ListenLoop(CancellationToken token)
        {
            try
            {
                byte[] headerBuffer = new byte[8];

                while (!token.IsCancellationRequested)
                {
                    if (!FindMagicWord(dataPort, token))
                    {
                        continue;
                    }

                    if (!TryReadExact(dataPort, headerBuffer, 8, token))
                    {
                        continue;
                    }

                    uint totalPacketLength = BitConverter.ToUInt32(headerBuffer, 4);
                    int bytesToRead = (int)totalPacketLength - 16;
                    if (bytesToRead <= 0 || bytesToRead > 1048576)
                    {
                        continue;
                    }

                    byte[] frameData = new byte[bytesToRead];
                    if (!TryReadExact(dataPort, frameData, bytesToRead, token))
                    {
                        continue;
                    }

                    VitalsData? vitals = ParseFrame(frameData);
                    if (vitals.HasValue)
                    {
                        float heartRate = vitals.Value.HeartRate + HeartRateOffset;
                        float breathRate = vitals.Value.BreathRate;
                        float breathDev = vitals.Value.BreathDeviation;

                        // --- State machine logic from breathingState.py ---
                        RespiratoryState currentStatus;

                        bool isLow = breathDev < BreathDeviationThreshold;
                        bool isAlert = breathRate > AlertBreathRateThreshold;

                        if (isAlert)
                        {
                            currentStatus = RespiratoryState.Alert;
                            neutralGraceCounter = 0;
                        }
                        else if (isLow)
                        {
                            if (previousStatus == RespiratoryState.Strained ||
                                previousStatus == RespiratoryState.HoldingBreath ||
                                previousStatus == RespiratoryState.Recovering)
                            {
                                currentStatus = RespiratoryState.HoldingBreath;
                            }
                            else
                            {
                                currentStatus = RespiratoryState.Strained;
                            }
                            neutralGraceCounter = 0;
                        }
                        else
                        {
                            if (previousStatus == RespiratoryState.HoldingBreath &&
                                neutralGraceCounter < MaxGracePackets)
                            {
                                currentStatus = RespiratoryState.Recovering;
                                neutralGraceCounter++;
                            }
                            else
                            {
                                currentStatus = RespiratoryState.Neutral;
                                neutralGraceCounter = 0;
                            }
                        }

                        // Override breath rate for Holding Breath
                        if (currentStatus == RespiratoryState.HoldingBreath)
                        {
                            breathRate = 0.0f;
                        }

                        previousStatus = currentStatus;

                        VitalsReceived?.Invoke(heartRate, breathRate, breathDev, currentStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    ErrorOccurred?.Invoke(ex.Message);
                }
            }
            finally
            {
                CloseDataPort();
            }
        }

        private void SendConfig(string cliPortName, string configFilePath)
        {
            string[] configLines = File.ReadAllLines(configFilePath);
            bool sensorStartSent = false;

            using (SerialPort cliPort = new SerialPort(cliPortName, CliBaudRate)
            {
                ReadTimeout = 800,
                WriteTimeout = 500
            })
            {
                cliPort.Open();
                StatusChanged?.Invoke($"Connected to CLI port {cliPortName}. Sending config...");

                foreach (string rawLine in configLines)
                {
                    string line = rawLine.Trim();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("%"))
                    {
                        continue;
                    }

                    cliPort.Write(line + Environment.NewLine);
                    Thread.Sleep(30);
                    SafeReadLine(cliPort);
                    SafeReadLine(cliPort);

                    if (line.IndexOf("sensorStart", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        sensorStartSent = true;
                    }
                }
            }

            if (!sensorStartSent)
            {
                throw new InvalidOperationException("Configuration file did not send sensorStart.");
            }
        }

        private static string SafeReadLine(SerialPort port)
        {
            try
            {
                return port.ReadLine();
            }
            catch (TimeoutException)
            {
                return string.Empty;
            }
        }

        private static bool FindMagicWord(SerialPort port, CancellationToken token)
        {
            int matchIndex = 0;
            byte[] oneByte = new byte[1];

            while (!token.IsCancellationRequested)
            {
                try
                {
                    int bytesRead = port.Read(oneByte, 0, 1);
                    if (bytesRead == 0)
                    {
                        continue;
                    }

                    if (oneByte[0] == MagicWord[matchIndex])
                    {
                        matchIndex++;
                        if (matchIndex == MagicWord.Length)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        matchIndex = oneByte[0] == MagicWord[0] ? 1 : 0;
                    }
                }
                catch (TimeoutException)
                {
                }
            }

            return false;
        }

        private static bool TryReadExact(SerialPort port, byte[] buffer, int count, CancellationToken token)
        {
            int offset = 0;

            while (offset < count && !token.IsCancellationRequested)
            {
                try
                {
                    int read = port.Read(buffer, offset, count - offset);
                    if (read <= 0)
                    {
                        continue;
                    }

                    offset += read;
                }
                catch (TimeoutException)
                {
                }
            }

            return offset == count;
        }

        private static VitalsData? ParseFrame(byte[] frameData)
        {
            if (frameData == null || frameData.Length < 24)
            {
                return null;
            }

            uint numTlvs = BitConverter.ToUInt32(frameData, 16);
            int pointer = 24;

            for (uint i = 0; i < numTlvs; i++)
            {
                if (pointer + 8 > frameData.Length)
                {
                    return null;
                }

                uint tlvType = BitConverter.ToUInt32(frameData, pointer);
                uint tlvLength = BitConverter.ToUInt32(frameData, pointer + 4);
                pointer += 8;

                if (tlvLength > int.MaxValue)
                {
                    return null;
                }

                int payloadLength = (int)tlvLength;
                if (pointer + payloadLength > frameData.Length)
                {
                    return null;
                }

                if (tlvType == VitalsTlvType)
                {
                    return ParseVitalsTlv(frameData, pointer, payloadLength);
                }

                pointer += payloadLength;
            }

            return null;
        }

        private static VitalsData? ParseVitalsTlv(byte[] frameData, int offset, int length)
        {
            // Need at least 16 bytes: id(2) + rangeBin(2) + breathDev(4) + heart(4) + breath(4)
            if (length < VitalsStructSize || offset + 16 > frameData.Length)
            {
                return null;
            }

            // Layout matches breathingState.py: '<2H3f' => id(2B) + rangeBin(2B) + breathDev(4B) + heart(4B) + breath(4B)
            float breathDeviation = BitConverter.ToSingle(frameData, offset + 4);
            float heartRate = BitConverter.ToSingle(frameData, offset + 8);
            float breathRate = BitConverter.ToSingle(frameData, offset + 12);

            if (float.IsNaN(heartRate) || float.IsInfinity(heartRate))
            {
                return null;
            }

            if (float.IsNaN(breathRate) || float.IsInfinity(breathRate))
            {
                return null;
            }

            if (float.IsNaN(breathDeviation) || float.IsInfinity(breathDeviation))
            {
                return null;
            }

            return new VitalsData
            {
                HeartRate = heartRate,
                BreathRate = breathRate,
                BreathDeviation = breathDeviation
            };
        }

        private void CloseDataPort()
        {
            lock (portLock)
            {
                if (dataPort == null)
                {
                    return;
                }

                try
                {
                    if (dataPort.IsOpen)
                    {
                        dataPort.DiscardInBuffer();
                        dataPort.DiscardOutBuffer();
                        dataPort.Close();
                    }
                }
                catch
                {
                }
                finally
                {
                    dataPort.Dispose();
                    dataPort = null;
                }
            }
        }

        private void SendSensorStop()
        {
            if (string.IsNullOrWhiteSpace(lastCliPortName))
            {
                return;
            }

            try
            {
                using (SerialPort cliPort = new SerialPort(lastCliPortName, CliBaudRate)
                {
                    ReadTimeout = 500,
                    WriteTimeout = 500
                })
                {
                    cliPort.Open();
                    cliPort.Write("sensorStop" + Environment.NewLine);
                    Thread.Sleep(30);
                }
            }
            catch
            {
                // Best-effort shutdown; sensor may already be stopped or disconnected.
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private struct VitalsData
        {
            internal float HeartRate;
            internal float BreathRate;
            internal float BreathDeviation;
        }
    }
}
