using System;
using System.Drawing;
using System.Windows.Forms;

namespace Respiratory_State_Visualizer_V0
{
    internal class HistoryPage : UserControl
    {
        private Label lblTitle;

        internal HistoryPage()
        {
            InitializeUi();
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

            Panel container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                BackColor = AppTheme.SlateGray
            };

            lblTitle = new Label
            {
                AutoSize = true,
                Text = "Session History",
                Font = new Font("Microsoft Sans Serif", 14f, FontStyle.Bold),
                ForeColor = AppTheme.Orange,
                Margin = new Padding(0, 0, 0, 14)
            };

            container.Controls.Add(lblTitle);
            root.Controls.Add(container, 0, 0);
            Controls.Add(root);
        }
    }
}
