using System;
using System.Windows.Forms;

namespace Respiratory_State_Visualizer_V0
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show splash screen for 3 seconds
            Application.Run(new SplashForm());

            // Then launch the main application
            Application.Run(new MainForm());
        }
    }
}
