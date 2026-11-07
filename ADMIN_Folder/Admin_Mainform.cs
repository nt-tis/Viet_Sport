using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ADMIN
{
    public partial class Admin_Mainform : Form
    {
        private Form activeChildForm = null;
        private Button currentButton = null;
        private Color defaultButtonBackColor;
        private readonly string _username;    // MN0001
        private readonly string _employeeId;  // EMP0001 (nếu có)
        private readonly string _fullName;
        private readonly string _role;

        private void ApplyUserInfo()
        {
            // Demo/fallback nếu login chưa truyền
            lblEmployeeID.Text = string.IsNullOrWhiteSpace(_employeeId) ? "EMP0001" : _employeeId;
            lblUsername.Text = string.IsNullOrWhiteSpace(_username) ? "MN0001" : _username;
            lblEmployeeName.Text = string.IsNullOrWhiteSpace(_fullName) ? "N/A" : _fullName;
            lblRole.Text = string.IsNullOrWhiteSpace(_role) ? "N/A" : _role;
        }

        public Admin_Mainform()
        {
            InitializeComponent();

            // Make scaling and font explicit so child forms render at the same logical size
            this.AutoScaleMode = AutoScaleMode.None;
            this.Font = new Font("Segoe UI", 9F);
            this.ClientSize = new Size(1100, 700); // choose a fixed client size your app expects
            this.StartPosition = FormStartPosition.CenterScreen;

            // store default button color for later restore
            defaultButtonBackColor = btnCreateInvoice?.BackColor ?? SystemColors.Control;

            // wire common handler for menu buttons
            btnCreateInvoice.Click += MenuButton_Click;
            btnPayment.Click += MenuButton_Click; // System Parameter button
            btnWorkSchedule.Click += MenuButton_Click;
            btnLeaveApplication.Click += MenuButton_Click;
            btnPayroll.Click += MenuButton_Click;
            btnChangePassword.Click += MenuButton_Click;
            btnLogout.Click += MenuButton_Click;
        }

        public Admin_Mainform(string username, string employeeId, string fullName, string role) : this() 
        {
            _employeeId = employeeId;
            _fullName = fullName;
            _role = role;
            _username = username;
            ApplyUserInfo();
        }

        private void btnCreateInvoice_Click(object sender, EventArgs e)
        {
            // kept for designer/backwards compatibility, delegate to shared handler
            MenuButton_Click(sender, e);
        }

        private void OpenChildForm(Form childForm)
        {
            if (childForm == null)
                return;

            // close existing child
            if (activeChildForm != null)
            {
                activeChildForm.Close();
            }

            activeChildForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panelContent.Controls.Clear();
            panelContent.Controls.Add(childForm);
            panelContent.Tag = childForm;

            childForm.BringToFront();
            childForm.Show();
        }

        private void ActivateButton(Button btn)
        {
            if (btn == null)
                return;

            // reset previous
            if (currentButton != null && !currentButton.IsDisposed)
            {
                currentButton.BackColor = defaultButtonBackColor;
            }

            currentButton = btn;
            currentButton.BackColor = Color.Orange;
        }

        // Shared handler for menu buttons
        private void MenuButton_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            ActivateButton(btn);

            if (btn == btnCreateInvoice)
            {
                OpenChildForm(new Admin_AccountEmployees());
                return;
            }

            if (btn == btnPayment)
            {
                // Open Admin_SystemParameters inside the content panel
                OpenChildForm(new Admin_SystemParameters(_employeeId));
                return;
            }

            if (btn == btnWorkSchedule)
            {
                OpenChildForm(new Admin_WorkScheduleView());
                return;
            }

            if (btn == btnLeaveApplication)
            {
                OpenChildForm(new Admin_LeaveApplicationView());
                return;
            }

            if (btn == btnPayroll)
            {
                OpenChildForm(new Admin_PayrollView());
                return;
            }

            if (btn == btnChangePassword)
            {
                OpenChildForm(new Admin_ChangePassword(_employeeId, _fullName, _username));
                return;
            }

            if (btn == btnLogout)
            {
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
                    ActivateButton(btnCreateInvoice);
                    OpenChildForm(new Admin_AccountEmployees());
                }
                return;
            }
        }

        private void panelContent_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
