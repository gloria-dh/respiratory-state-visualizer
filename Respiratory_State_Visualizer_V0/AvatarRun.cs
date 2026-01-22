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
using System.Reflection;

namespace Respiratory_State_Visualizer_V0
{
    public partial class AvatarRun : UserControl
    {
        // Objects
        private AvatarProfile currentProfile = new AvatarProfile();
        private AvatarState currentState = new AvatarState();

        // IMAGES
        private Image mainOutline; // Fixed     
        private Image hair; // Fixed  
        private Image skinTone; // Fixed  
        private Image clothing; // Fixed  
        private Image accesories; // Fixed  
        private Image face;
        private Image chestLevel;
        private Image breath;

        // TIMERS
        private Timer generalTimer;

        // chest level
        private Breathing breathing { get; set; }

        private enum Breathing { @out, @in }

        public AvatarRun()
        {
            InitializeComponent();


            pnlAvatarRun.Paint += pnlAvatarCustomization_Paint;

            tableLayoutPanel1.BackColor = MainForm.SlateGray;
            // lblCustomize.ForeColor = MainForm.Orange;

            generalTimer = new Timer();
            generalTimer.Interval = 600;
            generalTimer.Tick += updateDisplayState;
            generalTimer.Start();

            EnableDoubleBuffering(pnlAvatarRun);



        }

        private void EnableDoubleBuffering(Control c)
        {
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(c, true, null);
        }

        // Drawing layers in order in the panel
        private void pnlAvatarCustomization_Paint(object sender, PaintEventArgs e)
        {
            Rectangle r = pnlAvatarRun.ClientRectangle;

            // Bottom -> top
            if (skinTone != null) e.Graphics.DrawImage(skinTone, r);
            if (clothing != null) e.Graphics.DrawImage(clothing, r);
            if (mainOutline != null) e.Graphics.DrawImage(mainOutline, r);
            if (face != null) e.Graphics.DrawImage(face, r);
            if (hair != null) e.Graphics.DrawImage(hair, r);
            if (accesories != null) e.Graphics.DrawImage(accesories, r);
            if (chestLevel != null) e.Graphics.DrawImage(chestLevel, r);
            if (breath != null) e.Graphics.DrawImage(breath, r);
        }

        // Setters for customization buttons
        internal void setSkin(Image img) { skinTone = img; pnlAvatarRun.Invalidate(); }
        internal void setClothing(Image img) { clothing = img; pnlAvatarRun.Invalidate(); }
        internal void setOutline(Image img) { mainOutline = img; pnlAvatarRun.Invalidate(); }
        internal void setFace(Image img) { face = img; pnlAvatarRun.Invalidate(); }
        internal void setHair(Image img) { hair = img; pnlAvatarRun.Invalidate(); }
        internal void setAccesories(Image img) { accesories = img; pnlAvatarRun.Invalidate(); }
        internal void setChestLevel(Image img) { chestLevel = img; pnlAvatarRun.Invalidate(); }
        internal void setBreath(Image img) { breath = img; pnlAvatarRun.Invalidate(); }

        internal void setAvatarProfile(AvatarProfile profile)
        {
            currentProfile = profile;
            addDefaultFeatures();
            updateUI();
        }

        private void addDefaultFeatures()
        {
            setOutline(Properties.Resources.main_outline);
            setFace(Properties.Resources.face_calm);
        }

        private void updateUI()
        {
            // Skin tone selection
            switch (currentProfile.SkinTone) 
            {
                case SkinToneChoice.skin_1:
                    setSkin(Properties.Resources.skin_1);
                    break;
                case SkinToneChoice.skin_2:
                    setSkin(Properties.Resources.skin_2);
                    break;
                case SkinToneChoice.skin_3:
                    setSkin(Properties.Resources.skin_3);
                    break;
                case SkinToneChoice.skin_4:
                    setSkin(Properties.Resources.skin_4);
                    break;
                default:
                    // Add case
                    break;
            }

            // Clothing selection
            switch (currentProfile.Clothing)
            {
                case ClothingChoice.clothing_1:
                    setClothing(Properties.Resources.clothing_1);
                    break;
                case ClothingChoice.clothing_2:
                    setClothing(Properties.Resources.clothing_2);
                    break;
                default:
                    // Add case
                    break;
            }

            // Hair selection
            switch (currentProfile.Hair) 
            {
                case HairChoice.None: // None
                    setHair(null);
                    break;
                case HairChoice.long_black: // Black
                    setHair(Properties.Resources.hair_long_black);
                    break;
                case HairChoice.medium_black:
                    setHair(Properties.Resources.hair_medium_black);
                    break;
                case HairChoice.short_black:
                    setHair(Properties.Resources.hair_short_black);
                    break;
                case HairChoice.long_brown: // Brown
                    setHair(Properties.Resources.hair_long_brown);
                    break;
                case HairChoice.medium_brown:
                    setHair(Properties.Resources.hair_medium_brown);
                    break;
                case HairChoice.short_brown:
                    setHair(Properties.Resources.hair_short_brown);
                    break;
                case HairChoice.long_blonde: // Blonde
                    setHair(Properties.Resources.hair_long_blonde);
                    break;
                case HairChoice.medium_blonde:
                    setHair(Properties.Resources.hair_medium_blonde);
                    break;
                case HairChoice.short_blonde:
                    setHair(Properties.Resources.hair_short_blonde);
                    break;
                case HairChoice.long_red: // Red
                    setHair(Properties.Resources.hair_long_red);
                    break;
                case HairChoice.medium_red:
                    setHair(Properties.Resources.hair_medium_red);
                    break;
                case HairChoice.short_red:
                    setHair(Properties.Resources.hair_short_red);
                    break;
                default:
                    // Add case
                    break;
            }

            // Accessories selection
            switch (currentProfile.Accessories) 
            {
                case AccessoryChoice.None:
                    setAccesories(null);
                    break;
                case AccessoryChoice.headphones:
                    setAccesories(Properties.Resources.Accessories_headphones);
                    break;
            }
        }

        private void tsbStartStop_Click(object sender, EventArgs e)
        {
            currentState.displayState = State.calm;
        }

        private void updateDisplayState(object sender, EventArgs e)
        {
            switch (currentState.displayState)
            {
                case State.calm:
                    displayCalm();
                    break;
                case State.holding_breath:
                    displayHoldingBreath();
                    break;
                case State.hyperventilating:
                    displayHyperventilating();
                    break;
                default:
                    // Add Case
                    break;
            }
        }

        private bool skipNextTick = false;

        private void displayCalm()
        {
            setFace(Properties.Resources.face_calm);
            setBreath(null);

            skipNextTick = !skipNextTick;
            if (skipNextTick)
            {
                return;
            }

            updateBreathingState();
            toggleChest();
            
        }

        private void updateBreathingState()
        {
            if (breathing == Breathing.@in)
            {
                breathing = Breathing.@out;
            }
            else
            {
                breathing = Breathing.@in;
            }
        }

        private void toggleFace()
        {
            if (breathing == Breathing.@in)
            {
                setFace(Properties.Resources.face_hyperventilating_high);
            }
            else
            {
                setFace(Properties.Resources.face_hyperventilating_low);
            }
        }

        private void toggleBreath()
        {
            if (breathing == Breathing.@in) 
            {
                setBreath(null);
            }
            else
            {
                setBreath(Properties.Resources.breath_out);
            }
        }

        private void toggleChest()
        {
            if (breathing == Breathing.@in)
            {
                setChestLevel(Properties.Resources.chest_level_high);
            }
            else
            {
                setChestLevel(Properties.Resources.chest_level_low);
            }
        }
        private void displayHoldingBreath()
        {
            setBreath(null);
            setFace(Properties.Resources.face_holding_breath);
            breathing = Breathing.@out;
            setChestLevel(Properties.Resources.chest_level_low);
        }

        private void displayHyperventilating()
        {
            updateBreathingState();
            toggleChest();
            toggleBreath();
            toggleFace();
        }

        private void btnCalm_Click(object sender, EventArgs e)
        {
            currentState.displayState = State.calm;
        }

        private void btnHoldingBreath_Click(object sender, EventArgs e)
        {
            currentState.displayState = State.holding_breath;
        }

        private void btnHyperventilating_Click(object sender, EventArgs e)
        {
            currentState.displayState = State.hyperventilating;
        }
    }
}
