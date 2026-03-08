using System;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;

namespace Respiratory_State_Visualizer_V0
{
    internal class SetupPage : UserControl
    {
        private ComboBox cmbCliPort;
        private ComboBox cmbDataPort;
        private TextBox txtConfigPath;
        private Button btnRefreshPorts;
        private Button btnBrowseConfig;
        private Label lblStatusValue;

        internal SetupPage()
        {
            InitializeUi();
            LoadSavedSettings();
            RefreshComPorts();
            WireEvents();
        }

        private void InitializeUi()
        {
            BackColor = AppTheme.SlateGray;
            Dock = DockStyle.Fill;

            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1,
                BackColor = AppTheme.SlateGray
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            root.Controls.Add(BuildConfigPanel(), 0, 0);
            Controls.Add(root);
        }

        private Panel BuildConfigPanel()
        {
            Panel container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                BackColor = AppTheme.SlateGray
            };

            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 7
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));

            Label lblTitle = new Label
            {
                AutoSize = true,
                Text = "Application Setup",
                Font = new Font("Microsoft Sans Serif", 14f, FontStyle.Bold),
                ForeColor = AppTheme.Orange,
                Margin = new Padding(0, 0, 0, 14)
            };
            grid.Controls.Add(lblTitle, 0, 0);
            grid.SetColumnSpan(lblTitle, 2);

            cmbCliPort = new ComboBox { Dock = DockStyle.Fill };
            cmbDataPort = new ComboBox { Dock = DockStyle.Fill };
            txtConfigPath = new TextBox { Dock = DockStyle.Fill };

            btnRefreshPorts = new Button
            {
                Text = "Refresh COM Ports",
                Dock = DockStyle.Fill,
                Height = 36
            };
            btnBrowseConfig = new Button
            {
                Text = "Browse Config",
                Dock = DockStyle.Fill,
                Height = 36
            };


            lblStatusValue = CreateValueLabel("Set COM ports and config path, then use RUN > Read Sensor.");

            AddRow(grid, 1, "CLI Port (Enhanced):", cmbCliPort);
            AddRow(grid, 2, "DATA Port (Standard):", cmbDataPort);
            AddRow(grid, 3, "Config File:", txtConfigPath);
            AddRow(grid, 4, "", btnBrowseConfig);
            AddRow(grid, 5, "", btnRefreshPorts);
            AddRow(grid, 6, "Status:", lblStatusValue);

            container.Controls.Add(grid);

            return container;
        }

        private static Label CreateValueLabel(string initialValue)
        {
            return new Label
            {
                AutoSize = true,
                Text = initialValue,
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Bold),
                Margin = new Padding(3, 6, 3, 6)
            };
        }

        private static void AddRow(TableLayoutPanel grid, int row, string title, Control control)
        {
            Label label = new Label
            {
                Text = title,
                AutoSize = true,
                ForeColor = AppTheme.Orange,
                Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Bold),
                Margin = new Padding(0, 6, 0, 6)
            };

            grid.Controls.Add(label, 0, row);
            grid.Controls.Add(control, 1, row);
        }

        private void WireEvents()
        {
            btnRefreshPorts.Click += (sender, args) => RefreshComPorts();
            btnBrowseConfig.Click += btnBrowseConfig_Click;
            cmbCliPort.TextChanged += (sender, args) => SaveSettings();
            cmbDataPort.TextChanged += (sender, args) => SaveSettings();
            txtConfigPath.TextChanged += (sender, args) => SaveSettings();
        }

        private void RefreshComPorts()
        {
            string cliSelection = cmbCliPort.Text;
            string dataSelection = cmbDataPort.Text;
            string[] detectedPorts = SerialPort.GetPortNames();
            string[] allPorts = detectedPorts
                .Concat(new[] { "COM5", "COM6" })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(port => port)
                .ToArray();

            cmbCliPort.Items.Clear();
            cmbDataPort.Items.Clear();

            cmbCliPort.Items.AddRange(allPorts);
            cmbDataPort.Items.AddRange(allPorts);

            if (!string.IsNullOrWhiteSpace(cliSelection))
            {
                cmbCliPort.Text = cliSelection;
            }
            else if (allPorts.Length > 0)
            {
                cmbCliPort.Text = allPorts[0];
            }

            if (!string.IsNullOrWhiteSpace(dataSelection))
            {
                cmbDataPort.Text = dataSelection;
            }
            else if (allPorts.Length > 1)
            {
                cmbDataPort.Text = allPorts[1];
            }

            SaveSettings();
        }

        private void btnBrowseConfig_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "Config files (*.cfg)|*.cfg|All files (*.*)|*.*";
                fileDialog.Title = "Select mmWave Config File";
                fileDialog.FileName = txtConfigPath.Text;

                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtConfigPath.Text = fileDialog.FileName;
                    SaveSettings();
                }
            }
        }



        private void LoadSavedSettings()
        {
            cmbCliPort.Text = SensorSetupSettings.CliPort;
            cmbDataPort.Text = SensorSetupSettings.DataPort;
            txtConfigPath.Text = SensorSetupSettings.ConfigFilePath;
            UpdateStatus("Settings loaded.", false);
        }

        private void SaveSettings()
        {
            SensorSetupSettings.CliPort = cmbCliPort.Text.Trim();
            SensorSetupSettings.DataPort = cmbDataPort.Text.Trim();
            SensorSetupSettings.ConfigFilePath = txtConfigPath.Text.Trim();
            UpdateStatus("Settings saved for RUN tab.", false);
        }

        private void UpdateStatus(string message, bool isError)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateStatus(message, isError)));
                return;
            }

            lblStatusValue.Text = message;
            lblStatusValue.ForeColor = isError ? Color.IndianRed : Color.White;
        }
    }
}
