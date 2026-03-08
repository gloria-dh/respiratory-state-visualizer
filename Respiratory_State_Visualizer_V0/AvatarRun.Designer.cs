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
            this.pnlAboveAvatar = new System.Windows.Forms.Panel();
            this.lblDisplayState = new System.Windows.Forms.Label();
            this.lblDisplayHR = new System.Windows.Forms.Label();
            this.lblDisplayBPM = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblSensorStatusValue = new System.Windows.Forms.Label();
            this.btnReadSensor = new System.Windows.Forms.Button();
            this.chkSwitchReset = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.pnlAboveAvatar.SuspendLayout();
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
            this.tableLayoutPanel2.Controls.Add(this.pnlAboveAvatar, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.pnlAvatarRun, 1, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1246, 506);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // pnlAboveAvatar
            // 
            this.pnlAboveAvatar.Controls.Add(this.lblDisplayState);
            this.pnlAboveAvatar.Controls.Add(this.lblDisplayHR);
            this.pnlAboveAvatar.Controls.Add(this.lblDisplayBPM);
            this.pnlAboveAvatar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAboveAvatar.Name = "pnlAboveAvatar";
            this.pnlAboveAvatar.TabIndex = 1;
            // 
            // lblDisplayState
            // 
            this.lblDisplayState.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblDisplayState.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblDisplayState.ForeColor = System.Drawing.Color.White;
            this.lblDisplayState.Name = "lblDisplayState";
            this.lblDisplayState.Size = new System.Drawing.Size(512, 28);
            this.lblDisplayState.TabIndex = 0;
            this.lblDisplayState.Text = "";
            this.lblDisplayState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDisplayHR
            // 
            this.lblDisplayHR.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblDisplayHR.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular);
            this.lblDisplayHR.ForeColor = System.Drawing.Color.White;
            this.lblDisplayHR.Name = "lblDisplayHR";
            this.lblDisplayHR.Size = new System.Drawing.Size(512, 24);
            this.lblDisplayHR.TabIndex = 1;
            this.lblDisplayHR.Text = "Heart Rate:";
            this.lblDisplayHR.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDisplayBPM
            // 
            this.lblDisplayBPM.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblDisplayBPM.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular);
            this.lblDisplayBPM.ForeColor = System.Drawing.Color.White;
            this.lblDisplayBPM.Name = "lblDisplayBPM";
            this.lblDisplayBPM.Size = new System.Drawing.Size(512, 24);
            this.lblDisplayBPM.TabIndex = 2;
            this.lblDisplayBPM.Text = "Breath Rate:";
            this.lblDisplayBPM.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panel1.Controls.Add(this.lblSensorStatusValue);
            this.panel1.Controls.Add(this.chkSwitchReset);
            this.panel1.Controls.Add(this.btnReadSensor);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 550);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1246, 240);
            this.panel1.TabIndex = 2;
            // 
            // lblSensorStatusValue
            // 
            this.lblSensorStatusValue.AutoSize = true;
            this.lblSensorStatusValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSensorStatusValue.ForeColor = System.Drawing.Color.White;
            this.lblSensorStatusValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSensorStatusValue.Location = new System.Drawing.Point(12, 210);
            this.lblSensorStatusValue.Name = "lblSensorStatusValue";
            this.lblSensorStatusValue.Size = new System.Drawing.Size(83, 20);
            this.lblSensorStatusValue.TabIndex = 4;
            this.lblSensorStatusValue.Text = "Sensor Status: Idle";
            // 
            // btnReadSensor
            // 
            this.btnReadSensor.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnReadSensor.Location = new System.Drawing.Point(540, 60);
            this.btnReadSensor.Name = "btnReadSensor";
            this.btnReadSensor.Size = new System.Drawing.Size(166, 43);
            this.btnReadSensor.TabIndex = 3;
            this.btnReadSensor.Text = "READ SENSOR";
            this.btnReadSensor.UseVisualStyleBackColor = true;
            this.btnReadSensor.Click += new System.EventHandler(this.btnReadSensor_Click);
            // 
            // chkSwitchReset
            // 
            this.chkSwitchReset.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.chkSwitchReset.AutoSize = true;
            this.chkSwitchReset.ForeColor = System.Drawing.Color.Orange;
            this.chkSwitchReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.chkSwitchReset.Location = new System.Drawing.Point(498, 30);
            this.chkSwitchReset.Name = "chkSwitchReset";
            this.chkSwitchReset.Size = new System.Drawing.Size(250, 20);
            this.chkSwitchReset.TabIndex = 9;
            this.chkSwitchReset.Text = "Sensor switch has been reset";
            this.chkSwitchReset.UseVisualStyleBackColor = true;
            this.chkSwitchReset.CheckedChanged += new System.EventHandler(this.chkSwitchReset_CheckedChanged);
            // 
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
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 240F));
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
            this.pnlAboveAvatar.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Panel pnlAvatarRun;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel pnlAboveAvatar;
        private System.Windows.Forms.Label lblDisplayState;
        private System.Windows.Forms.Label lblDisplayHR;
        private System.Windows.Forms.Label lblDisplayBPM;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStripButton tsbStartStop;
        private System.Windows.Forms.ToolStripLabel tslStartStop;
        private System.Windows.Forms.Label lblSensorStatusValue;
        private System.Windows.Forms.Button btnReadSensor;
        private System.Windows.Forms.CheckBox chkSwitchReset;
    }
}
