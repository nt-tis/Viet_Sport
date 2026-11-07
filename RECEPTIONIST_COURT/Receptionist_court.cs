using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RECEPTIONIST
{
    public partial class Receptionist_court : Form
    {
        // Keep reference to the currently-hosted child Form instance so its events remain active.
        private Form currentHostedChild;

        // Store default sidebar control color so we can restore it when switching selection.
        private Color sidebarDefaultColor;

        // Backup host main-panel original controls so they can be restored after hosting child content.
        private Control[] hostMainOriginalControls;

        public Receptionist_court()
        {
            InitializeComponent();

            // Use lookup helpers instead of direct field references to avoid "name does not exist"
            sidebarDefaultColor = GetPanel("panelSideBar")?.BackColor ?? SystemColors.Control;

            // Capture a snapshot of the original controls inside panelMain (filters + grid)
            var main = GetPanel("panelMain");
            hostMainOriginalControls = main != null ? main.Controls.Cast<Control>().ToArray() : new Control[0];

            InitializeForm();
        }

        // Helper: find a Panel by name searching the form and its descendants.
        // Avoids compile-time dependency on a specific designer field name.
        private Panel GetPanel(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            try
            {
                var found = this.Controls.Find(name, true);
                if (found != null && found.Length > 0 && found[0] is Panel p) return p;

                // Last-resort: search ActiveForm (host scenario)
                var active = Form.ActiveForm;
                if (active != null)
                {
                    found = active.Controls.Find(name, true);
                    if (found != null && found.Length > 0 && found[0] is Panel p2) return p2;
                }
            }
            catch
            {
                // ignore
            }
            return null;
        }

        private void InitializeForm()
        {
            // Test database connection
            if (!DatabaseHelper.TestConnection())
            {
                MessageBox.Show("Không thể kết nối đến database. Vui lòng kiểm tra lại cấu hình.",
                    "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadCourtTypes();
            LoadCourtsData();
        }

        private void LoadCourtTypes()
        {
            try
            {
                // Load court types from database using Stored Procedure
                comboBox1.Items.Clear();
                comboBox1.Items.Add("All");

                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("SP_GetCourtTypes");

                foreach (DataRow row in dt.Rows)
                {
                    comboBox1.Items.Add(row["CourtTypeName"].ToString());
                }

                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải loại sân: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCourtsData()
        {
            try
            {
                // Load court data using Stored Procedure
                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("SP_GetAllCourts");

                // Clear existing rows
                dataGridView1.Rows.Clear();

                // Populate DataGridView
                foreach (DataRow row in dt.Rows)
                {
                    int rowIndex = dataGridView1.Rows.Add();
                    DataGridViewRow dgvRow = dataGridView1.Rows[rowIndex];

                    dgvRow.Cells["CourtID"].Value = row["CourtID"].ToString();
                    dgvRow.Cells["CourtType"].Value = row["CourtType"].ToString();
                    dgvRow.Cells["CourtName"].Value = row["CourtTypeName"].ToString();
                    dgvRow.Cells["Capacity"].Value = row["Capacity"].ToString();
                    dgvRow.Cells["UnitPrice"].Value = row["UnitPrice"].ToString();
                    dgvRow.Cells["CenterID"].Value = row["CenterID"].ToString();
                    dgvRow.Cells["Status"].Value = row["Status"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu sân: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            // Search button - filter courts by date, time and type
            SearchCourts();
        }

        private void SearchCourts()
        {
            try
            {
                // Get filter values
                string selectedCourtType = comboBox1.SelectedItem?.ToString()?.Trim();

                // Load all courts
                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("SP_GetAllCourts");

                // Clear existing rows
                dataGridView1.Rows.Clear();

                // Populate DataGridView with filtered data
                foreach (DataRow row in dt.Rows)
                {
                    string courtTypeName = row["CourtTypeName"]?.ToString()?.Trim() ?? "";
                    string courtTypeCode = row["CourtType"]?.ToString()?.Trim() ?? "";

                    // Filter by court type if not "All" or "Tất cả"
                    if (!string.IsNullOrEmpty(selectedCourtType) &&
                        !string.Equals(selectedCourtType, "All", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(selectedCourtType, "Tất cả", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.Equals(courtTypeName, selectedCourtType, StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(courtTypeCode, selectedCourtType, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }

                    int rowIndex = dataGridView1.Rows.Add();
                    DataGridViewRow dgvRow = dataGridView1.Rows[rowIndex];

                    dgvRow.Cells["CourtID"].Value = row["CourtID"]?.ToString();
                    dgvRow.Cells["CourtType"].Value = row["CourtType"]?.ToString();
                    dgvRow.Cells["CourtName"].Value = row["CourtTypeName"]?.ToString();
                    dgvRow.Cells["Capacity"].Value = row["Capacity"]?.ToString();
                    dgvRow.Cells["UnitPrice"].Value = row["UnitPrice"]?.ToString();
                    dgvRow.Cells["CenterID"].Value = row["CenterID"]?.ToString();
                    dgvRow.Cells["Status"].Value = row["Status"]?.ToString();
                }

                if (dataGridView1.Rows.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy sân nào phù hợp với điều kiện tìm kiếm.",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tìm kiếm: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Load a child form into the right-side panelMain.
        /// If the child form exposes SidebarPanel and ContentPanel, move those panels
        /// into this host so only one dashboard (the child's) is visible.
        /// This method is kept for fallback/full-form hosting behavior.
        /// </summary>
        private void OpenFormInPanel(Form child)
        {
            if (child == null) return;

            // If there's an existing hosted child different from the new child, dispose it now.
            if (currentHostedChild != null && currentHostedChild != child)
            {
                try
                {
                    currentHostedChild.Dispose();
                }
                catch
                {
                    // swallow - disposal shouldn't throw in normal situation
                }
                currentHostedChild = null;
            }

            // Try to detect forms that expose SidebarPanel and ContentPanel
            Panel childSidebar = null;
            Panel childContent = null;

            // booking form
            if (child is Receiptionist_BookingForm booking)
            {
                // Subscribe host navigation handlers to events raised by booking sidebar.
                booking.ShowCourtRequested += (s, evt) =>
                {
                    // Open Receptionist_court (host will replace panels)
                    var f = new Receptionist_court();
                    OpenFormInPanel(f);
                };

                booking.ShowCancellationRequested += (s, evt) =>
                {
                    var f = new Receiptionist_CancellationForm();
                    OpenFormInPanel(f);
                };

                childSidebar = booking.SidebarPanel;
                childContent = booking.ContentPanel;
            }
            // cancellation form
            else if (child is Receiptionist_CancellationForm cancel)
            {
                childSidebar = cancel.SidebarPanel;
                childContent = cancel.ContentPanel;
            }

            if (childSidebar != null && childContent != null)
            {
                // Remove existing sidebar content (host's)
                var hostSide = GetPanel("panelSideBar");
                if (hostSide != null) hostSide.Controls.Clear();

                // Detach panels from child form and add into host panels
                if (child.Controls.Contains(childSidebar)) child.Controls.Remove(childSidebar);
                if (child.Controls.Contains(childContent)) child.Controls.Remove(childContent);

                // Sidebar goes into host left panel - fill it
                childSidebar.Dock = DockStyle.Fill;
                if (hostSide != null) hostSide.Controls.Add(childSidebar);

                // Content goes into host main panel - fill it
                var hostMain = GetPanel("panelMain");
                if (hostMain != null)
                {
                    // Save host main original controls if not already captured (defensive)
                    if (hostMainOriginalControls == null || hostMainOriginalControls.Length == 0)
                        hostMainOriginalControls = hostMain.Controls.Cast<Control>().ToArray();

                    hostMain.Controls.Clear();
                    childContent.Dock = DockStyle.Fill;
                    hostMain.Controls.Add(childContent);
                }

                // Ensure visible and refresh layout
                childSidebar.Visible = true;
                childContent.Visible = true;
                hostSide?.Refresh();
                hostMain?.Refresh();

                // Keep a reference to the child instance so its events remain active
                currentHostedChild = child;

                return;
            }

            // Fallback: host the full form inside panelMain (previous behavior)
            var fallbackMain = GetPanel("panelMain");
            if (fallbackMain != null) fallbackMain.Controls.Clear();

            child.TopLevel = false;
            child.FormBorderStyle = FormBorderStyle.None;
            child.Dock = DockStyle.Fill;

            if (fallbackMain != null)
            {
                fallbackMain.Controls.Add(child);
                fallbackMain.Tag = child;
            }

            // Keep reference to hosted form so we can dispose it later
            currentHostedChild = child;

            child.Show();
        }

        /// <summary>
        /// NEW: Only extract the child's ContentPanel and host it in panelMain.
        /// The host's sidebar (panelSideBar) is left untouched.
        /// </summary>
        private void OpenChildContentInPanel(Form child)
        {
            if (child == null) return;

            // Dispose previously hosted child (safe because its panels were removed when hosted)
            if (currentHostedChild != null && currentHostedChild != child)
            {
                try
                {
                    currentHostedChild.Dispose();
                }
                catch
                {
                    // ignore
                }
                currentHostedChild = null;
            }

            Panel childContent = null;

            if (child is Receiptionist_BookingForm booking)
            {
                childContent = booking.ContentPanel;
            }
            else if (child is Receiptionist_CancellationForm cancel)
            {
                childContent = cancel.ContentPanel;
            }

            if (childContent != null)
            {
                // Detach content panel from the child form and host it
                if (child.Controls.Contains(childContent)) child.Controls.Remove(childContent);

                childContent.Dock = DockStyle.Fill;
                var hostMain = GetPanel("panelMain");
                if (hostMain != null)
                {
                    hostMain.Controls.Clear();
                    hostMain.Controls.Add(childContent);
                    childContent.Visible = true;
                    hostMain.Refresh();
                }

                // Keep reference so we can dispose later without affecting the hosted panel
                currentHostedChild = child;
                return;
            }

            // If no ContentPanel found, fallback to hosting the whole form (previous behavior)
            OpenFormInPanel(child);
        }

        /// <summary>
        /// Restore the host's original main controls (filters + grid) that were present before hosting child content.
        /// </summary>
        private void RestoreHostMainControls()
        {
            // Remove any hosted external content
            var hostMain = GetPanel("panelMain");
            if (hostMain == null) return;

            hostMain.Controls.Clear();

            if (hostMainOriginalControls != null && hostMainOriginalControls.Length > 0)
            {
                foreach (var ctrl in hostMainOriginalControls)
                {
                    try
                    {
                        // detach from previous parent if still attached somewhere
                        if (ctrl.Parent != null && ctrl.Parent != hostMain)
                            ctrl.Parent.Controls.Remove(ctrl);

                        hostMain.Controls.Add(ctrl);
                    }
                    catch
                    {
                        // ignore individual failures
                    }
                }
            }
        }

        /// <summary>
        /// Highlight the selected control in the sidebar by setting its BackColor to Orange.
        /// Reset other direct children of panelSideBar to the saved default color.
        /// </summary>
        private void HighlightSidebarSelection(Control selected)
        {
            var hostSide = GetPanel("panelSideBar");
            if (hostSide == null) return;

            foreach (Control c in hostSide.Controls)
            {
                try
                {
                    c.BackColor = sidebarDefaultColor;
                }
                catch
                {
                    // ignore if some controls don't accept BackColor change
                }
            }

            if (selected != null)
            {
                try
                {
                    selected.BackColor = Color.Orange;
                }
                catch
                {
                    // ignore if not changeable
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                var bookingForm = new Receiptionist_BookingForm();

                // Host only the booking content panel; keep the host sidebar unchanged.
                OpenChildContentInPanel(bookingForm);

                // Highlight the corresponding sidebar entry. If the button that represents booking
                // lives inside panelSideBar you can pass that control instead. Here we highlight
                // the actual host-side button that was clicked.
                HighlightSidebarSelection(sender as Control);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở Booking Form: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            try
            {
                var cancelForm = new Receiptionist_CancellationForm();

                // Host only the cancellation content panel; keep the host sidebar unchanged.
                OpenChildContentInPanel(cancelForm);

                // Highlight the corresponding sidebar entry (the button clicked).
                HighlightSidebarSelection(sender as Control);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở Cancellation Form: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Added to fix the missing event-handler compile error.
        // This restores the host's main content (the DataGridView listing) and resets highlight.
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                // Restore original host main controls (filters + grid)
                RestoreHostMainControls();

                // Ensure DataGridView is present and docked correctly
                if (dataGridView1.Parent != GetPanel("panelMain"))
                {
                    var prev = dataGridView1.Parent;
                    if (prev != null) prev.Controls.Remove(dataGridView1);

                    dataGridView1.Dock = DockStyle.Fill;

                    // Add DataGridView to panelMain if it is not already present because some layouts
                    // place the grid inside a container (e.g., GroupBox). If so, ensure its container
                    // is restored by RestoreHostMainControls above.
                    var hostMain = GetPanel("panelMain");
                    if (hostMain != null && !hostMain.Controls.Contains(dataGridView1))
                        hostMain.Controls.Add(dataGridView1);
                }

                dataGridView1.Visible = true;

                // Reload content to ensure up-to-date display
                LoadCourtTypes();
                LoadCourtsData();

                // Highlight the clicked sidebar control if appropriate
                HighlightSidebarSelection(sender as Control);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi trở về trang chính: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Designer event handler stubs

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // Designer expects this handler. Leave empty or add custom painting.
        }

        private void panelMain_Paint(object sender, PaintEventArgs e)
        {
            // Designer expects this handler. Leave empty or add custom painting.
        }

        private void panel10_Paint(object sender, PaintEventArgs e)
        {
            // Designer expects this handler. Leave empty or add custom painting.
        }

        private void panel12_Paint(object sender, PaintEventArgs e)
        {
            // Designer expects this handler. Leave empty or add custom painting.
        }

        #endregion

        // Missing DataGridView cell click handler expected by the Designer
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // No-op for now; implement behavior when a cell is clicked.
            // Example: open details when e.RowIndex >= 0
            if (e.RowIndex >= 0)
            {
                var row = dataGridView1.Rows[e.RowIndex];
                // string courtID = row.Cells["CourtID"]?.Value?.ToString();
                // TODO: handle selection
            }
        }

        private void button5_Click_2(object sender, EventArgs e)
        {
            // Designer expects this handler. If this button should navigate/open a page,
            // implement the logic here. For now keep it no-op to satisfy the Designer.
        }
    }
}