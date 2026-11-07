using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ADMIN
{
    public partial class UpdateSystemParameterForm : Form
    {
        private string _initialSystemPID;

        public UpdateSystemParameterForm(string initialSystemPID = null)
        {
            InitializeComponent();
            _initialSystemPID = initialSystemPID;
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            // Load existing data based on SystemPID
            string systemPID = txtSystemPID.Text.Trim();

            if (string.IsNullOrEmpty(systemPID))
            {
                MessageBox.Show("Vui lòng nhập mã tham số hệ thống (SystemPID).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@SystemPID", SqlDbType.NVarChar, 50) { Value = systemPID }
                };

                DataTable dt = DatabaseConnection.ExecuteQuery(
                    "SELECT Price, MinBookingTime, MaxBookingTime, MaxCourtsPerCustomer FROM SYSTEM_PARAMETER WHERE SystemPID = @SystemPID",
                    parameters
                );

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show($"Không tìm thấy tham số hệ thống '{systemPID}'.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Load data into controls
                DataRow row = dt.Rows[0];
                numPrice.Value = Convert.ToDecimal(row["Price"]);
                numMinBookingTime.Value = Convert.ToInt32(row["MinBookingTime"]);
                numMaxBookingTime.Value = Convert.ToInt32(row["MaxBookingTime"]);
                numMaxCourts.Value = Convert.ToInt32(row["MaxCourtsPerCustomer"]);

                // Enable controls for editing
                EnableEditControls(true);
                MessageBox.Show("Tải dữ liệu thành công! Bạn có thể chỉnh sửa các giá trị.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            // Validation
            if (!ValidateInputs(out string errorMessage))
            {
                MessageBox.Show(errorMessage, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string systemPID = txtSystemPID.Text.Trim();

            // Confirmation
            DialogResult result = MessageBox.Show(
                $"Bạn có chắc chắn muốn cập nhật tham số hệ thống '{systemPID}'?",
                "Xác nhận cập nhật",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                // Parse inputs
                decimal price = numPrice.Value;
                int minBookingTime = (int)numMinBookingTime.Value;
                int maxBookingTime = (int)numMaxBookingTime.Value;
                int maxCourts = (int)numMaxCourts.Value;

                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new SqlCommand("sp_Admin_Dashboard_Update", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@SystemPID", SqlDbType.NVarChar, 50) { Value = systemPID });
                    var pPrice = new SqlParameter("@NewPrice", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = price };
                    cmd.Parameters.Add(pPrice);
                    cmd.Parameters.Add(new SqlParameter("@NewMinTime", SqlDbType.Int) { Value = minBookingTime });
                    cmd.Parameters.Add(new SqlParameter("@NewMaxTime", SqlDbType.Int) { Value = maxBookingTime });
                    cmd.Parameters.Add(new SqlParameter("@NewMaxCourts", SqlDbType.Int) { Value = maxCourts });

                    var returnParam = new SqlParameter
                    {
                        ParameterName = "@ReturnVal",
                        Direction = ParameterDirection.ReturnValue,
                        SqlDbType = SqlDbType.Int
                    };
                    cmd.Parameters.Add(returnParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    int ret = (returnParam.Value != DBNull.Value) ? Convert.ToInt32(returnParam.Value) : 0;

                    // SP returns negative error codes (-1..-4). Treat 0 or positive as success.
                    if (ret < 0)
                    {
                        string msg;
                        switch (ret)
                        {
                            case -1:
                                msg = $"Lỗi: Không tìm thấy thiết lập có mã {systemPID} để cập nhật.";
                                break;
                            case -2:
                                msg = "Lỗi: Đơn giá mới (NewPrice) phải lớn hơn 0.";
                                break;
                            case -3:
                                msg = "Lỗi: Số lượng sân đặt tối đa (NewMaxCourts) phải lớn hơn 0.";
                                break;
                            case -4:
                                msg = "Lỗi: Dữ liệu thời gian đặt sân không hợp lệ (Phải > 0 và Min < Max).";
                                break;
                            default:
                                msg = "Lỗi không rõ từ stored procedure.";
                                break;
                        }

                        MessageBox.Show(msg, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Success
                    MessageBox.Show($"Cập nhật tham số hệ thống '{systemPID}' thành công!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi SQL khi cập nhật tham số: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật tham số: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInputs(out string errorMessage)
        {
            errorMessage = string.Empty;

            // SystemPID required
            if (string.IsNullOrEmpty(txtSystemPID.Text.Trim()))
            {
                errorMessage = "Vui lòng nhập mã tham số hệ thống (SystemPID).";
                return false;
            }

            // Price must be > 0
            if (numPrice.Value <= 0)
            {
                errorMessage = "Đơn giá sân (Price) phải lớn hơn 0.";
                return false;
            }

            // Min < Max
            if (numMinBookingTime.Value >= numMaxBookingTime.Value)
            {
                errorMessage = "Thời gian đặt tối thiểu phải nhỏ hơn thời gian đặt tối đa.";
                return false;
            }

            // Min > 0
            if (numMinBookingTime.Value <= 0)
            {
                errorMessage = "Thời gian đặt tối thiểu phải lớn hơn 0.";
                return false;
            }

            // Max > 0
            if (numMaxBookingTime.Value <= 0)
            {
                errorMessage = "Thời gian đặt tối đa phải lớn hơn 0.";
                return false;
            }

            // MaxCourts > 0
            if (numMaxCourts.Value <= 0)
            {
                errorMessage = "Số lượng sân đặt tối đa phải lớn hơn 0.";
                return false;
            }

            return true;
        }

        private void EnableEditControls(bool enabled)
        {
            numPrice.Enabled = enabled;
            numMinBookingTime.Enabled = enabled;
            numMaxBookingTime.Enabled = enabled;
            numMaxCourts.Enabled = enabled;
            btnUpdate.Enabled = enabled;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Auto-format SystemPID input to uppercase
        private void TxtSystemPID_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = txtSystemPID.SelectionStart;
            txtSystemPID.Text = txtSystemPID.Text.ToUpper();
            txtSystemPID.SelectionStart = selectionStart;
        }

        private void UpdateSystemParameterForm_Load(object sender, EventArgs e)
        {
            // Initially disable edit controls until data is loaded
            EnableEditControls(false);

            try
            {
                DataTable dtPids = DatabaseConnection.ExecuteQuery("SELECT TOP 50 SystemPID FROM SYSTEM_PARAMETER ORDER BY SetupDate DESC, SetupTime DESC");
                if (dtPids != null && dtPids.Rows.Count > 0)
                {
                    var ac = new AutoCompleteStringCollection();
                    foreach (DataRow r in dtPids.Rows)
                    {
                        ac.Add(r["SystemPID"].ToString());
                    }
                    txtSystemPID.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    txtSystemPID.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    txtSystemPID.AutoCompleteCustomSource = ac;

                    if (string.IsNullOrWhiteSpace(_initialSystemPID))
                    {
                        _initialSystemPID = dtPids.Rows[0]["SystemPID"].ToString();
                    }
                }
            }
            catch { /* non-blocking */ }

            if (!string.IsNullOrWhiteSpace(_initialSystemPID))
            {
                txtSystemPID.Text = _initialSystemPID;
                BtnLoad_Click(this, EventArgs.Empty);
            }
        }
    }
}
