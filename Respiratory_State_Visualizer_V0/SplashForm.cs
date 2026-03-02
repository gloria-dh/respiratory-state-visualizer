using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Respiratory_State_Visualizer_V0
{
    public class SplashForm : Form
    {
        private Timer closeTimer;
        private PictureBox logoPictureBox;

        public SplashForm()
        {
            // Form settings
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = AppTheme.SlateGray;
            Size = new Size(500, 400);

            // Load the logo
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "LOGO.png");
            Image logo = Image.FromFile(logoPath);

            // Logo display
            logoPictureBox = new PictureBox
            {
                Image = logo,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(300, 300),
                BackColor = Color.Transparent
            };

            // Center the logo in the form
            logoPictureBox.Location = new Point(
                (ClientSize.Width - logoPictureBox.Width) / 2,
                (ClientSize.Height - logoPictureBox.Height) / 2
            );

            Controls.Add(logoPictureBox);

            // 3-second timer to close the splash screen
            closeTimer = new Timer();
            closeTimer.Interval = 3000;
            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                Close();
            };
            closeTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (closeTimer != null) closeTimer.Dispose();
                if (logoPictureBox != null)
                {
                    if (logoPictureBox.Image != null) logoPictureBox.Image.Dispose();
                    logoPictureBox.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
