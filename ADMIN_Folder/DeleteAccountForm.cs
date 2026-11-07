using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ADMIN
{
    public partial class DeleteAccountForm : Form
    {
        private string _pendingEmployeeID;

        public DeleteAccountForm()
        {
            InitializeComponent();
        }

        // Public property to safely prefill EmployeeID from the caller
        public string EmployeeID
        {
            get => txtEmployeeID?.Text ?? string.Empty;
            set
            {
                if (txtEmployeeID != null)
                    txtEmployeeID.Text = value;
                else
                    _pendingEmployeeID = value;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!string.IsNullOrEmpty(_pendingEmployeeID))
            {
                txtEmployeeID.Text = _pendingEmployeeID;
                _pendingEmployeeID = null;
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            string employeeId = txtEmployeeID.Text.Trim();

            if (string.IsNullOrEmpty(employeeId))
            {
                MessageBox.Show("Vui lòng nhập mã nhân viên.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa tài khoản của nhân viên '{employeeId}'? Thao tác này không thể hoàn tác.",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new SqlCommand("sp_Admin_AccountEmployees_DeleteAccount", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@EmployeeID", SqlDbType.NVarChar, 50) { Value = employeeId });

                    var returnParam = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue,
                        SqlDbType = SqlDbType.Int
                    };
                    cmd.Parameters.Add(returnParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    int ret = (returnParam.Value != DBNull.Value) ? Convert.ToInt32(returnParam.Value) : 0;

                    if (ret == 0 || ret == 1)
                    {
                        MessageBox.Show($"Xóa tài khoản của nhân viên '{employeeId}' thành công.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return;
                    }

                    switch (ret)
                    {
                        case -1:
                            MessageBox.Show($"Lỗi: Nhân viên có mã '{employeeId}' không tồn tại trên hệ thống.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        case -2:
                            MessageBox.Show($"Lỗi: Nhân viên '{employeeId}' vốn không có tài khoản.", "No Account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        default:
                            MessageBox.Show($"Lỗi không rõ (code {ret}).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi SQL khi xóa tài khoản: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa tài khoản: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void TxtEmployeeID_TextChanged(object sender, EventArgs e)
        {
            int pos = txtEmployeeID.SelectionStart;
            txtEmployeeID.Text = txtEmployeeID.Text.ToUpper();
            txtEmployeeID.SelectionStart = pos;
        }
    }
}
