using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Respiratory_State_Visualizer_V0
{
    public partial class AvatarRun : UserControl
    {
        private const int MinAnimationIntervalMs = 120;
        private const int MaxAnimationIntervalMs = 2000;
        private int strainedTickCounter = 0;
        private int strainedStage = 1;
        private RespiratoryState previousDisplayState;

        // Objects
        private AvatarProfile currentProfile = new AvatarProfile();
        private AvatarState currentState = new AvatarState();
        private readonly RadarVitalsReader vitalsReader = new RadarVitalsReader();
        private readonly SessionLogger sessionLogger = new SessionLogger();
        private AvatarLayerManager layers;
        private bool isReadingSensor;

        // Calibration / startup gating
        private const int CalibrationFrames = 3;
        private int frameCount;
        private bool calibrationComplete;
        private Label lblOverlayMessage;

        // TIMERS
        private Timer generalTimer;

        // Breathing phase
        private BreathPhase currentBreathPhase;

        private enum BreathPhase { Out, In }

        // Lookup tables for avatar appearance

        private static readonly Dictionary<SkinToneChoice, Func<Image>> SkinLookup =
            new Dictionary<SkinToneChoice, Func<Image>>
            {
                { SkinToneChoice.Skin1, () => Properties.Resources.skin_1 },
                { SkinToneChoice.Skin2, () => Properties.Resources.skin_2 },
                { SkinToneChoice.Skin3, () => Properties.Resources.skin_3 },
                { SkinToneChoice.Skin4, () => Properties.Resources.skin_4 },
            };

        private static readonly Dictionary<ClothingChoice, Func<Image>> ClothingLookup =
            new Dictionary<ClothingChoice, Func<Image>>
            {
                { ClothingChoice.Clothing1, () => Properties.Resources.clothing_1 },
                { ClothingChoice.Clothing2, () => Properties.Resources.clothing_2 },
            };

        private static readonly Dictionary<HairChoice, Func<Image>> HairLookup =
            new Dictionary<HairChoice, Func<Image>>
            {
                { HairChoice.LongBlack,    () => Properties.Resources.hair_long_black },
                { HairChoice.MediumBlack,  () => Properties.Resources.hair_medium_black },
                { HairChoice.ShortBlack,   () => Properties.Resources.hair_short_black },
                { HairChoice.LongBrown,    () => Properties.Resources.hair_long_brown },
                { HairChoice.MediumBrown,  () => Properties.Resources.hair_medium_brown },
                { HairChoice.ShortBrown,   () => Properties.Resources.hair_short_brown },
                { HairChoice.LongBlonde,   () => Properties.Resources.hair_long_blonde },
                { HairChoice.MediumBlonde, () => Properties.Resources.hair_medium_blonde },
                { HairChoice.ShortBlonde,  () => Properties.Resources.hair_short_blonde },
                { HairChoice.LongRed,      () => Properties.Resources.hair_long_red },
                { HairChoice.MediumRed,    () => Properties.Resources.hair_medium_red },
                { HairChoice.ShortRed,     () => Properties.Resources.hair_short_red },
            };

        private static readonly Dictionary<AccessoryChoice, Func<Image>> AccessoryLookup =
            new Dictionary<AccessoryChoice, Func<Image>>
            {
                { AccessoryChoice.Headphones, () => Properties.Resources.Accessories_headphones },
            };

        // Constructor

        public AvatarRun()
        {
            InitializeComponent();

            layers = new AvatarLayerManager(pnlAvatarRun);
            pnlAvatarRun.Paint += PnlAvatarRun_Paint;

            // Overlay label for status messages (centred on avatar panel)
            lblOverlayMessage = new Label
            {
                Text = "Press Read Sensor to Begin",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Microsoft Sans Serif", 16f, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            pnlAvatarRun.Controls.Add(lblOverlayMessage);

            // Hide data labels until calibration completes
            lblDisplayState.Visible = false;
            lblDisplayHR.Visible = false;
            lblDisplayBPM.Visible = false;

            tableLayoutPanel1.BackColor = AppTheme.SlateGray;

            generalTimer = new Timer();
            generalTimer.Interval = 600;
            generalTimer.Tick += UpdateDisplayState;
            generalTimer.Start();

            EnableDoubleBuffering(pnlAvatarRun);
            currentState.DisplayState = RespiratoryState.Neutral;
            lblSensorStatusValue.Text = "Sensor Status: Idle";
            btnReadSensor.Enabled = false;

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

        // Drawing layers in order via shared painter
        private void PnlAvatarRun_Paint(object sender, PaintEventArgs e)
        {
            if (calibrationComplete)
            {
                layers.PaintLayers(e.Graphics, pnlAvatarRun.ClientRectangle);
            }
        }

        internal void SetAvatarProfile(AvatarProfile profile)
        {
            currentProfile = profile;
            AddDefaultFeatures();
            UpdateAvatarUI();
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
                tableLayoutPanel1.RowStyles[0].SizeType = SizeType.Percent;
                tableLayoutPanel1.RowStyles[0].Height = 100f;
                tableLayoutPanel1.RowStyles[1].SizeType = SizeType.Absolute;
                tableLayoutPanel1.RowStyles[1].Height = 35f;
                tableLayoutPanel1.RowStyles[2].SizeType = SizeType.Absolute;
                tableLayoutPanel1.RowStyles[2].Height = 240f;
                toolStrip1.Visible = true;
                panel1.Visible = true;
            }

            tableLayoutPanel1.PerformLayout();
        }

        internal void ApplyVitalSigns(float heartRateBpm, float breathingRateBpm, float breathDeviation,
            RespiratoryState state)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                    ApplyVitalSigns(heartRateBpm, breathingRateBpm, breathDeviation, state)));
                return;
            }

            // Store values on state object
            currentState.HeartRate = heartRateBpm;
            currentState.BreathRate = breathingRateBpm;
            currentState.BreathDeviation = breathDeviation;
            currentState.DisplayState = state;

            // Adjust animation speed based on breath rate (use original rate for animation, even if overridden to 0)
            float animationRate = breathingRateBpm > 0.0f ? breathingRateBpm : 12.0f;
            generalTimer.Interval = CalculateAnimationIntervalMs(animationRate);

            // Immediately update the avatar with the new state
            UpdateDisplayState(null, EventArgs.Empty);
        }

        private int CalculateAnimationIntervalMs(float breathingRateBpm)
        {
            float halfBreathMs = 30000.0f / breathingRateBpm;
            int interval = (int)Math.Round(halfBreathMs);
            return Math.Max(MinAnimationIntervalMs, Math.Min(MaxAnimationIntervalMs, interval));
        }

        private void AddDefaultFeatures()
        {
            layers.SetOutline(Properties.Resources.main_outline);
            layers.SetFace(Properties.Resources.face_calm);
        }

        // Avatar appearance (dictionary lookups)

        private void UpdateAvatarUI()
        {
            // Skin tone
            if (SkinLookup.TryGetValue(currentProfile.SkinTone, out var skinFunc))
            {
                layers.SetSkinTone(skinFunc());
            }

            // Clothing
            if (ClothingLookup.TryGetValue(currentProfile.Clothing, out var clothingFunc))
            {
                layers.SetClothing(clothingFunc());
            }

            // Hair
            if (currentProfile.Hair == HairChoice.None)
            {
                layers.SetHair(null);
            }
            else if (HairLookup.TryGetValue(currentProfile.Hair, out var hairFunc))
            {
                layers.SetHair(hairFunc());
            }

            // Accessories
            if (currentProfile.Accessories == AccessoryChoice.None)
            {
                layers.SetAccessories(null);
            }
            else if (AccessoryLookup.TryGetValue(currentProfile.Accessories, out var accFunc))
            {
                layers.SetAccessories(accFunc());
            }
        }

        // Sensor

        private void tsbStartStop_Click(object sender, EventArgs e)
        {
            currentState.DisplayState = RespiratoryState.Neutral;
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
                // Reset calibration state
                frameCount = 0;
                calibrationComplete = false;
                lblOverlayMessage.Text = "Configuring Sensor";
                lblOverlayMessage.Visible = true;
                lblDisplayState.Visible = false;
                lblDisplayHR.Visible = false;
                lblDisplayBPM.Visible = false;
                pnlAvatarRun.Invalidate();

                UpdateSensorStatus("Starting sensor stream...", false);
                sessionLogger.StartSession();
                
                vitalsReader.Start(
                    SensorSetupSettings.CliPort,
                    SensorSetupSettings.DataPort,
                    SensorSetupSettings.ConfigFilePath,
                    SensorSetupSettings.PythonScriptPath);

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
            sessionLogger.EndSession();
            isReadingSensor = false;
            btnReadSensor.Text = "READ SENSOR";
            chkSwitchReset.Checked = false;
            UpdateSensorStatus("Sensor stopped.", false);

            // Reset to idle state
            frameCount = 0;
            calibrationComplete = false;
            lblOverlayMessage.Text = "Press Read Sensor to Begin";
            lblOverlayMessage.Visible = true;
            lblDisplayState.Visible = false;
            lblDisplayHR.Visible = false;
            lblDisplayBPM.Visible = false;
            pnlAvatarRun.Invalidate();
        }

        // Stops the sensor if it's currently reading (called when navigating away from Run tab)
        internal void StopSensorIfRunning()
        {
            if (isReadingSensor)
            {
                StopSensorReading();
            }
        }

        private void VitalsReader_VitalsReceived(float heartRate, float breathRate, float breathDeviation,
            RespiratoryState state)
        {
            if (IsDisposed)
            {
                return;
            }


            BeginInvoke(new Action(() =>
            {
                // Count calibration frames
                frameCount++;

                if (frameCount == 1)
                {
                    // First frame: show avatar, switch overlay to calibrating
                    calibrationComplete = true;
                    lblOverlayMessage.Visible = false;
                    pnlAvatarRun.Invalidate();
                }

                if (frameCount <= CalibrationFrames)
                {
                    // During calibration: show "Calibrating X/3" above avatar
                    lblDisplayState.Text = $"Calibrating {frameCount}/{CalibrationFrames}";
                    lblDisplayState.Visible = true;
                    lblDisplayHR.Visible = false;
                    lblDisplayBPM.Visible = false;
                }
                else if (frameCount == CalibrationFrames + 1)
                {
                    // Calibration just finished: show data labels
                    lblDisplayHR.Visible = true;
                    lblDisplayBPM.Visible = true;
                }

                // Always apply vitals once the avatar is visible
                if (frameCount > CalibrationFrames)
                {
                    lblDisplayState.Text = FormatStateName(state);
                    lblDisplayHR.Text = $"Heart Rate: {heartRate:F0} BPM";
                    lblDisplayBPM.Text = $"Breath Rate: {GetBreathRateRange(state)}";
                }

                sessionLogger.LogEntry(heartRate, breathRate, breathDeviation, state);
                ApplyVitalSigns(heartRate, breathRate, breathDeviation, state);
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

            lblSensorStatusValue.Text = $"Sensor Status: {message}";
            lblSensorStatusValue.ForeColor = isError ? Color.IndianRed : Color.White;
        }

        private void UpdateSensorError(string message)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateSensorError(message)));
                return;
            }

            // Update status label first, then clean up sensor state
            UpdateSensorStatus(message, true);
            vitalsReader.Stop();
            sessionLogger.EndSession();
            isReadingSensor = false;
            btnReadSensor.Text = "READ SENSOR";
            chkSwitchReset.Checked = false;
        }

        private void AvatarRun_Disposed(object sender, EventArgs e)
        {
            vitalsReader.Dispose();
            sessionLogger.Dispose();
        }

        // Display state animation

        private void UpdateDisplayState(object sender, EventArgs e)
        {

            if (previousDisplayState != currentState.DisplayState)
            {
                strainedTickCounter = 0;
                strainedStage = 1;

                previousDisplayState = currentState.DisplayState;
            }

            switch (currentState.DisplayState)
            {
                case RespiratoryState.Neutral:
                    DisplayNeutral();
                    break;
                case RespiratoryState.Strained:
                    DisplayStrained();
                    break;
                case RespiratoryState.HoldingBreath:
                    DisplayHoldingBreath();
                    break;
                case RespiratoryState.Recovering:
                    DisplayRecovering();
                    break;
                case RespiratoryState.Alert:
                    DisplayAlert();
                    break;
                default:
                    break;
            }
        }

        private void DisplayNeutral()
        {
            layers.SetFace(Properties.Resources.face_calm);
            layers.SetBreath(null);
            layers.SetCheeks(null);

            ToggleBreathPhase();
            ToggleChest();
        }

        private void DisplayStrained()
        {
            layers.SetBreath(null);
            layers.SetFace(Properties.Resources.face_hyperventilating_high);
            currentBreathPhase = BreathPhase.Out;
            layers.SetChestLevel(Properties.Resources.chest_level_low);

            strainedTickCounter++;

            int ticksFor4Seconds = 4000 / generalTimer.Interval;

            if (strainedTickCounter >= ticksFor4Seconds)
            {
                strainedStage = 3;
            }
            else if (strainedTickCounter >= ticksFor4Seconds / 2)
            {
                strainedStage = 2;
            }
            else
            {
                strainedStage = 1;
            }

            switch (strainedStage)
            {
                case 1:
                    layers.SetCheeks(Properties.Resources.strained_1);
                    break;

                case 2:
                    layers.SetCheeks(Properties.Resources.strained_2);
                    break;

                case 3:
                    layers.SetCheeks(Properties.Resources.strained_3);
                    break;
            }
        }

        private void DisplayHoldingBreath()
        {
            layers.SetBreath(null);
            layers.SetFace(Properties.Resources.face_holding_breath);
            currentBreathPhase = BreathPhase.Out;
            layers.SetChestLevel(Properties.Resources.chest_level_low);
            layers.SetCheeks(null);
        }

        private void DisplayRecovering()
        {
            layers.SetBreath(null);
            layers.SetCheeks(Properties.Resources.strained_1);

            ToggleBreathPhase();
            ToggleChest();
            ToggleFace();
        }

        private void DisplayAlert()
        {
            layers.SetCheeks(null);
            // Fast breathing — hyperventilating animation
            ToggleBreathPhase();
            ToggleChest();
            ToggleBreath();
            ToggleFace();
        }

        private void ToggleBreathPhase()
        {
            if (currentBreathPhase == BreathPhase.In)
            {
                currentBreathPhase = BreathPhase.Out;
            }
            else
            {
                currentBreathPhase = BreathPhase.In;
            }
        }

        private void ToggleFace()
        {
            if (currentBreathPhase == BreathPhase.In)
            {
                layers.SetFace(Properties.Resources.face_hyperventilating_high);
            }
            else
            {
                layers.SetFace(Properties.Resources.face_hyperventilating_low);
            }
        }

        private void ToggleBreath()
        {
            if (currentBreathPhase == BreathPhase.In)
            {
                layers.SetBreath(null);
            }
            else
            {
                layers.SetBreath(Properties.Resources.breath_out);
            }
        }

        private void ToggleChest()
        {
            if (currentBreathPhase == BreathPhase.In)
            {
                layers.SetChestLevel(Properties.Resources.chest_level_high);
            }
            else
            {
                layers.SetChestLevel(Properties.Resources.chest_level_low);
            }
        }

        private void chkSwitchReset_CheckedChanged(object sender, EventArgs e)
        {
            btnReadSensor.Enabled = chkSwitchReset.Checked && !isReadingSensor;
        }

        // Debug buttons

        private void btnNeutral_Click(object sender, EventArgs e)
        {
            currentState.DisplayState = RespiratoryState.Neutral;
            lblDisplayState.Text = "Neutral";
        }

        private void btnStrained_Click(object sender, EventArgs e)
        {
            currentState.DisplayState = RespiratoryState.Strained;
            lblDisplayState.Text = "Strained";
        }

        private void btnHoldingBreath_Click(object sender, EventArgs e)
        {
            currentState.DisplayState = RespiratoryState.HoldingBreath;
            lblDisplayState.Text = "Holding Breath";
        }

        private void btnRecovering_Click(object sender, EventArgs e)
        {
            currentState.DisplayState = RespiratoryState.Recovering;
            lblDisplayState.Text = "Recovering";
        }

        private void btnAlert_Click(object sender, EventArgs e)
        {
            currentState.DisplayState = RespiratoryState.Alert;
            lblDisplayState.Text = "Alert";
        }

        // Helpers

        private static string FormatStateName(RespiratoryState state)
        {
            switch (state)
            {
                case RespiratoryState.HoldingBreath: return "Holding Breath";
                case RespiratoryState.Recovering:    return "Restoration";
                default:                             return state.ToString();
            }
        }

        private static string GetBreathRateRange(RespiratoryState state)
        {
            switch (state)
            {
                case RespiratoryState.Neutral:       return "10 - 16 BPM";
                case RespiratoryState.Strained:      return "5 - 10 BPM";
                case RespiratoryState.HoldingBreath:  return "<5 BPM";
                case RespiratoryState.Recovering:    return "5 - 10 BPM";
                case RespiratoryState.Alert:          return ">16 BPM";
                default:                             return "-- BPM";
            }
        }
    }
}
