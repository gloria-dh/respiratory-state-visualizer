namespace Respiratory_State_Visualizer_V0
{
    internal static class SensorSetupSettings
    {
        internal static string CliPort { get; set; } = "COM5";
        internal static string DataPort { get; set; } = "COM6";
        internal static string ConfigFilePath { get; set; } =
            @"C:\ti\radar_toolbox_3_30_00_06\source\ti\examples\Industrial_and_Personal_Electronics\Vital_Signs\Vital_Signs_With_People_Tracking\chirp_configs\vital_signs_AOP_2m.cfg";
    }
}
