using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Respiratory_State_Visualizer_V0
{
    internal class HistoryPage : UserControl
    {
        private Label lblTitle;
        private ListBox lstSessions;
        private DataGridView dgvLog;
        private Button btnRefresh;
        private Button btnDelete;
        private Button btnOpenFolder;
        private Label lblNoLogs;

        private string logsDir;

        internal HistoryPage()
        {
            logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            InitializeUi();
        }

        private void InitializeUi()
        {
            BackColor = AppTheme.SlateGray;
            Dock = DockStyle.Fill;

            // Root layout
            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppTheme.SlateGray,
                Padding = new Padding(12)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));   // title row
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));   // body row

            // Title
            lblTitle = new Label
            {
                AutoSize = true,
                Text = "Session History",
                Font = new Font("Microsoft Sans Serif", 14f, FontStyle.Bold),
                ForeColor = AppTheme.Orange,
                Anchor = AnchorStyles.Left
            };
            root.Controls.Add(lblTitle, 0, 0);

            // Body split: left list (25%) + right grid (75%)
            TableLayoutPanel body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.SlateGray
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75f));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.Controls.Add(body, 0, 1);

            // Left panel
            TableLayoutPanel leftPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppTheme.SlateGray
            };
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // list
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f)); // buttons

            lstSessions = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 60, 58),
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 9.5f),
                BorderStyle = BorderStyle.None,
                IntegralHeight = false
            };
            lstSessions.SelectedIndexChanged += LstSessions_SelectedIndexChanged;
            leftPanel.Controls.Add(lstSessions, 0, 0);

            // Button bar
            FlowLayoutPanel btnBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = AppTheme.SlateGray,
                Padding = new Padding(0, 4, 0, 0)
            };

            btnRefresh = MakeButton("Refresh");
            btnRefresh.Click += (s, e) => LoadSessionList();

            btnDelete = MakeButton("Delete");
            btnDelete.Click += BtnDelete_Click;

            btnOpenFolder = MakeButton("Open Folder");
            btnOpenFolder.Click += (s, e) =>
            {
                if (Directory.Exists(logsDir))
                    Process.Start("explorer.exe", logsDir);
            };

            btnBar.Controls.Add(btnRefresh);
            btnBar.Controls.Add(btnDelete);
            btnBar.Controls.Add(btnOpenFolder);
            leftPanel.Controls.Add(btnBar, 0, 1);

            body.Controls.Add(leftPanel, 0, 0);

            // Right panel (grid + empty label)
            Panel rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.SlateGray
            };

            dgvLog = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.FromArgb(60, 60, 58),
                GridColor = Color.FromArgb(100, 100, 98),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(60, 60, 58),
                    ForeColor = Color.White,
                    SelectionBackColor = AppTheme.Orange,
                    SelectionForeColor = AppTheme.Black,
                    Font = new Font("Microsoft Sans Serif", 9f)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = AppTheme.Orange,
                    ForeColor = AppTheme.Black,
                    Font = new Font("Microsoft Sans Serif", 9.5f, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                },
                EnableHeadersVisualStyles = false
            };

            lblNoLogs = new Label
            {
                Text = "No session logs found.\nRun a sensor session to create logs.",
                ForeColor = Color.LightGray,
                Font = new Font("Microsoft Sans Serif", 11f),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Visible = false
            };

            rightPanel.Controls.Add(dgvLog);
            rightPanel.Controls.Add(lblNoLogs);
            body.Controls.Add(rightPanel, 1, 0);

            Controls.Add(root);

            // Initial load
            LoadSessionList();
        }

        // Helpers

        private Button MakeButton(string text)
        {
            return new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppTheme.Orange,
                ForeColor = AppTheme.Black,
                Font = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Bold),
                Size = new Size(82, 28),
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
        }

        // Session list

        internal void LoadSessionList()
        {
            lstSessions.Items.Clear();
            dgvLog.DataSource = null;
            dgvLog.Columns.Clear();

            if (!Directory.Exists(logsDir))
            {
                ShowEmptyState(true);
                return;
            }

            string[] files = Directory.GetFiles(logsDir, "session_*.csv")
                                      .OrderByDescending(f => f)
                                      .ToArray();

            if (files.Length == 0)
            {
                ShowEmptyState(true);
                return;
            }

            ShowEmptyState(false);

            foreach (string f in files)
                lstSessions.Items.Add(Path.GetFileName(f));

            lstSessions.SelectedIndex = 0;
        }

        private void ShowEmptyState(bool empty)
        {
            lblNoLogs.Visible = empty;
            dgvLog.Visible = !empty;
        }

        // Events

        private void LstSessions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstSessions.SelectedItem == null) return;

            string filePath = Path.Combine(logsDir, lstSessions.SelectedItem.ToString());
            if (!File.Exists(filePath)) return;

            LoadCsvIntoGrid(filePath);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lstSessions.SelectedItem == null) return;

            string fileName = lstSessions.SelectedItem.ToString();
            DialogResult result = MessageBox.Show(
                $"Delete '{fileName}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                string filePath = Path.Combine(logsDir, fileName);
                try
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not delete file:\n{ex.Message}",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                LoadSessionList();
            }
        }

        // CSV loader

        private void LoadCsvIntoGrid(string csvPath)
        {
            dgvLog.DataSource = null;
            dgvLog.Columns.Clear();

            string[] lines;
            try
            {
                lines = File.ReadAllLines(csvPath);
            }
            catch
            {
                return;
            }

            if (lines.Length < 2) return; // header only or empty

            // Parse header
            string[] headers = lines[0].Split(',');

            // Build columns
            dgvLog.ColumnCount = headers.Length;
            for (int i = 0; i < headers.Length; i++)
                dgvLog.Columns[i].Name = headers[i].Trim();

            // Add rows
            dgvLog.Rows.Clear();
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] cells = line.Split(',');
                dgvLog.Rows.Add(cells);
            }
        }
    }
}
