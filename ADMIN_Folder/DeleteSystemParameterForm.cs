using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ADMIN
{
    public partial class DeleteSystemParameterForm : Form
    {
        private string _initialSystemPID;

        public DeleteSystemParameterForm(string initialSystemPID = null)
        {
            InitializeComponent();
            _initialSystemPID = initialSystemPID;
            if (!string.IsNullOrWhiteSpace(_initialSystemPID))
            {
                txtSystemPID.Text = _initialSystemPID;
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            // Validation
            string systemPID = txtSystemPID.Text.Trim();

            if (string.IsNullOrEmpty(systemPID))
            {
                MessageBox.Show("Vui lòng nhập mã tham số hệ thống (SystemPID).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirmation dialog
            DialogResult result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa tham số hệ thống '{systemPID}'?\n\nThao tác này không thể hoàn tác.",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new SqlCommand("sp_Admin_SystemParameters_Delete", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemPID", SqlDbType.NVarChar, 50) { Value = systemPID });

                    var returnParam = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue,
                        SqlDbType = SqlDbType.Int
                    };
                    cmd.Parameters.Add(returnParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    int ret = (returnParam.Value != DBNull.Value) ? Convert.ToInt32(returnParam.Value) : 0;

                    if (ret < 0)
                    {
                        // SP returns -1 when record not found
                        switch (ret)
                        {
                            case -1:
                                MessageBox.Show($"Lỗi: Không tìm thấy tham số hệ thống '{systemPID}' để xóa.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                break;
                            default:
                                MessageBox.Show("Lỗi không rõ từ stored procedure.", "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                break;
                        }

                        return;
                    }

                    // Success
                    MessageBox.Show($"Xóa tham số hệ thống '{systemPID}' thành công!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi SQL khi xóa tham số: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa tham số: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
    }
}
