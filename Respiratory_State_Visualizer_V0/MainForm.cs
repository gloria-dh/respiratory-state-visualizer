using System;
using System.Drawing;
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
        }

        // UPDATE AVATAR PROFILE FROM CUSTOMIZE UI EVENT
        private void AvatarCustomize_AvatarSaved(AvatarProfile profile)
        {
            currentProfile = profile;
            runPage.SetAvatarProfile(currentProfile);
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
