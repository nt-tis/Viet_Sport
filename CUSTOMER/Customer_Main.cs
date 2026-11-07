//using CUSTOMER;
//using LOGIN
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
namespace CUSTOMER
{
    public partial class Customer_Main : Form
    {
        private Form _currentForm;
        private Panel panelMain;

        private readonly Color _menuNormalColor = Color.ForestGreen;
        private readonly Color _menuActiveColor = Color.FromArgb(192, 64, 0);

        private readonly string _customerId;
        private readonly string _fullName;
        private readonly string _role;
        private readonly string _username;
        private readonly string _level;
        private decimal _balanceValue; // Thay đổi: lưu giá trị decimal gốc
        private string _balance;

        private List<Button> _menuButtons;

        // Thêm properties public để truy cập
        public string CustomerId => _customerId;
        public string Level => _level;
        public decimal BalanceValue => _balanceValue;
        public string FullName => _fullName;
        public string UserName => _username;



        public Customer_Main()
        {
            InitializeComponent();
            InitializeContentPanel();
            InitializeMenu();
        }

        private void ApplyUserInfo()
        {
            EmployeeID.Text = string.IsNullOrWhiteSpace(_customerId) ? "N/A" : _customerId;
            EmployeeName.Text = string.IsNullOrWhiteSpace(_fullName) ? "N/A" : _fullName;
            Role.Text = string.IsNullOrWhiteSpace(_role) ? "N/A" : _role;
            Username.Text = string.IsNullOrWhiteSpace(_username) ? "N/A" : _username;
            MembershipLevel.Text = string.IsNullOrWhiteSpace(_level) ? "N/A" : _level;
            Balance.Text = string.IsNullOrWhiteSpace(_username) ? "N/A" : _balance;
        }

        public Customer_Main(string employeeId, string fullName, string role, string username, string level, decimal balance)
        {
            InitializeComponent();

            _customerId = employeeId;
            _fullName = fullName;
            _role = role;
            _username = username;
            _level = level;
            _balanceValue = balance; // Lưu giá trị gốc
            _balance = string.Format("{0:N0} VNĐ", balance);

            InitializeContentPanel();
            InitializeMenu();

            ActiveButton(btnViewCourt);
            OpenForm(new View_Court(_customerId));
            ApplyUserInfo();
        }

        private void InitializeContentPanel()
        {
            panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            Controls.Add(panelMain);
            panelMain.BringToFront();
        }

        private void InitializeMenu()
        {
            _menuButtons = new List<Button>
            {
                btnViewCourt,
                btnBookedCourts,
                btnDepositMoney,
                btnChangePass,
                btnLogOut
            };

            btnViewCourt.Click += (s, e) =>
            {
                ActiveButton(btnViewCourt);
                OpenForm(new View_Court(_customerId));
            };
           

            btnBookedCourts.Click += (s, e) =>
            {
                ActiveButton(btnBookedCourts);
                OpenForm(new View_BookedCourts(_customerId));
            };

            btnChangePass.Click += (s, e) =>
            {
                ActiveButton(btnChangePass);
                OpenForm(new CustomerChangePassword(this));
            };

            btnDepositMoney.Click += (s, e) =>
            {
                ActiveButton(btnDepositMoney);
                ShowDepositMoneyDialog();
            };

            btnLogOut.Click += (s, e) =>
            {
                ActiveButton(btnLogOut);
                if (MessageBox.Show("Bạn chắc chắn muốn đăng xuất không?", "Xác nhận đăng xuất",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Close();
                }
                else
                {
                    ActiveButton(btnViewCourt);
                    OpenForm(new View_Court(_customerId));
                }
            };
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

        private void OpenForm(Form form)
        {
            if (_currentForm != null && !_currentForm.IsDisposed)
            {
                panelMain.Controls.Remove(_currentForm);
                _currentForm.Close();
                _currentForm.Dispose();
            }

            _currentForm = form;
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;

            panelMain.Controls.Clear();
            panelMain.Controls.Add(form);
            form.Show();
            form.BringToFront();
        }

        public void OpenViewCourt()
        {
            ActiveButton(btnViewCourt);
            OpenForm(new View_Court(_customerId));
        }

        public void OpenBookedCourts()
        {
            ActiveButton(btnBookedCourts);
            OpenForm(new View_BookedCourts(_customerId));
        }

        public void UpdateBalance(decimal newBalance)
        {
            _balanceValue = newBalance;
            _balance = string.Format("{0:N0} VNĐ", newBalance);
            // Cập nhật hiển thị trên form nếu có label/textbox hiển thị số dư
            Balance.Text = _balance;
            // Ví dụ: lblBalance.Text = string.Format("{0:N0} VNĐ", newBalance);
        }

        private void ShowDepositMoneyDialog()
        {
            // Tạo form dialog để nhập số tiền
            Form depositForm = new Form
            {
                Text = "Nạp tiền",
                Width = 400,
                Height = 200,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label lblAmount = new Label
            {
                Text = "Số tiền cần nạp (VNĐ):",
                Location = new Point(20, 20),
                AutoSize = true
            };

            TextBox txtAmount = new TextBox
            {
                Location = new Point(20, 50),
                Width = 340,
                Font = new Font("Microsoft Sans Serif", 12F)
            };

            Label lblCurrentBalance = new Label
            {
                Text = $"Số dư hiện tại: {_balanceValue:N0} VNĐ",
                Location = new Point(20, 80),
                AutoSize = true,
                ForeColor = Color.Blue
            };

            Button btnConfirm = new Button
            {
                Text = "Xác nhận",
                Location = new Point(180, 110),
                Width = 80,
                DialogResult = DialogResult.OK
            };

            Button btnCancel = new Button
            {
                Text = "Hủy",
                Location = new Point(270, 110),
                Width = 80,
                DialogResult = DialogResult.Cancel
            };

            depositForm.Controls.AddRange(new Control[] { lblAmount, txtAmount, lblCurrentBalance, btnConfirm, btnCancel });
            depositForm.AcceptButton = btnConfirm;
            depositForm.CancelButton = btnCancel;

            if (depositForm.ShowDialog() == DialogResult.OK)
            {
                if (decimal.TryParse(txtAmount.Text, out decimal amount) && amount > 0)
                {
                    DepositMoney(amount);
                }
                else
                {
                    MessageBox.Show("Vui lòng nhập số tiền hợp lệ (lớn hơn 0)", "Lỗi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DepositMoney(decimal amount)
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_NapTien", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@CustomerID", _customerId);
                        cmd.Parameters.AddWithValue("@MoneyAmount", amount);

                        cmd.ExecuteNonQuery();

                        // Cập nhật số dư mới
                        decimal newBalance = _balanceValue + amount;
                        UpdateBalance(newBalance);

                        MessageBox.Show(
                            $"Nạp tiền thành công!\nSố tiền nạp: {amount:N0} VNĐ\nSố dư mới: {newBalance:N0} VNĐ",
                            "Thành công",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    $"Lỗi nạp tiền: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi không xác định: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private void LoadCustomerBalance()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    
                    string query = @"SELECT alc.Balance 
                                    FROM ACCOUNT_LOGIN_CUSTOMER alc
                                    JOIN CUSTOMER c ON c.AccountLogin = alc.AccountLoginC
                                    WHERE c.CustomerID = @CustomerID";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", _customerId);
                        object result = cmd.ExecuteScalar();
                        
                        if (result != null && result != DBNull.Value)
                        {
                            decimal newBalance = Convert.ToDecimal(result);
                            UpdateBalance(newBalance);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi load số dư: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        public void RefreshBalance()
        {
            LoadCustomerBalance();
        }
    }
}
