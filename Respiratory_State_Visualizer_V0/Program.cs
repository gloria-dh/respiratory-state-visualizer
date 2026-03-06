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

            // Launch the main application (splash is shown inside MainForm)
            Application.Run(new MainForm());
        }
    }
}
