namespace Respiratory_State_Visualizer_V0
{
    internal static class SensorSetupSettings
    {
        internal static string CliPort { get; set; } = "COM3";
        internal static string DataPort { get; set; } = "COM4";
        internal static string ConfigFilePath { get; set; } =
            System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, ".config", "vital_signs_AOP_2m-custom.cfg");
        internal static string PythonScriptPath { get; set; } =
            System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "scripts", "sensorPipeline.py");
    }
}
