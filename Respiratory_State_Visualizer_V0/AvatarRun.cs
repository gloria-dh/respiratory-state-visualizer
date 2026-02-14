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
        private const float HoldingBreathThresholdBpm = 3.0f;
        private const float HyperventilatingThresholdBpm = 20.0f;
        private const int MinAnimationIntervalMs = 120;
        private const int MaxAnimationIntervalMs = 2000;

        // Objects
        private AvatarProfile currentProfile = new AvatarProfile();
        private AvatarState currentState = new AvatarState();
        private readonly RadarVitalsReader vitalsReader = new RadarVitalsReader();
        private bool isReadingSensor;

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
            currentState.displayState = State.calm;
            lblSensorStatusValue.Text = "Sensor: Idle";
            lblHeartRateValue.Text = "Heart Rate: -- bpm";
            lblBreathRateValue.Text = "Breath Rate: -- bpm";

            vitalsReader.VitalsReceived += VitalsReader_VitalsReceived;
            vitalsReader.StatusChanged += message => UpdateSensorStatus(message, false);
            vitalsReader.ErrorOccurred += message => UpdateSensorError(message);
            Disposed += AvatarRun_Disposed;



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

        internal void SetCompactMode(bool compact)
        {
            if (compact)
            {
                tableLayoutPanel1.RowStyles[0].SizeType = SizeType.Percent;
                tableLayoutPanel1.RowStyles[0].Height = 100f;
                tableLayoutPanel1.RowStyles[1].SizeType = SizeType.Absolute;
                tableLayoutPanel1.RowStyles[1].Height = 0f;
                tableLayoutPanel1.RowStyles[2].SizeType = SizeType.Absolute;
                tableLayoutPanel1.RowStyles[2].Height = 0f;
                toolStrip1.Visible = false;
                panel1.Visible = false;
            }
            else
            {
                tableLayoutPanel1.RowStyles[0].SizeType = SizeType.Absolute;
                tableLayoutPanel1.RowStyles[0].Height = 512f;
                tableLayoutPanel1.RowStyles[1].SizeType = SizeType.Absolute;
                tableLayoutPanel1.RowStyles[1].Height = 35f;
                tableLayoutPanel1.RowStyles[2].SizeType = SizeType.Percent;
                tableLayoutPanel1.RowStyles[2].Height = 100f;
                toolStrip1.Visible = true;
                panel1.Visible = true;
            }

            tableLayoutPanel1.PerformLayout();
        }

        internal void ApplyVitalSigns(float heartRateBpm, float breathingRateBpm)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ApplyVitalSigns(heartRateBpm, breathingRateBpm)));
                return;
            }

            if (float.IsNaN(breathingRateBpm) || float.IsInfinity(breathingRateBpm) || breathingRateBpm <= 0.0f)
            {
                return;
            }

            if (breathingRateBpm <= HoldingBreathThresholdBpm)
            {
                currentState.displayState = State.holding_breath;
            }
            else if (breathingRateBpm >= HyperventilatingThresholdBpm)
            {
                currentState.displayState = State.hyperventilating;
            }
            else
            {
                currentState.displayState = State.calm;
            }

            generalTimer.Interval = CalculateAnimationIntervalMs(breathingRateBpm);
        }

        private int CalculateAnimationIntervalMs(float breathingRateBpm)
        {
            float halfBreathMs = 30000.0f / breathingRateBpm;
            int interval = (int)Math.Round(halfBreathMs);
            return Math.Max(MinAnimationIntervalMs, Math.Min(MaxAnimationIntervalMs, interval));
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

        private void btnReadSensor_Click(object sender, EventArgs e)
        {
            if (isReadingSensor)
            {
                StopSensorReading();
                return;
            }

            try
            {
                UpdateSensorStatus("Starting sensor stream...", false);
                vitalsReader.Start(
                    SensorSetupSettings.CliPort,
                    SensorSetupSettings.DataPort,
                    SensorSetupSettings.ConfigFilePath);

                isReadingSensor = true;
                btnReadSensor.Text = "STOP SENSOR";
            }
            catch (Exception ex)
            {
                UpdateSensorStatus($"Sensor error: {ex.Message}", true);
                MessageBox.Show(this, ex.Message, "Sensor Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopSensorReading()
        {
            vitalsReader.Stop();
            isReadingSensor = false;
            btnReadSensor.Text = "READ SENSOR";
            UpdateSensorStatus("Sensor stopped.", false);
        }

        private void VitalsReader_VitalsReceived(float heartRate, float breathRate)
        {
            if (IsDisposed)
            {
                return;
            }

            BeginInvoke(new Action(() =>
            {
                lblHeartRateValue.Text = $"Heart Rate: {heartRate:F2} bpm";
                lblBreathRateValue.Text = $"Breath Rate: {breathRate:F2} bpm";
                ApplyVitalSigns(heartRate, breathRate);
            }));
        }

        private void UpdateSensorStatus(string message, bool isError)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateSensorStatus(message, isError)));
                return;
            }

            lblSensorStatusValue.Text = $"Sensor: {message}";
            lblSensorStatusValue.ForeColor = isError ? Color.IndianRed : Color.White;
        }

        private void UpdateSensorError(string message)
        {
            UpdateSensorStatus(message, true);

            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateSensorError(message)));
                return;
            }

            vitalsReader.Stop();
            isReadingSensor = false;
            btnReadSensor.Text = "READ SENSOR";
        }

        private void AvatarRun_Disposed(object sender, EventArgs e)
        {
            vitalsReader.Dispose();
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

        private void displayCalm()
        {
            setFace(Properties.Resources.face_calm);
            setBreath(null);

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
