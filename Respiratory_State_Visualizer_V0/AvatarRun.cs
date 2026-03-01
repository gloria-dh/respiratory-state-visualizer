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

        // Objects
        private AvatarProfile currentProfile = new AvatarProfile();
        private AvatarState currentState = new AvatarState();
        private readonly RadarVitalsReader vitalsReader = new RadarVitalsReader();
        private readonly SessionLogger sessionLogger = new SessionLogger();
        private AvatarLayerManager layers;
        private bool isReadingSensor;

        // TIMERS
        private Timer generalTimer;

        // Breathing phase
        private BreathPhase currentBreathPhase;

        private enum BreathPhase { Out, In }

        // ── Lookup tables for avatar appearance ──────────────────────────

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

        // ── Constructor ──────────────────────────────────────────────────

        public AvatarRun()
        {
            InitializeComponent();

            layers = new AvatarLayerManager(pnlAvatarRun);
            pnlAvatarRun.Paint += PnlAvatarRun_Paint;

            tableLayoutPanel1.BackColor = AppTheme.SlateGray;

            generalTimer = new Timer();
            generalTimer.Interval = 600;
            generalTimer.Tick += UpdateDisplayState;
            generalTimer.Start();

            EnableDoubleBuffering(pnlAvatarRun);
            currentState.DisplayState = RespiratoryState.Neutral;
            lblSensorStatusValue.Text = "Sensor: Idle";
            lblHeartRateValue.Text = "Heart Rate: -- bpm";
            lblBreathRateValue.Text = "Breath Rate: -- bpm";
            lblDeviationValue.Text = "Deviation: --";
            lblStateValue.Text = "State: --";

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
            layers.PaintLayers(e.Graphics, pnlAvatarRun.ClientRectangle);
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

        // ── Avatar appearance (dictionary lookups) ───────────────────────

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

        // ── Sensor ───────────────────────────────────────────────────────

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
                UpdateSensorStatus("Starting sensor stream...", false);
                sessionLogger.StartSession();
                vitalsReader.Start(
                    SensorSetupSettings.CliPort,
                    SensorSetupSettings.DataPort,
                    SensorSetupSettings.ConfigFilePath);

                isReadingSensor = true;
                btnReadSensor.Text = "STOP SENSOR";
            }
            catch (Exception ex)
            {
                sessionLogger.EndSession();
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
            UpdateSensorStatus("Sensor stopped.", false);
        }

        /// <summary>
        /// Stops the sensor if it is currently reading. Called by MainForm when
        /// the user navigates away from the Run tab.
        /// </summary>
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

            // Log to CSV (thread-safe via StreamWriter with AutoFlush)
            sessionLogger.LogEntry(heartRate, breathRate, breathDeviation, state);

            BeginInvoke(new Action(() =>
            {
                lblHeartRateValue.Text = $"Heart Rate: {heartRate:F2} bpm";
                lblBreathRateValue.Text = $"Breath Rate: {breathRate:F2} bpm";
                lblDeviationValue.Text = $"Deviation: {breathDeviation:F4}";
                lblStateValue.Text = $"State: {state}";
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

            lblSensorStatusValue.Text = $"Sensor: {message}";
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
        }

        private void AvatarRun_Disposed(object sender, EventArgs e)
        {
            vitalsReader.Dispose();
            sessionLogger.Dispose();
        }

        // ── Display state animation ─────────────────────────────────────

        private void UpdateDisplayState(object sender, EventArgs e)
        {
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

            ToggleBreathPhase();
            ToggleChest();
        }

        private void DisplayStrained()
        {
            layers.SetBreath(null);
            layers.SetFace(Properties.Resources.face_calm);
            currentBreathPhase = BreathPhase.Out;
            layers.SetChestLevel(Properties.Resources.chest_level_low);
        }

        private void DisplayHoldingBreath()
        {
            layers.SetBreath(null);
            layers.SetFace(Properties.Resources.face_holding_breath);
            currentBreathPhase = BreathPhase.Out;
            layers.SetChestLevel(Properties.Resources.chest_level_low);
        }

        private void DisplayRecovering()
        {
            layers.SetBreath(null);

            ToggleBreathPhase();
            ToggleChest();
            ToggleFace();
        }

        private void DisplayAlert()
        {
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

        // ── Debug buttons ───────────────────────────────────────────────

        private void btnNeutral_Click(object sender, EventArgs e)
        {
            currentState.DisplayState = RespiratoryState.Neutral;
            lblStateValue.Text = "State: Neutral";
        }

        private void btnStrained_Click(object sender, EventArgs e)
        {
            currentState.DisplayState = RespiratoryState.Strained;
            lblStateValue.Text = "State: Strained";
        }

        private void btnHoldingBreath_Click(object sender, EventArgs e)
        {
            currentState.DisplayState = RespiratoryState.HoldingBreath;
            lblStateValue.Text = "State: HoldingBreath";
        }

        private void btnRecovering_Click(object sender, EventArgs e)
        {
            currentState.DisplayState = RespiratoryState.Recovering;
            lblStateValue.Text = "State: Recovering";
        }

        private void btnAlert_Click(object sender, EventArgs e)
        {
            currentState.DisplayState = RespiratoryState.Alert;
            lblStateValue.Text = "State: Alert";
        }
    }
}
