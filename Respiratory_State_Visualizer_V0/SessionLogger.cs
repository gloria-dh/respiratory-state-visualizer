using System;
using System.Globalization;
using System.IO;

namespace Respiratory_State_Visualizer_V0
{
    // Writes sensor data to a timestamped CSV in the logs/ folder.
    internal sealed class SessionLogger : IDisposable
    {
        private StreamWriter writer;
        private int packetNumber;


        internal void StartSession()
        {
            EndSession();

            string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logsDir);

            string fileName = $"session_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string filePath = Path.Combine(logsDir, fileName);

            writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8)
            {
                AutoFlush = true
            };

            writer.WriteLine("Timestamp,PacketNumber,HeartRate,BreathRate,BreathDeviation,State");
            packetNumber = 0;
        }


        internal void LogEntry(float heartRate, float breathRate, float breathDeviation, RespiratoryState state)
        {
            if (writer == null)
            {
                return;
            }

            packetNumber++;
            writer.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0:O},{1},{2:F2},{3:F2},{4:F4},{5}",
                DateTime.Now,
                packetNumber,
                heartRate,
                breathRate,
                breathDeviation,
                state));
        }


        internal void EndSession()
        {
            if (writer != null)
            {
                try
                {
                    writer.Flush();
                    writer.Close();
                }
                catch
                {
                    // Best-effort close
                }
                finally
                {
                    writer.Dispose();
                    writer = null;
                }
            }
        }

        public void Dispose()
        {
            EndSession();
        }
    }
}
