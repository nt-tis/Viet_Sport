using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ADMIN
{
    public partial class AddEmployeeForm : Form
    {
        public AddEmployeeForm()
        {
            InitializeComponent();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // Gather values
            string fullName = txtFullName.Text.Trim();
            DateTime birthday = dtpBirthday.Value.Date;
            string gender = cmbGender.Text;
            string cccd = txtCCCD.Text.Trim();
            string street = txtStreet.Text.Trim();
            string district = txtDistrict.Text.Trim();
            string city = txtCity.Text.Trim();
            string phone = txtPhone.Text.Trim();
            decimal basicSalary = numBasicSalary.Value;
            string managerId = string.IsNullOrWhiteSpace(txtManagerID.Text) ? null : txtManagerID.Text.Trim();
            string centerId = txtCenterID.Text.Trim();
            string role = cmbRole.Text;

            // Basic client-side validation
            if (string.IsNullOrEmpty(fullName))
            {
                MessageBox.Show("Vui lòng nhập họ tên.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            if (basicSalary <= 0)
            {
                MessageBox.Show("Lương cơ bản phải lớn hơn 0.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numBasicSalary.Focus();
                return;
            }

            if (string.IsNullOrEmpty(cccd))
            {
                MessageBox.Show("Vui lòng nhập CCCD.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCCCD.Focus();
                return;
            }

            if (string.IsNullOrEmpty(centerId))
            {
                MessageBox.Show("Vui lòng nhập CenterID.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCenterID.Focus();
                return;
            }

            if (string.IsNullOrEmpty(role))
            {
                MessageBox.Show("Vui lòng chọn Role.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbRole.Focus();
                return;
            }

            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new SqlCommand("sp_Admin_AccountEmployees_Add", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 255) { Value = fullName });
                    cmd.Parameters.Add(new SqlParameter("@Birthday", SqlDbType.Date) { Value = birthday });
                    cmd.Parameters.Add(new SqlParameter("@Gender", SqlDbType.NVarChar, 10) { Value = gender });
                    cmd.Parameters.Add(new SqlParameter("@CCCD", SqlDbType.NVarChar, 20) { Value = cccd });
                    cmd.Parameters.Add(new SqlParameter("@Street", SqlDbType.NVarChar, 255) { Value = street });
                    cmd.Parameters.Add(new SqlParameter("@District", SqlDbType.NVarChar, 255) { Value = district });
                    cmd.Parameters.Add(new SqlParameter("@City", SqlDbType.NVarChar, 255) { Value = city });
                    cmd.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.NVarChar, 20) { Value = phone });
                    var pSalary = new SqlParameter("@BasicSalary", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = basicSalary };
                    cmd.Parameters.Add(pSalary);
                    cmd.Parameters.Add(new SqlParameter("@ManagerID", SqlDbType.NVarChar, 50) { Value = (object)managerId ?? DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@CenterID", SqlDbType.NVarChar, 50) { Value = centerId });
                    cmd.Parameters.Add(new SqlParameter("@Role", SqlDbType.NVarChar, 50) { Value = role });

                    var returnParam = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue,
                        SqlDbType = SqlDbType.Int
                    };
                    cmd.Parameters.Add(returnParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    int ret = (returnParam.Value != DBNull.Value) ? Convert.ToInt32(returnParam.Value) : 0;

                    // Map procedure return codes (your proc returns negative codes on error)
                    if (ret < 0)
                    {
                        string msg;
                        switch (ret)
                        {
                            case -1:
                                msg = "Lỗi: Lương cơ bản (BasicSalary) phải lớn hơn 0.";
                                break;
                            case -2:
                                msg = "Lỗi: Đã đạt giới hạn tối đa mã nhân viên (EMP9999).";
                                break;
                            case -3:
                                msg = "Lỗi: Số CCCD đã tồn tại trong hệ thống.";
                                break;
                            case -4:
                                msg = "Lỗi: Mã người quản lý (ManagerID) không tồn tại trong hệ thống.";
                                break;
                            case -5:
                                msg = "Lỗi: Người được chỉ định không có quyền Quản lý (Manager).";
                                break;
                            default:
                                msg = "Lỗi không rõ từ stored procedure.";
                                break;
                        }

                        MessageBox.Show(msg, "Add Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Success (procedure currently might not RETURN 1; treat 0 or positive as success)
                    MessageBox.Show("Thêm nhân viên thành công.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi SQL khi thêm nhân viên: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm nhân viên: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
