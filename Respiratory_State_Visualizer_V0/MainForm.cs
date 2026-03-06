using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Respiratory_State_Visualizer_V0
{
    public partial class MainForm : Form
    {
        // User control objects
        private SetupPage setupPage = new SetupPage();
        private AvatarCustomize customizePage = new AvatarCustomize();
        private AvatarRun runPage = new AvatarRun();

        // Objects
        private AvatarProfile currentProfile = new AvatarProfile();

        // Splash overlay
        private Panel splashOverlay;
        private Timer splashAnimTimer;
        private Timer splashHoldTimer;
        private PictureBox leftLogoPB;
        private PictureBox rightLogoPB;
        private int leftTargetY;
        private int rightTargetY;
        private const int AnimSpeed = 8; // pixels per tick


        public MainForm()
        {
            InitializeComponent();

            tableLayoutPanel1.BackColor = AppTheme.SlateGray;
            tableLayoutPanel2.BackColor = AppTheme.SlateGray;
            pnlMainDock.BackColor = AppTheme.SlateGray;
            tableLayoutPanel3.BackColor = AppTheme.Orange;

            ResetButtonStyles();
            HighlightSetupButton();

            customizePage.AvatarSaved += AvatarCustomize_AvatarSaved;
            customizePage.PullCurrentProfile();
            runPage.SetAvatarProfile(currentProfile);
            ShowSetupPage();

            // Show splash overlay on top of everything
            ShowSplashOverlay();
        }

        // UPDATE AVATAR PROFILE FROM CUSTOMIZE UI EVENT
        private void AvatarCustomize_AvatarSaved(AvatarProfile profile)
        {
            currentProfile = profile;
            runPage.SetAvatarProfile(currentProfile);
        }

        // SPLASH OVERLAY
        private void ShowSplashOverlay()
        {
            splashOverlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.SlateGray
            };

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string leftPath = Path.Combine(basePath, "Resources", "leftLOGO.png");
            string rightPath = Path.Combine(basePath, "Resources", "rightLOGO.png");

            // Create left logo (starts below the window, will slide up)
            if (File.Exists(leftPath))
            {
                leftLogoPB = new PictureBox
                {
                    Image = Image.FromFile(leftPath),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(306, 306),
                    BackColor = Color.Transparent
                };
                splashOverlay.Controls.Add(leftLogoPB);
            }

            // Create right logo (starts above the window, will slide down)
            if (File.Exists(rightPath))
            {
                rightLogoPB = new PictureBox
                {
                    Image = Image.FromFile(rightPath),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(306, 306),
                    BackColor = Color.Transparent
                };
                splashOverlay.Controls.Add(rightLogoPB);
            }

            // Add overlay on top of all other controls
            Controls.Add(splashOverlay);
            splashOverlay.BringToFront();

            // Position logos at their starting points once the overlay is laid out
            splashOverlay.Layout += SplashOverlay_InitialLayout;

            // Animation timer (~60fps)
            splashAnimTimer = new Timer { Interval = 16 };
            splashAnimTimer.Tick += SplashAnimTimer_Tick;
            splashAnimTimer.Start();
        }

        private void SplashOverlay_InitialLayout(object sender, LayoutEventArgs e)
        {
            // Only run once
            splashOverlay.Layout -= SplashOverlay_InitialLayout;

            int totalWidth = 306 + 306;
            int leftX = (splashOverlay.Width - totalWidth) / 2;
            int rightX = leftX + 306;
            int centerY = (splashOverlay.Height - 306) / 2;

            // Left logo starts below the visible area, slides up
            if (leftLogoPB != null)
                leftLogoPB.Location = new Point(leftX, splashOverlay.Height);

            // Right logo starts above the visible area, slides down
            if (rightLogoPB != null)
                rightLogoPB.Location = new Point(rightX, -306);

            leftTargetY = centerY;
            rightTargetY = centerY;
        }

        private void SplashAnimTimer_Tick(object sender, EventArgs e)
        {
            bool leftDone = true;
            bool rightDone = true;

            // Move left logo upward toward center
            if (leftLogoPB != null)
            {
                if (leftLogoPB.Top > leftTargetY)
                {
                    int newY = leftLogoPB.Top - AnimSpeed;
                    if (newY <= leftTargetY) newY = leftTargetY;
                    leftLogoPB.Top = newY;
                    leftDone = (newY == leftTargetY);
                }
            }

            // Move right logo downward toward center
            if (rightLogoPB != null)
            {
                if (rightLogoPB.Top < rightTargetY)
                {
                    int newY = rightLogoPB.Top + AnimSpeed;
                    if (newY >= rightTargetY) newY = rightTargetY;
                    rightLogoPB.Top = newY;
                    rightDone = (newY == rightTargetY);
                }
            }

            // Both logos have arrived — stop animating, hold for 3 seconds
            if (leftDone && rightDone)
            {
                splashAnimTimer.Stop();
                splashAnimTimer.Dispose();
                splashAnimTimer = null;

                splashHoldTimer = new Timer { Interval = 2000 };
                splashHoldTimer.Tick += (s, args) => RemoveSplashOverlay();
                splashHoldTimer.Start();
            }
        }

        private void RemoveSplashOverlay()
        {
            if (splashHoldTimer != null)
            {
                splashHoldTimer.Stop();
                splashHoldTimer.Dispose();
                splashHoldTimer = null;
            }

            if (splashOverlay != null)
            {
                // Dispose images
                if (leftLogoPB != null && leftLogoPB.Image != null)
                {
                    leftLogoPB.Image.Dispose();
                    leftLogoPB.Image = null;
                }
                if (rightLogoPB != null && rightLogoPB.Image != null)
                {
                    rightLogoPB.Image.Dispose();
                    rightLogoPB.Image = null;
                }

                Controls.Remove(splashOverlay);
                splashOverlay.Dispose();
                splashOverlay = null;
                leftLogoPB = null;
                rightLogoPB = null;
            }
        }

        // BUTTON STYLE AND BACKGROUND COLOUR

        private void ResetButtonStyles()
        {
            // Background color reset
            btnSetup.BackColor = AppTheme.Orange;
            btnCustomizeAvatar.BackColor = AppTheme.Orange;
            btnRun.BackColor = AppTheme.Orange;

            // Text color reset
            btnSetup.ForeColor = AppTheme.Black;
            btnCustomizeAvatar.ForeColor = AppTheme.Black;
            btnRun.ForeColor = AppTheme.Black;
        }

        private void HighlightSetupButton()
        {
            ResetButtonStyles();
            btnSetup.BackColor = AppTheme.SlateGray;
            btnSetup.ForeColor = AppTheme.Orange;
        }

        private void HighlightCustomizeButton()
        {
            ResetButtonStyles();
            btnCustomizeAvatar.BackColor = AppTheme.SlateGray;
            btnCustomizeAvatar.ForeColor = AppTheme.Orange;
        }

        private void HighlightRunButton()
        {
            ResetButtonStyles();
            btnRun.BackColor = AppTheme.SlateGray;
            btnRun.ForeColor = AppTheme.Orange;
        }


        // TAB SWITCHING

        private void ClearMainDock()
        {
            // Stop the sensor if navigating away from the Run page
            if (pnlMainDock.Controls.Contains(runPage))
            {
                runPage.StopSensorIfRunning();
            }

            pnlMainDock.Controls.Clear();
        }

        private void ShowSetupPage()
        {
            ClearMainDock();
            setupPage.Dock = DockStyle.Fill;
            pnlMainDock.Controls.Add(setupPage);
        }

        private void ShowCustomizePage()
        {
            ClearMainDock();
            customizePage.Dock = DockStyle.Fill;
            pnlMainDock.Controls.Add(customizePage);
        }

        private void ShowRunPage()
        {
            ClearMainDock();
            runPage.Dock = DockStyle.Fill;
            pnlMainDock.Controls.Add(runPage);
            runPage.SetAvatarProfile(currentProfile);
        }

        // EVENTS

        private void btnSetup_Click(object sender, EventArgs e)
        {
            HighlightSetupButton();
            ShowSetupPage();
        }

        private void btnCustomizeAvatar_Click(object sender, EventArgs e)
        {
            HighlightCustomizeButton();
            ShowCustomizePage();

        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            HighlightRunButton();
            ShowRunPage();
        }
    }
}
