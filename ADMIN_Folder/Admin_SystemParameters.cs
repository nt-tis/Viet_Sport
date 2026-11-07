using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ADMIN
{
    public partial class Admin_SystemParameters : Form
    {
        private string currentAdminID = "EMP0021"; // TODO: Get from session/login

        public Admin_SystemParameters(string adminID) : this()
        {
            if (!string.IsNullOrWhiteSpace(adminID))
                currentAdminID = adminID;
        }

        public Admin_SystemParameters()
        {
            InitializeComponent();
            // Make scaling and font explicit to match main form
            this.AutoScaleMode = AutoScaleMode.None;
            this.Font = new Font("Segoe UI", 9F);
            this.ClientSize = new Size(1100, 700); // same client size as main form (or desired size)
            this.StartPosition = FormStartPosition.CenterParent;

            // Ensure form load triggers data load
            this.Load += Admin_SystemParameters_Load;

            // Wire Search button
            btnExportReport.Click += btnExportReport_Click;

            // Wire Add button (button18)
            button18.Click += Button18_Click;

            // Wire Delete button (button19)
            button19.Click += Button19_Click;

            // Wire Update button (button20) - Remove this line, it's already in Designer
            // button20.Click += Button20_Click;

            // Apply action button colors to match Admin_AccountEmployees (fixed colors)
            ApplyActionButtonStyles();
        }

        private void ApplyActionButtonStyles()
        {
            // Colors: Create(blue), Add(green), Update(amber), Delete(red)
            Color createBlue = Color.FromArgb(0, 123, 255);
            Color addGreen = Color.FromArgb(40, 167, 69);
            Color updateAmber = Color.FromArgb(255, 193, 7);
            Color deleteRed = Color.FromArgb(220, 53, 69);

            // Helper to apply fixed style
            Action<Button, Color, Color> apply = (btn, bg, fg) =>
            {
                if (btn == null) return;
                btn.UseVisualStyleBackColor = false;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = bg;
                btn.ForeColor = fg;
                // keep color fixed on hover/press
                btn.FlatAppearance.MouseOverBackColor = bg;
                btn.FlatAppearance.MouseDownBackColor = bg;
            };

            apply(btnExportReport, createBlue, Color.White); // Search uses Create blue
            apply(button18, addGreen, Color.White);           // Add
            apply(button20, updateAmber, Color.FromArgb(33,37,41)); // Update (dark text for contrast)
            apply(button19, deleteRed, Color.White);          // Delete
        }

        private void Admin_SystemParameters_Load(object sender, EventArgs e)
        {
            LoadSystemParameters();
        }

        /// <summary>
        /// Load data from stored procedure sp_Admin_Dashboard_ShowAllParams,
        /// clear existing grid and bind the results.
        /// </summary>
        private void LoadSystemParameters()
        {
            try
            {
                // Clear any existing display (columns, rows, datasource)
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();

                // Execute stored procedure and get results
                DataTable dt = DatabaseConnection.ExecuteQuery("EXEC sp_Admin_Dashboard_ShowAllParams");

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("No system parameters found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Bind the returned table directly so grid columns match SP result
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load system parameters: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Search button click -> open date-range picker dialog and call SP sp_Admin_SystemParameters_Search
        private void btnExportReport_Click(object sender, EventArgs e)
        {
            using (var dlg = new DateRangePickerForm())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                DateTime from = dlg.FromDate.Date;
                DateTime to = dlg.ToDate.Date;

                // Defensive check
                if (from > to)
                {
                    MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    dataGridView1.DataSource = null;
                    dataGridView1.Rows.Clear();
                    dataGridView1.Columns.Clear();

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@FromDate", SqlDbType.Date) { Value = from },
                        new SqlParameter("@ToDate",   SqlDbType.Date) { Value = to }
                    };

                    // Call stored procedure with parameters
                    DataTable dt = DatabaseConnection.ExecuteQuery("EXEC sp_Admin_SystemParameters_Search @FromDate, @ToDate", parameters);

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No records found for the selected date range.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    dataGridView1.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Search failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Add button click -> open AddSystemParameterForm
        private void Button18_Click(object sender, EventArgs e)
        {
            using (var dlg = new AddSystemParameterForm(currentAdminID))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    // Reload data after successful add
                    LoadSystemParameters();
                }
            }
        }

        // Delete button click -> open DeleteSystemParameterForm
        private void Button19_Click(object sender, EventArgs e)
        {
            string selectedPID = null;
            if (dataGridView1.CurrentRow != null && dataGridView1.Columns.Contains("SystemPID"))
            {
                selectedPID = dataGridView1.CurrentRow.Cells["SystemPID"].Value?.ToString();
            }

            using (var dlg = new DeleteSystemParameterForm(selectedPID))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    // Reload data after successful delete
                    LoadSystemParameters();
                }
            }
        }

        // Update button click -> open UpdateSystemParameterForm
        // Changed method name from Button20_Click to button20_Click to match Designer
        private void button20_Click(object sender, EventArgs e)
        {
            string selectedPID = null;
            if (dataGridView1.CurrentRow != null && dataGridView1.Columns.Contains("SystemPID"))
            {
                selectedPID = dataGridView1.CurrentRow.Cells["SystemPID"].Value?.ToString();
            }

            using (var dlg = new UpdateSystemParameterForm(selectedPID))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    // Reload data after successful update
                    LoadSystemParameters();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {


        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void button15_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void button14_Click(object sender, EventArgs e)
        {

        }
    }
}
