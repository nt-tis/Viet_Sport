using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace RECEPTIONIST
{
    public partial class Receiptionist_Service : Form
    {
        private DataGridView dgvServices;
        private FlowLayoutPanel pnlActions;
        private Panel pnlTop;
        private ComboBox cboFilter;
        private Button btnRefresh;
        private Button btnAdd;
        private Button btnUpdateStatus;

        public Receiptionist_Service()
        {
            InitializeComponent();

            // Ensure consistent scaling & font
            this.AutoScaleMode = AutoScaleMode.None;
            this.Font = new Font("Segoe UI", 9F);

            BuildRuntimeUi();
            this.Load += Receiptionist_Service_Load;
        }

        private void BuildRuntimeUi()
        {
            // Top panel with filter combobox
            pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                Padding = new Padding(8),
                BackColor = Color.Transparent
            };

            var lblFilter = new Label
            {
                Text = "Filter:",
                AutoSize = true,
                Location = new Point(8, 12),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(33, 37, 41)
            };

            // Filter combobox: display friendly text, ValueMember is the proc value
            cboFilter = new ComboBox
            {
                Left = lblFilter.Right + 10,
                Top = 8,
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F)
            };

            var filterItems = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Tất cả", null),
                new KeyValuePair<string, string>("Tủ (CLOSET)", "CLOSET"),
                new KeyValuePair<string, string>("Thiết bị (EQUIPMENT)", "EQUIPMENT"),
                new KeyValuePair<string, string>("VIP (VIP)", "VIP"),
                new KeyValuePair<string, string>("HLV (COACH)", "COACH")
            };

            cboFilter.DataSource = new BindingSource(filterItems, null);
            cboFilter.DisplayMember = "Key";
            cboFilter.ValueMember = "Value";
            // guard SelectedIndex (avoid exception when Items is empty)
            if (cboFilter.Items != null && cboFilter.Items.Count > 0)
            {
                cboFilter.SelectedIndex = 0;
            }
            cboFilter.SelectedIndexChanged += (s, e) => LoadServices(cboFilter.SelectedValue as string);

            pnlTop.Controls.Add(lblFilter);
            pnlTop.Controls.Add(cboFilter);

            // DataGridView
            dgvServices = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            // header look fixed
            dgvServices.EnableHeadersVisualStyles = false;
            var headerStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(34, 139, 79),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };
            dgvServices.ColumnHeadersDefaultCellStyle = headerStyle;
            dgvServices.RowHeadersDefaultCellStyle = headerStyle;

            // Right action panel (vertical)
            pnlActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 160,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(8),
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            btnRefresh = new Button { Text = "Refresh", Height = 42, Width = pnlActions.Width - pnlActions.Padding.Horizontal, Margin = new Padding(0, 6, 0, 0) };
            btnAdd = new Button { Text = "Add", Height = 44, Width = pnlActions.Width - pnlActions.Padding.Horizontal, Margin = new Padding(0, 12, 0, 0) };
            btnUpdateStatus = new Button { Text = "Update Status", Height = 44, Width = pnlActions.Width - pnlActions.Padding.Horizontal, Margin = new Padding(0, 12, 0, 0) };

            // apply consistent fixed colors similar to other forms
            ApplyActionStyle(btnRefresh, Color.FromArgb(0, 123, 255), Color.White);
            ApplyActionStyle(btnAdd, Color.FromArgb(40, 167, 69), Color.White);
            ApplyActionStyle(btnUpdateStatus, Color.FromArgb(255, 193, 7), Color.FromArgb(33, 37, 41));

            // wire actions
            btnRefresh.Click += (s, e) => LoadServices(cboFilter.SelectedValue as string);
            btnAdd.Click += (s, e) => AddService();
            btnUpdateStatus.Click += (s, e) => UpdateServiceStatus();

            pnlActions.Controls.Add(btnRefresh);
            pnlActions.Controls.Add(btnAdd);
            pnlActions.Controls.Add(btnUpdateStatus);

            // Add controls to form: top panel first so DataGridView fills remaining space
            this.Controls.Add(dgvServices);
            this.Controls.Add(pnlActions);
            this.Controls.Add(pnlTop);
        }

        private void ApplyActionStyle(Button btn, Color back, Color fore)
        {
            btn.UseVisualStyleBackColor = false;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = back;
            btn.ForeColor = fore;
            btn.FlatAppearance.MouseOverBackColor = back;
            btn.FlatAppearance.MouseDownBackColor = back;
        }

        private void Receiptionist_Service_Load(object sender, EventArgs e)
        {
            // initial load uses combobox selection (null means all)
            LoadServices(cboFilter?.SelectedValue as string);
        }

        // (1) Load & display services with optional filter
        private void LoadServices(string filterType = null)
        {
            try
            {
                SqlParameter[] parms = new[] {
                    new SqlParameter("@FilterType", (object)filterType ?? DBNull.Value)
                };

                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("usp_GetServicesByType", parms);

                // Normalize status values to Vietnamese for UI (handle both English and VN stored values)
                if (dt.Columns.Contains("AvailableStatus"))
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        var v = (r["AvailableStatus"] ?? "").ToString().Trim();
                        if (string.Equals(v, "Available", StringComparison.OrdinalIgnoreCase) || v == "Đang hoạt động" || v.Contains("hoạt động"))
                            r["AvailableStatus"] = "Đang hoạt động";
                        else if (string.Equals(v, "Unavailable", StringComparison.OrdinalIgnoreCase) || v == "Bảo trì" || v.Contains("Bảo trì") || v.Contains("bảo trì"))
                            r["AvailableStatus"] = "Bảo trì";
                        else if (v.Contains("ngưng") || v.Contains("ngung"))
                            r["AvailableStatus"] = "Tạm ngưng";
                    }
                }

                dgvServices.DataSource = dt;

                // hide all columns first (safe for variable result sets)
                foreach (DataGridViewColumn col in dgvServices.Columns)
                    col.Visible = false;

                // Always show basic service columns if present
                EnsureColumnVisible("ServiceID", "Mã Dịch Vụ");
                EnsureColumnVisible("ServiceName", "Tên Dịch Vụ");
                EnsureColumnVisible("TimeUnitType", "Đơn Vị Tính");
                EnsureColumnVisible("Price", "Đơn Giá (VNĐ)");
                EnsureColumnVisible("AvailableStatus", "Trạng Thái");

                if (dgvServices.Columns.Contains("Price"))
                {
                    dgvServices.Columns["Price"].DefaultCellStyle.Format = "N0";
                }

                // Show filter-specific columns only
                if (!string.IsNullOrEmpty(filterType))
                {
                    switch (filterType.ToUpperInvariant())
                    {
                        case "CLOSET":
                            EnsureColumnVisible("ClosetID", "Mã Tủ");
                            EnsureColumnVisible("StatusCloset", "Trạng Thái Tủ");
                            break;
                        case "EQUIPMENT":
                            EnsureColumnVisible("EquipmentType", "Loại Dụng Cụ");
                            EnsureColumnVisible("Quantity", "Số Lượng");
                            break;
                        case "VIP":
                            EnsureColumnVisible("RoomID", "Số Phòng VIP");
                            EnsureColumnVisible("StatusRoom", "Trạng Thái Phòng");
                            break;
                        case "COACH":
                        case "COACHES":
                            EnsureColumnVisible("CoachID", "Mã HLV");
                            EnsureColumnVisible("CoachName", "Họ Tên HLV");
                            EnsureColumnVisible("Specialization", "Chuyên Môn");
                            break;
                        default:
                            break;
                    }
                }

                // Formatting and layout adjustments
                dgvServices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvServices.AutoResizeColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách dịch vụ:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // helper used by LoadServices
        private void EnsureColumnVisible(string columnName, string headerText)
        {
            if (dgvServices.Columns.Contains(columnName))
            {
                dgvServices.Columns[columnName].Visible = true;
                dgvServices.Columns[columnName].HeaderText = headerText;
            }
        }

        // (2) Add new service - updated to match new usp_InsertService signature
        private void AddService()
        {
            using (var f = new Form())
            {
                f.Text = "Add Service";
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.ClientSize = new Size(460, 360);
                f.MinimizeBox = false; f.MaximizeBox = false;
                f.BackColor = Color.FromArgb(249, 250, 252);
                f.Padding = new Padding(10);

                // Basic fields
                var lblId = new Label { Text = "Service ID", Left = 12, Top = 16, Width = 120 };
                var txtId = new TextBox { Left = 140, Top = 12, Width = 300 };

                var lblName = new Label { Text = "Name", Left = 12, Top = 52, Width = 120 };
                var txtName = new TextBox { Left = 140, Top = 48, Width = 300 };

                var lblTime = new Label { Text = "Time Unit", Left = 12, Top = 88, Width = 120 };
                var txtTime = new TextBox { Left = 140, Top = 84, Width = 300 };

                var lblPrice = new Label { Text = "Price", Left = 12, Top = 124, Width = 120 };
                var txtPrice = new TextBox { Left = 140, Top = 120, Width = 300 };

                var lblStatus = new Label { Text = "Status", Left = 12, Top = 160, Width = 120 };
                var cboStatus = new ComboBox { Left = 140, Top = 156, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
                cboStatus.Items.AddRange(new[] { "Đang hoạt động", "Bảo trì" });
                // after cboStatus.Items.AddRange(...)
                if (cboStatus.Items != null && cboStatus.Items.Count > 0)
                    cboStatus.SelectedIndex = 0;

                var lblType = new Label { Text = "Service Type", Left = 12, Top = 196, Width = 120 };
                var cboType = new ComboBox { Left = 140, Top = 192, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
                // display friendly + map to proc values
                cboType.Items.AddRange(new[]
                {
                    "Thiết bị (EQUIPMENT)",
                    "Tủ cá nhân (CLOSET)",
                    "Phòng VIP (VIP)",
                    "Huấn luyện viên (HLV)"
                });
                // after cboType.Items.AddRange(...)
                if (cboType.Items != null && cboType.Items.Count > 0)
                    cboType.SelectedIndex = 0;

                // Extra inputs (shown conditionally)
                var lblEquipType = new Label { Text = "Equipment Type", Left = 12, Top = 232, Width = 120, Visible = false };
                var txtEquipType = new TextBox { Left = 140, Top = 228, Width = 300, Visible = false };

                var lblQuantity = new Label { Text = "Quantity", Left = 12, Top = 268, Width = 120, Visible = false };
                var nudQuantity = new NumericUpDown { Left = 140, Top = 264, Width = 120, Minimum = 1, Maximum = 10000, Value = 1, Visible = false };

                var lblClosetRoom = new Label { Text = "Closet/Room ID", Left = 12, Top = 232, Width = 120, Visible = false };
                var nudClosetRoom = new NumericUpDown { Left = 140, Top = 228, Width = 120, Minimum = 0, Maximum = int.MaxValue, Visible = false };

                var lblCoach = new Label { Text = "Coach ID", Left = 12, Top = 268, Width = 120, Visible = false };
                var txtCoach = new TextBox { Left = 140, Top = 264, Width = 300, Visible = false };

                // Show/hide extras based on type
                cboType.SelectedIndexChanged += (s, e) =>
                {
                    var sel = cboType.SelectedItem?.ToString() ?? "";
                    bool isEquipment = sel.Contains("EQUIPMENT");
                    bool isCloset = sel.Contains("CLOSET");
                    bool isVIP = sel.Contains("VIP") && !sel.Contains("EQUIPMENT"); // VIP item contains VIP
                    bool isHLV = sel.Contains("HLV");

                    lblEquipType.Visible = txtEquipType.Visible = isEquipment;
                    lblQuantity.Visible = nudQuantity.Visible = isEquipment;

                    lblClosetRoom.Visible = nudClosetRoom.Visible = isCloset || isVIP;
                    lblCoach.Visible = txtCoach.Visible = isHLV;

                    // ensure a sensible default / minimum when shown
                    if (isEquipment)
                    {
                        if (nudQuantity.Value < nudQuantity.Minimum) nudQuantity.Value = nudQuantity.Minimum;
                    }
                };

                // Buttons
                var btnOk = new Button { Text = "OK", Left = 240, Width = 100, Top = 300, DialogResult = DialogResult.OK };
                var btnCancel = new Button { Text = "Cancel", Left = 360, Width = 80, Top = 300, DialogResult = DialogResult.Cancel };
                ApplyActionStyle(btnOk, Color.FromArgb(40, 167, 69), Color.White);
                ApplyActionStyle(btnCancel, Color.FromArgb(220, 220, 220), Color.FromArgb(33, 37, 41));
                btnOk.Click += (s, e) => { f.DialogResult = DialogResult.OK; };

                // Styling labels/inputs
                foreach (var lbl in new[] { lblId, lblName, lblTime, lblPrice, lblStatus, lblType, lblEquipType, lblQuantity, lblClosetRoom, lblCoach })
                {
                    lbl.ForeColor = Color.FromArgb(33, 37, 41);
                    lbl.Font = new Font("Segoe UI", 9F);
                }
                foreach (var tb in new[] { txtId, txtName, txtTime, txtPrice, txtEquipType, txtCoach })
                {
                    tb.BackColor = Color.White; tb.BorderStyle = BorderStyle.FixedSingle; tb.Font = new Font("Segoe UI", 9F);
                }
                cboStatus.BackColor = cboType.BackColor = Color.White;

                f.Controls.AddRange(new Control[] {
                    lblId, txtId, lblName, txtName, lblTime, txtTime, lblPrice, txtPrice,
                    lblStatus, cboStatus, lblType, cboType,
                    lblEquipType, txtEquipType, lblQuantity, nudQuantity,
                    lblClosetRoom, nudClosetRoom, lblCoach, txtCoach,
                    btnOk, btnCancel
                });

                // trigger initial visibility
                cboType.SelectedIndex = 0;

                if (f.ShowDialog(this) != DialogResult.OK) return;

                // Read inputs and validate
                string id = txtId.Text.Trim();
                string name = txtName.Text.Trim();
                string timeUnit = txtTime.Text.Trim();
                if (!decimal.TryParse(txtPrice.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price))
                {
                    MessageBox.Show("Price invalid.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string statusToStore = cboStatus.SelectedItem?.ToString() ?? "Đang hoạt động";

                // Map display type to proc value
                string selType = cboType.SelectedItem?.ToString() ?? "";
                string serviceType;
                if (selType.Contains("EQUIPMENT")) serviceType = "EQUIPMENT";
                else if (selType.Contains("CLOSET")) serviceType = "CLOSET";
                else if (selType.Contains("VIP")) serviceType = "VIP";
                else serviceType = "HLV";

                // Child params (determine from serviceType — do NOT rely on control.Visible)
                string equipmentType = serviceType == "EQUIPMENT" ? txtEquipType.Text.Trim() : null;
                int? quantity = serviceType == "EQUIPMENT" ? (int?)Convert.ToInt32(nudQuantity.Value) : null;
                int? closetOrRoomId = (serviceType == "CLOSET" || serviceType == "VIP") ? (int?)Convert.ToInt32(nudClosetRoom.Value) : null;
                string coachId = serviceType == "HLV" ? txtCoach.Text.Trim() : null;

                // Validate required child inputs depending on type
                if (serviceType == "EQUIPMENT")
                {
                    if (string.IsNullOrWhiteSpace(equipmentType) || !quantity.HasValue || quantity.Value < 1)
                    {
                        MessageBox.Show($"Please provide Equipment Type and positive Quantity.\nEquipmentType='{(equipmentType ?? "<null>")}', Quantity={(quantity.HasValue ? quantity.Value.ToString() : "<null>")}", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if ((serviceType == "CLOSET" || serviceType == "VIP") && !closetOrRoomId.HasValue)
                {
                    MessageBox.Show("Please provide Closet/Room ID.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (serviceType == "HLV" && string.IsNullOrWhiteSpace(coachId))
                {
                    MessageBox.Show("Please provide Coach ID.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Call stored proc with matching parameters
                try
                {
                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    using (SqlCommand cmd = new SqlCommand("usp_InsertService", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ServiceID", string.IsNullOrEmpty(id) ? (object)DBNull.Value : id);
                        cmd.Parameters.AddWithValue("@ServiceName", string.IsNullOrEmpty(name) ? (object)DBNull.Value : name);
                        cmd.Parameters.AddWithValue("@TimeUnitType", string.IsNullOrEmpty(timeUnit) ? (object)DBNull.Value : timeUnit);
                        cmd.Parameters.Add(new SqlParameter("@Price", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = price });
                        cmd.Parameters.AddWithValue("@AvailableStatus", (object)statusToStore ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ServiceType", serviceType);

                        // optional child params
                        if (serviceType == "EQUIPMENT")
                        {
                            cmd.Parameters.AddWithValue("@EquipmentType", (object)equipmentType ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Quantity", (object)quantity ?? DBNull.Value);
                        }
                        else if (serviceType == "CLOSET" || serviceType == "VIP")
                        {
                            cmd.Parameters.AddWithValue("@ClosetOrRoomID", (object)closetOrRoomId ?? DBNull.Value);
                        }
                        else if (serviceType == "HLV")
                        {
                            cmd.Parameters.AddWithValue("@CoachID", (object)coachId ?? DBNull.Value);
                        }

                        // return val
                        var ret = new SqlParameter("@ReturnVal", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                        cmd.Parameters.Add(ret);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        int result = (ret.Value != DBNull.Value) ? Convert.ToInt32(ret.Value) : 0;

                        if (result == 1)
                        {
                            MessageBox.Show("Service added.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadServices();
                            return;
                        }
                        else if (result == -1)
                        {
                            MessageBox.Show("ServiceID already exists.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        else if (result == -2)
                        {
                            MessageBox.Show("Price cannot be negative.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        else if (result == -3)
                        {
                            MessageBox.Show("System error while inserting service.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        else
                        {
                            // unknown return -> still inform user
                            MessageBox.Show("Insert returned: " + result, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                }
                catch (SqlException sqlex)
                {
                    // if stored proc uses THROW with custom error number, show message
                    MessageBox.Show("Database error: " + sqlex.Message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Insert failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        // (3) Update status for selected service - store VN string and accept VN/EN when preselecting
        private void UpdateServiceStatus()
        {
            if (dgvServices.CurrentRow == null)
            {
                MessageBox.Show("Select a service row first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var row = dgvServices.CurrentRow;
            string serviceId = row.Cells["ServiceID"]?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(serviceId))
            {
                MessageBox.Show("Invalid ServiceID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var f = new Form())
            {
                f.Text = "Update Service Status";
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.ClientSize = new Size(360, 140);

                var lbl = new Label { Text = "New Status", Left = 12, Top = 16, Width = 100 };
                var cbo = new ComboBox { Left = 120, Top = 12, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
                cbo.Items.AddRange(new[] { "Đang hoạt động", "Bảo trì" });

                try
                {
                    var currentStatusValue = row.Cells["AvailableStatus"]?.Value?.ToString() ?? "";
                    // Accept both English and Vietnamese values when preselecting
                    if (string.Equals(currentStatusValue, "Available", StringComparison.OrdinalIgnoreCase) || currentStatusValue == "Đang hoạt động")
                        cbo.SelectedItem = "Đang hoạt động";
                    else if (string.Equals(currentStatusValue, "Unavailable", StringComparison.OrdinalIgnoreCase) || currentStatusValue == "Bảo trì")
                        cbo.SelectedItem = "Bảo trì";
                    else
                        cbo.SelectedIndex = 0;
                }
                catch
                {
                    cbo.SelectedIndex = 0;
                }

                var btnOk = new Button { Text = "OK", Left = 180, Width = 80, Top = 64, DialogResult = DialogResult.OK };
                var btnCancel = new Button { Text = "Cancel", Left = 268, Width = 80, Top = 64, DialogResult = DialogResult.Cancel };
                btnOk.Click += (s, e) => { f.DialogResult = DialogResult.OK; };

                // Styling (kept from previous)
                f.BackColor = Color.FromArgb(249, 250, 252);
                lbl.ForeColor = Color.FromArgb(33, 37, 41);
                lbl.Font = new Font("Segoe UI", 9F);
                cbo.BackColor = Color.White;
                cbo.FlatStyle = FlatStyle.Flat;
                cbo.Font = new Font("Segoe UI", 9F);

                ApplyActionStyle(btnOk, Color.FromArgb(34, 139, 79), Color.White);
                ApplyActionStyle(btnCancel, Color.FromArgb(220, 220, 220), Color.FromArgb(33, 37, 41));

                f.Controls.AddRange(new Control[] { lbl, cbo, btnOk, btnCancel });

                if (f.ShowDialog(this) != DialogResult.OK) return;

                // Store VN label
                string newStatusDisplay = cbo.SelectedItem?.ToString() ?? "Đang hoạt động";
                string newStatusToStore = newStatusDisplay; // store VN label

                try
                {
                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    using (SqlCommand cmd = new SqlCommand("usp_UpdateServiceStatus", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ServiceID", serviceId);
                        cmd.Parameters.AddWithValue("@NewStatus", newStatusToStore);

                        var ret = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
                        ret.Direction = ParameterDirection.ReturnValue;

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        int returnVal = (ret.Value != DBNull.Value) ? Convert.ToInt32(ret.Value) : 0;

                        bool success = returnVal == 1 || returnVal == 0 || rowsAffected > 0;

                        if (success)
                        {
                            MessageBox.Show("Status updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadServices();
                        }
                        else if (returnVal == -1)
                        {
                            MessageBox.Show("ServiceID not found.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show($"Update returned: {returnVal}\nRowsAffected: {rowsAffected}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Update failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
