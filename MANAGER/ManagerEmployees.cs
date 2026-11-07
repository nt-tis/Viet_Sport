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


namespace MANAGER
{
    public partial class ManagerEmployees : Form
    {
        private static string _connStr =
             "Data Source=.;Initial Catalog=VIET_SPORT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";
        private readonly ManagerMainFrom _main;   // ✅ ĐÚNG TÊN MAIN
        private string _employeeID;               // lấy từ label
        private string _centerID;                 // suy ra từ employeeID
        private readonly DataTable _dtEmployees = new DataTable();

        public ManagerEmployees(ManagerMainFrom main)
        {
            InitializeComponent();
            _main = main;

            this.Load += ManagerEmployees_Load;
            btnSearch.Click += btnSearch_Click;
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
        }

        private void ManagerEmployees_Load(object sender, EventArgs e)
        {
            SetupGenderCombo();
            SetupGrid();

            // ✅ LẤY EMPLOYEEID TỪ LABEL MAIN
            _employeeID = _main.CurrentEmployeeID; // hoặc _main.mid nếu m có property mid

            if (string.IsNullOrWhiteSpace(_employeeID) || _employeeID == "N/A")
            {
                MessageBox.Show("Không lấy được EmployeeID từ MainForm.");
                this.Close();
                return;
            }

            LoadCenterByEmployeeID();
            LoadEmployees();
        }

        private void LoadCenterByEmployeeID()
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT CenterID FROM EMPLOYEE WHERE EmployeeID = @EID", conn))
            {
                cmd.Parameters.AddWithValue("@EID", _employeeID);
                conn.Open();

                object result = cmd.ExecuteScalar();
                if (result == null)
                {
                    MessageBox.Show("Không suy ra được CenterID từ EmployeeID!");
                    this.Close();
                    return;
                }

                _centerID = result.ToString();
            }
        }

        private void SetupGenderCombo()
        {
            cboGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cboGender.Items.Clear();
            cboGender.Items.Add("All");
            cboGender.Items.Add("Nam");
            cboGender.Items.Add("Nữ");
            cboGender.SelectedIndex = 0;
        }

        private void SetupGrid()
        {
            dgvEmployees.AutoGenerateColumns = true;
            dgvEmployees.AllowUserToAddRows = true;
            dgvEmployees.MultiSelect = false;
            dgvEmployees.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void LoadEmployees()
        {
            string empId = txtEmployeeID.Text.Trim();
            string empName = txtEmployeeName.Text.Trim();

            string gender = null;
            if (cboGender.SelectedItem != null &&
                !cboGender.SelectedItem.ToString().Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                gender = cboGender.SelectedItem.ToString();
            }

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand("sp_QL1_Manager_ViewEmployees", conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CenterID", _centerID);
                cmd.Parameters.AddWithValue("@EmployeeID", string.IsNullOrWhiteSpace(empId) ? (object)DBNull.Value : empId);
                cmd.Parameters.AddWithValue("@EmployeeName", string.IsNullOrWhiteSpace(empName) ? (object)DBNull.Value : empName);
                cmd.Parameters.AddWithValue("@Gender", gender == null ? (object)DBNull.Value : gender);

                _dtEmployees.Clear();
                conn.Open();
                da.Fill(_dtEmployees);
            }

            dgvEmployees.DataSource = _dtEmployees;
            AddCheckBoxColumnAtEnd();
        }

        private void AddCheckBoxColumnAtEnd()
        {
            if (!dgvEmployees.Columns.Contains("chkSelect"))
            {
                dgvEmployees.Columns.Add(new DataGridViewCheckBoxColumn
                {
                    Name = "chkSelect",
                    HeaderText = "Select",
                    Width = 60
                });
            }
        }

        private DataRow GetCheckedRow()
        {
            foreach (DataGridViewRow row in dgvEmployees.Rows)
            {
                if (row.Cells["chkSelect"].Value != null &&
                    Convert.ToBoolean(row.Cells["chkSelect"].Value))
                {
                    return (row.DataBoundItem as DataRowView)?.Row;
                }
            }
            return null;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try { LoadEmployees(); }
            catch (Exception ex) { MessageBox.Show("Search failed:\n" + ex.Message); }
        }

        // ===== HELPERS (DÁN TRONG CLASS ManagerEmployees) =====
        private static object DbStrOrNull(object v)
        {
            if (v == null || v == DBNull.Value) return DBNull.Value;
            var s = v.ToString().Trim();
            return string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s;
        }

        private static object DbDateOrNull(object v)
        {
            if (v == null || v == DBNull.Value) return DBNull.Value;

            if (v is DateTime dt) return dt;

            if (DateTime.TryParse(v.ToString(), out var d))
                return d;

            return DBNull.Value;
        }

        private static object DbDecimalOrNull(object v)
        {
            if (v == null || v == DBNull.Value) return DBNull.Value;

            if (v is decimal dec) return dec;

            if (decimal.TryParse(v.ToString(), out var x))
                return x;

            return DBNull.Value;
        }

        private static bool IsValidRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return false;

            // 5 role hợp lệ
            return role.Equals("Manager", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Cashier", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Receptionist", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Technician", StringComparison.OrdinalIgnoreCase);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            dgvEmployees.EndEdit();

            DataRow row = GetCheckedRow();
            if (row == null)
            {
                MessageBox.Show("Tick Select 1 nhân viên để UPDATE.");
                return;
            }

            string role = row["Role"]?.ToString();
            if (!IsValidRole(role))
            {
                MessageBox.Show("Role không hợp lệ! Chỉ được: Manager, Cashier, Admin, Receptionist, Technician.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand("sp_QL1_Manager_UpdateEmployee", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // dùng type chuẩn
                    cmd.Parameters.Add("@CenterID", SqlDbType.NVarChar, 50).Value = _centerID;
                    cmd.Parameters.Add("@EmployeeID", SqlDbType.NVarChar, 50).Value = row["EmployeeID"].ToString();

                    cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 255).Value = DbStrOrNull(row["FullName"]);
                    cmd.Parameters.Add("@Birthday", SqlDbType.Date).Value = DbDateOrNull(row["Birthday"]);
                    cmd.Parameters.Add("@Gender", SqlDbType.NVarChar, 10).Value = DbStrOrNull(row["Gender"]);
                    cmd.Parameters.Add("@CCCD", SqlDbType.NVarChar, 20).Value = DbStrOrNull(row["CCCD"]);
                    cmd.Parameters.Add("@Street", SqlDbType.NVarChar, 255).Value = DbStrOrNull(row["Street"]);
                    cmd.Parameters.Add("@District", SqlDbType.NVarChar, 255).Value = DbStrOrNull(row["District"]);
                    cmd.Parameters.Add("@City", SqlDbType.NVarChar, 255).Value = DbStrOrNull(row["City"]);
                    cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 20).Value = DbStrOrNull(row["PhoneNumber"]);
                    cmd.Parameters.Add("@BasicSalary", SqlDbType.Decimal).Value = DbDecimalOrNull(row["BasicSalary"]);

                    // decimal phải set precision/scale
                    var pSalary = cmd.Parameters["@BasicSalary"];
                    pSalary.Precision = 10;
                    pSalary.Scale = 2;

                    cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 30).Value = role.Trim();

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("UPDATE thành công!");
                LoadEmployees();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL Error:\n" + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update failed:\n" + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            dgvEmployees.EndEdit();

            DataRow row = GetCheckedRow();
            if (row == null)
            {
                MessageBox.Show("Tick Select dòng mới để ADD.");
                return;
            }

            string empId = row["EmployeeID"]?.ToString();
            string fullName = row["FullName"]?.ToString();
            string role = row["Role"]?.ToString();

            if (string.IsNullOrWhiteSpace(empId) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("ADD cần EmployeeID, FullName và Role.");
                return;
            }

            if (!IsValidRole(role))
            {
                MessageBox.Show("Role không hợp lệ! Chỉ được: Manager, Cashier, Admin, Receptionist, Technician.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand("sp_QL1_Manager_AddEmployee", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CenterID", SqlDbType.NVarChar, 50).Value = _centerID;
                    cmd.Parameters.Add("@ManagerEmployeeID", SqlDbType.NVarChar, 50).Value = _main.CurrentEmployeeID;

                    cmd.Parameters.Add("@EmployeeID", SqlDbType.NVarChar, 50).Value = empId.Trim();
                    cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 255).Value = fullName.Trim();

                    cmd.Parameters.Add("@Birthday", SqlDbType.Date).Value = DbDateOrNull(row["Birthday"]);
                    cmd.Parameters.Add("@Gender", SqlDbType.NVarChar, 10).Value = DbStrOrNull(row["Gender"]);
                    cmd.Parameters.Add("@CCCD", SqlDbType.NVarChar, 20).Value = DbStrOrNull(row["CCCD"]);
                    cmd.Parameters.Add("@Street", SqlDbType.NVarChar, 255).Value = DbStrOrNull(row["Street"]);
                    cmd.Parameters.Add("@District", SqlDbType.NVarChar, 255).Value = DbStrOrNull(row["District"]);
                    cmd.Parameters.Add("@City", SqlDbType.NVarChar, 255).Value = DbStrOrNull(row["City"]);
                    cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 20).Value = DbStrOrNull(row["PhoneNumber"]);
                    cmd.Parameters.Add("@BasicSalary", SqlDbType.Decimal).Value = DbDecimalOrNull(row["BasicSalary"]);

                    var pSalary = cmd.Parameters["@BasicSalary"];
                    pSalary.Precision = 10;
                    pSalary.Scale = 2;

                    cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 30).Value = role.Trim();
                

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("ADD thành công!");
                LoadEmployees();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL Error:\n" + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Add failed:\n" + ex.Message);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}