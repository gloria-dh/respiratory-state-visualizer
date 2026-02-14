namespace Respiratory_State_Visualizer_V0
{
    partial class AvatarRun
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.pnlAvatarRun = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbStartStop = new System.Windows.Forms.ToolStripButton();
            this.tslStartStop = new System.Windows.Forms.ToolStripLabel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblBreathRateValue = new System.Windows.Forms.Label();
            this.lblHeartRateValue = new System.Windows.Forms.Label();
            this.lblSensorStatusValue = new System.Windows.Forms.Label();
            this.btnReadSensor = new System.Windows.Forms.Button();
            this.btnHyperventilating = new System.Windows.Forms.Button();
            this.btnHoldingBreath = new System.Windows.Forms.Button();
            this.btnCalm = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlAvatarRun
            // 
            this.pnlAvatarRun.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAvatarRun.Location = new System.Drawing.Point(370, 3);
            this.pnlAvatarRun.Name = "pnlAvatarRun";
            this.pnlAvatarRun.Size = new System.Drawing.Size(506, 500);
            this.pnlAvatarRun.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbStartStop,
            this.tslStartStop});
            this.toolStrip1.Location = new System.Drawing.Point(0, 512);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1252, 35);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbStartStop
            // 
            this.tsbStartStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbStartStop.Image = global::Respiratory_State_Visualizer_V0.Properties.Resources.icon_start_button;
            this.tsbStartStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbStartStop.Name = "tsbStartStop";
            this.tsbStartStop.Size = new System.Drawing.Size(29, 32);
            this.tsbStartStop.Text = "toolStripButton1";
            this.tsbStartStop.Click += new System.EventHandler(this.tsbStartStop_Click);
            // 
            // tslStartStop
            // 
            this.tslStartStop.Name = "tslStartStop";
            this.tslStartStop.Size = new System.Drawing.Size(50, 32);
            this.tslStartStop.Text = "START";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 512F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.pnlAvatarRun, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1246, 506);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panel1.Controls.Add(this.lblBreathRateValue);
            this.panel1.Controls.Add(this.lblHeartRateValue);
            this.panel1.Controls.Add(this.lblSensorStatusValue);
            this.panel1.Controls.Add(this.btnReadSensor);
            this.panel1.Controls.Add(this.btnHyperventilating);
            this.panel1.Controls.Add(this.btnHoldingBreath);
            this.panel1.Controls.Add(this.btnCalm);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 550);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1246, 248);
            this.panel1.TabIndex = 2;
            // 
            // lblBreathRateValue
            // 
            this.lblBreathRateValue.AutoSize = true;
            this.lblBreathRateValue.ForeColor = System.Drawing.Color.White;
            this.lblBreathRateValue.Location = new System.Drawing.Point(24, 131);
            this.lblBreathRateValue.Name = "lblBreathRateValue";
            this.lblBreathRateValue.Size = new System.Drawing.Size(122, 16);
            this.lblBreathRateValue.TabIndex = 6;
            this.lblBreathRateValue.Text = "Breath Rate: -- bpm";
            // 
            // lblHeartRateValue
            // 
            this.lblHeartRateValue.AutoSize = true;
            this.lblHeartRateValue.ForeColor = System.Drawing.Color.White;
            this.lblHeartRateValue.Location = new System.Drawing.Point(24, 105);
            this.lblHeartRateValue.Name = "lblHeartRateValue";
            this.lblHeartRateValue.Size = new System.Drawing.Size(110, 16);
            this.lblHeartRateValue.TabIndex = 5;
            this.lblHeartRateValue.Text = "Heart Rate: -- bpm";
            // 
            // lblSensorStatusValue
            // 
            this.lblSensorStatusValue.AutoSize = true;
            this.lblSensorStatusValue.ForeColor = System.Drawing.Color.White;
            this.lblSensorStatusValue.Location = new System.Drawing.Point(24, 79);
            this.lblSensorStatusValue.Name = "lblSensorStatusValue";
            this.lblSensorStatusValue.Size = new System.Drawing.Size(83, 16);
            this.lblSensorStatusValue.TabIndex = 4;
            this.lblSensorStatusValue.Text = "Sensor: Idle";
            // 
            // btnReadSensor
            // 
            this.btnReadSensor.Location = new System.Drawing.Point(27, 23);
            this.btnReadSensor.Name = "btnReadSensor";
            this.btnReadSensor.Size = new System.Drawing.Size(166, 43);
            this.btnReadSensor.TabIndex = 3;
            this.btnReadSensor.Text = "READ SENSOR";
            this.btnReadSensor.UseVisualStyleBackColor = true;
            this.btnReadSensor.Click += new System.EventHandler(this.btnReadSensor_Click);
            // 
            // btnHyperventilating
            // 
            this.btnHyperventilating.Location = new System.Drawing.Point(629, 79);
            this.btnHyperventilating.Name = "btnHyperventilating";
            this.btnHyperventilating.Size = new System.Drawing.Size(209, 58);
            this.btnHyperventilating.TabIndex = 2;
            this.btnHyperventilating.Text = "HYPERVENTILATING";
            this.btnHyperventilating.UseVisualStyleBackColor = true;
            this.btnHyperventilating.Click += new System.EventHandler(this.btnHyperventilating_Click);
            // 
            // btnHoldingBreath
            // 
            this.btnHoldingBreath.Location = new System.Drawing.Point(414, 79);
            this.btnHoldingBreath.Name = "btnHoldingBreath";
            this.btnHoldingBreath.Size = new System.Drawing.Size(209, 58);
            this.btnHoldingBreath.TabIndex = 1;
            this.btnHoldingBreath.Text = "HOLDING BREATH";
            this.btnHoldingBreath.UseVisualStyleBackColor = true;
            this.btnHoldingBreath.Click += new System.EventHandler(this.btnHoldingBreath_Click);
            // 
            // btnCalm
            // 
            this.btnCalm.Location = new System.Drawing.Point(199, 79);
            this.btnCalm.Name = "btnCalm";
            this.btnCalm.Size = new System.Drawing.Size(209, 58);
            this.btnCalm.TabIndex = 0;
            this.btnCalm.Text = "CALM";
            this.btnCalm.UseVisualStyleBackColor = true;
            this.btnCalm.Click += new System.EventHandler(this.btnCalm_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.LightSlateGray;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 512F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1252, 801);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // AvatarRun
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "AvatarRun";
            this.Size = new System.Drawing.Size(1252, 801);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Panel pnlAvatarRun;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStripButton tsbStartStop;
        private System.Windows.Forms.ToolStripLabel tslStartStop;
        private System.Windows.Forms.Button btnHyperventilating;
        private System.Windows.Forms.Button btnHoldingBreath;
        private System.Windows.Forms.Button btnCalm;
        private System.Windows.Forms.Label lblBreathRateValue;
        private System.Windows.Forms.Label lblHeartRateValue;
        private System.Windows.Forms.Label lblSensorStatusValue;
        private System.Windows.Forms.Button btnReadSensor;
    }
}
