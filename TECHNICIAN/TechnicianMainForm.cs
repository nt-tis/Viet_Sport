using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TECHNICIAN
{
    public partial class TechnicianMainForm : Form
    {
        // ===== CACHE FORM (GIỐNG MANAGER) =====
        private readonly Dictionary<Type, Form> _cache = new Dictionary<Type, Form>();
        private Form _currentForm;

        // ===== THÔNG TIN LOGIN (TÙY BẠN TRUYỀN TỪ LOGIN) =====
        private readonly string _username;     // ví dụ: TC0001
        private readonly string _employeeId;   // ví dụ: EMP00xx
        private readonly string _fullName;
        private readonly string _role;

        // ===== MENU STYLE =====
        private List<Button> _menuButtons;
        private readonly Color _sidebarColor = Color.ForestGreen;
        private readonly Color _activeColor = Color.SaddleBrown;

        // ✅ chống flicker giống Manager
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        // ===== CTOR DESIGNER =====
        public TechnicianMainForm()
        {
            InitializeComponent();

            panelMenu.Dock = DockStyle.Left;
            panelMain.Dock = DockStyle.Fill;
            panelMenu.SendToBack();
            panelMain.BringToFront();
            panelMain.Padding = new Padding(12, 0, 0, 0);

            EnableDoubleBuffer(panelMain);

            // ✅ gom button menu (đúng tên bên designer của bạn)
            _menuButtons = new List<Button>
            {
                btnMaintenanceList, // Maintenance List
                btnWorkSchedule,    // Work Schedule
                button5,            // Leave Application
                btnReports,         // Reports
                button4,            // Payroll
                button3,            // Change Password
                button2             // Log Out
            };

            foreach (var btn in _menuButtons)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.UseVisualStyleBackColor = false;
                btn.BackColor = _sidebarColor;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.White;
            }

            this.Shown += TechnicianMainForm_Shown;
        }

        // ===== CTOR LOGIN (GIỐNG MANAGER) =====
        public TechnicianMainForm(string employeeId, string fullName, string role, string username) : this()
        {
            _employeeId = employeeId;
            _fullName = fullName;
            _role = role;
            _username = username;
        }

        // ===== GETTER CHO FORM CON (BẠN CÓ LABEL THÌ TRẢ VỀ LABEL, KO THÌ TRẢ VỀ FIELD) =====
        // Nếu form Technician có label giống Manager (EmployeeID, Username) thì đổi thành label.Text.
        public string CurrentUsername => Username.Text;
        public string CurrentEmployeeID => EmployeeID.Text;

        // (tuỳ bạn) nếu bạn có CenterID cần inject: public string CurrentCenterID => CenterIDLabel.Text; hoặc field

        // ===== APPLY USER INFO (NẾU BẠN CÓ LABEL HIỂN THỊ) =====
        // Nếu TechnicianMainForm có label EmployeeID/Username như Manager thì mở phần này.
        private void ApplyUserInfo()
        {
            // Ví dụ:
            EmployeeID.Text = string.IsNullOrWhiteSpace(_employeeId) ? "EMP0001" : _employeeId;
            Username.Text   = string.IsNullOrWhiteSpace(_username) ? "TC0001" : _username;
            EmployeeName.Text = string.IsNullOrWhiteSpace(_fullName) ? "EmployeeName" : _fullName;
        }

        private void TechnicianMainForm_Shown(object sender, EventArgs e)
        {
            ApplyUserInfo();

            // ✅ mặc định mở Maintenance List
            SetActiveButton(btnMaintenanceList);
            BeginInvoke(new Action(() => LoadForm<TechnicianMaintenanceList>()));
        }

        private static void EnableDoubleBuffer(Control c)
        {
            typeof(Control)
                .GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(c, true, null);
        }

        // ===== ACTIVE BUTTON (GIỐNG MANAGER) =====
        private void SetActiveButton(Button activeBtn)
        {
            foreach (var btn in _menuButtons)
            {
                btn.BackColor = _sidebarColor;
                btn.ForeColor = Color.White;
            }

            if (activeBtn == null) return;
            activeBtn.BackColor = _activeColor;
            activeBtn.ForeColor = Color.White;
        }

        // ===== LOAD CHILD FORM GENERIC + CACHE + INJECT MAIN (GIỐNG MANAGER) =====
        public void LoadForm<T>() where T : Form
        {
            if (panelMain == null) return;

            var key = typeof(T);

            if (!_cache.TryGetValue(key, out var frm) || frm == null || frm.IsDisposed)
            {
                // ✅ ưu tiên ctor(TechnicianMainForm), fallback ctor mặc định
                var ctor = typeof(T).GetConstructor(new[] { typeof(TechnicianMainForm) });
                frm = ctor != null
                    ? (Form)ctor.Invoke(new object[] { this })
                    : (Form)Activator.CreateInstance(typeof(T));

                frm.TopLevel = false;
                frm.FormBorderStyle = FormBorderStyle.None;
                frm.Dock = DockStyle.Fill;
                frm.Visible = false;

                var p2 = frm.Controls["panel2"];
                if (p2 != null)
                {
                    p2.Visible = false;
                    p2.Size = new Size(0, 0);
                }
                var p1 = frm.Controls["panel1"];
                if (p1 != null) p1.Dock = DockStyle.Fill;

                panelMain.Controls.Add(frm);
                _cache[key] = frm;
            }

            var panel2Cached = frm.Controls["panel2"];
            if (panel2Cached != null) panel2Cached.Visible = false;
            var panel1Cached = frm.Controls["panel1"];
            if (panel1Cached != null) panel1Cached.Dock = DockStyle.Fill;

            if (_currentForm != null && !_currentForm.IsDisposed && !ReferenceEquals(_currentForm, frm))
                _currentForm.Visible = false;

            frm.Visible = true;
            frm.BringToFront();
            frm.Focus();

            _currentForm = frm;
            panelMenu.SendToBack();
            panelMain.BringToFront();
        }

        // =========================
        // ✅ MENU EVENTS (GIỐNG MANAGER)
        // =========================

        private void btnMaintenanceList_Click_1(object sender, EventArgs e)
        {
            SetActiveButton(btnMaintenanceList);
            LoadForm<TechnicianMaintenanceList>();
        }

        private void button8_Click(object sender, EventArgs e) // Work Schedule (bạn đặt handler tên button8_Click)
        {
            SetActiveButton(btnWorkSchedule);
            LoadForm<TechnicianWorkSchedule>();
        }

        private void button5_Click(object sender, EventArgs e) // Leave Application
        {
            SetActiveButton(button5);
            LoadForm<TechnicianLeaveApplication>();
        }

        private void btnReports_Click_1(object sender, EventArgs e)
        {
            SetActiveButton(btnReports);
            LoadForm<TechnicianReports>();
        }

        private void button4_Click(object sender, EventArgs e) // Payroll
        {
            SetActiveButton(button4);
            LoadForm<TechnicianPayroll>();
        }

        private void button3_Click(object sender, EventArgs e) // Change Password
        {
            SetActiveButton(button3);
            LoadForm<TechnicianChangePassword>();
        }

        private void button2_Click(object sender, EventArgs e) // Log Out
        {
            SetActiveButton(button2);

            var confirm = MessageBox.Show(
                "Bạn chắc chắn muốn đăng xuất không?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                this.Close();
                return;
            }

            // NO => quay lại Maintenance List
            SetActiveButton(btnMaintenanceList);
            LoadForm<TechnicianMaintenanceList>();
        }

        private void panelMain_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}