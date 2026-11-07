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

namespace CASHIER
{
    public partial class Cashier_Main : Form
    {
        private Form _currentForm;

        private readonly string _username;
        private readonly string _employeeId;
        private readonly string _fullName;
        private readonly string _role;
        private string _centerID;  // THÊM CENTERID

        private readonly Color _menuNormalColor = Color.ForestGreen;
        private readonly Color _menuActiveColor = Color.SaddleBrown;

        private List<Button> _menuButtons;

        private void ApplyUserInfo()
        {
            // Demo/fallback nếu login chưa truyền
            lblEmployeeID.Text = string.IsNullOrWhiteSpace(_employeeId) ? "EMP0001" : _employeeId;
            lblUsername.Text = string.IsNullOrWhiteSpace(_username) ? "MN0001" : _username;
            lblEmployeeName.Text = string.IsNullOrWhiteSpace(_fullName) ? "N/A" : _fullName;
            lblRole.Text = string.IsNullOrWhiteSpace(_role) ? "N/A" : _role;
        }

        public Cashier_Main()
        {
            InitializeComponent();

            panelMenu.Dock = DockStyle.Left;
            panelMain.Dock = DockStyle.Fill;

            _menuButtons = new List<Button>
            {
                btnCreateInvoice,
                btnPayment,
                btnCancellationFee,
                btnWorkSchedule,
                btnPayroll,
                btnLeaveApplication,
                btnChangePassword,
                btnLogout
            };

            foreach (var btn in _menuButtons)
            {
                btn.BackColor = _menuNormalColor;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.White;
            }

            this.Shown += (s, e) =>
            {
                ActiveButton(btnCreateInvoice);
                OpenForm(new Cashier_InvoiceCreate(_employeeId, _fullName, _username, _centerID));
            };
        }

        public Cashier_Main(string employeeId, string fullName, string role, string username) : this()
        {
            _employeeId = employeeId;
            _fullName = fullName;
            _role = role;
            _username = username;
            
            ApplyUserInfo();
            
            // Lấy CenterID của nhân viên
            LoadEmployeeCenterInfo();
        }

        private void LoadEmployeeCenterInfo()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_Cashier_GetEmployeeCenterID", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeID", _employeeId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        _centerID = dt.Rows[0]["CenterID"]?.ToString();

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải thông tin center:\n{ex.Message}", 
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ActiveButton(Button activeBtn)
        {
            if (activeBtn == null) return;

            foreach (var btn in _menuButtons)
            {
                btn.BackColor = _menuNormalColor;
            }

            activeBtn.BackColor = _menuActiveColor;
        }

        private void OpenForm(Form frm)
        {
            if (_currentForm != null && !_currentForm.IsDisposed)
            {
                panelMain.Controls.Remove(_currentForm);
                _currentForm.Close();
                _currentForm.Dispose();
            }

            _currentForm = frm;

            frm.TopLevel = false;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock = DockStyle.Fill;
            frm.Location = Point.Empty;
            frm.Margin = Padding.Empty;
            frm.Padding = Padding.Empty;

            panelMain.Controls.Clear();
            panelMain.Controls.Add(frm);

            frm.Show();

            frm.Width = panelMain.ClientSize.Width;
            frm.Height = panelMain.ClientSize.Height;

            frm.BringToFront();
        }

        private void btnCreateInvoice_Click(object sender, EventArgs e)
        {
            ActiveButton(btnCreateInvoice);
            OpenForm(new Cashier_InvoiceCreate(_employeeId, _fullName, _username, _centerID));
        }

        private void btnPayment_Click(object sender, EventArgs e)
        {
            ActiveButton(btnPayment);
            OpenForm(new Cashier_Payment(_employeeId, _fullName, _username, _centerID));
        }

        private void btnCancellationFee_Click(object sender, EventArgs e)
        {
            ActiveButton(btnCancellationFee);
            OpenForm(new Cashier_CancellationFee(_employeeId, _fullName, _username, _centerID));
        }

        private void btnWorkSchedule_Click(object sender, EventArgs e)
        {
            ActiveButton(btnWorkSchedule);
            OpenForm(new Cashier_WorkSchedule(_employeeId, _fullName, _username));
        }

        private void btnPayroll_Click(object sender, EventArgs e)
        {
            ActiveButton(btnPayroll);
            OpenForm(new Cashier_Payroll(_employeeId, _fullName, _username));
        }

        private void btnLeaveApplication_Click(object sender, EventArgs e)
        {
            ActiveButton(btnLeaveApplication);
            OpenForm(new Cashier_LeaveApplication(_employeeId, _fullName, _username));
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            ActiveButton(btnChangePassword);
            OpenForm(new Cashier_ChangePassword(_employeeId, _fullName, _username));
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            ActiveButton(btnLogout);

            var confirm = MessageBox.Show(
                "Bạn chắc chắn muốn đăng xuất không?",
                "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm == DialogResult.Yes)
            {
                this.Close();
            }
            else
            {
                ActiveButton(btnCreateInvoice);
                OpenForm(new Cashier_InvoiceCreate(_employeeId, _fullName, _username, _centerID));
            }
        }

        private void panelMain_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
