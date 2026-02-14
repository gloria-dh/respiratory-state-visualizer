using Respiratory_State_Visualizer_V0.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * Main Color Pallete: (119, 136, 153) - Light Slate Gray
*/

namespace Respiratory_State_Visualizer_V0
{
    public partial class MainForm : Form
    {
        // Color pallette
        public static readonly Color SlateGray = Color.FromArgb(87, 87, 85);
        public static readonly Color Orange = Color.FromArgb(255, 207, 94);
        public static readonly Color Black = Color.FromArgb(0, 0, 0);

        // Page states
        public static bool setupPageOn, customizePageOn, runPageOn; 

        // User control objects
        private SetupPage setupPage = new SetupPage();
        private AvatarCustomize customizePage = new AvatarCustomize();
        private AvatarRun runPage = new AvatarRun();

        // Objects
        private AvatarProfile currentProfile = new AvatarProfile();


        public MainForm()
        {
            InitializeComponent();

            // Pages on start
            setupPageOn = true;
            customizePageOn = false;
            runPageOn = false;

            tableLayoutPanel1.BackColor = SlateGray;
            tableLayoutPanel2.BackColor = SlateGray;
            pnlMainDock.BackColor = SlateGray;
            tableLayoutPanel3.BackColor = Orange;

            btnStyleReset();
            btnOnSetupPage();

            customizePage.AvatarSaved += AvatarCustomize_AvatarSaved;
            customizePage.PullCurrentProfile();
            runPage.setAvatarProfile(currentProfile);
            showSetupPage();
        }

        // UPDATE AVATAR PROFILE FROM CUSTOMIZE UI EVENT
        private void AvatarCustomize_AvatarSaved(AvatarProfile profile)
        {
            currentProfile = profile;
            runPage.setAvatarProfile(currentProfile);
        }

        // BUTTON STYLE AND BACKGROUND COLOUR

        public void btnStyleReset()
        {
            // Background color reset
            btnSetup.BackColor = Orange;
            btnCustomizeAvatar.BackColor = Orange;
            btnRun.BackColor = Orange;

            // Text color reset
            btnSetup.ForeColor = Black;
            btnCustomizeAvatar.ForeColor = Black;
            btnRun.ForeColor = Black;
        }

        public void btnOnSetupPage()
        {
            btnStyleReset();
            btnSetup.BackColor = SlateGray;
            btnSetup.ForeColor = Orange;
        }

        public void btnOnCustomizePage()
        {
            btnStyleReset();
            btnCustomizeAvatar.BackColor = SlateGray;
            btnCustomizeAvatar.ForeColor = Orange;
        }

        public void btnOnRunPage()
        {
            btnStyleReset();
            btnRun.BackColor = SlateGray;
            btnRun.ForeColor = Orange;
        }


        // TAB SWITCHING

        private void clearMainDock()
        {
            pnlMainDock.Controls.Clear();
        }

        private void showSetupPage()
        {
            clearMainDock();
            setupPage.Dock = DockStyle.Fill;
            pnlMainDock.Controls.Add(setupPage);
        }

        private void showCustomizePage()
        {
            clearMainDock();
            customizePage.Dock = DockStyle.Fill;
            pnlMainDock.Controls.Add(customizePage);
        }

        private void showRunPage()
        {
            clearMainDock();
            runPage.Dock = DockStyle.Fill;
            pnlMainDock.Controls.Add(runPage);
            runPage.setAvatarProfile(currentProfile);
        }

        // EVENTS

        private void btnSetup_Click(object sender, EventArgs e)
        {
            btnOnSetupPage();
            showSetupPage();
        }

        private void btnCustomizeAvatar_Click(object sender, EventArgs e)
        {
            btnOnCustomizePage();
            showCustomizePage();

        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            btnOnRunPage();
            showRunPage();
        }
    }
}
