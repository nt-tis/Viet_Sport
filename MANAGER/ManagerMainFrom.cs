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


namespace MANAGER
{
    public partial class ManagerMainFrom : Form
    {
        // ===== CƠ CHẾ CACHE FORM (GIỐNG MẪU) =====
        private readonly Dictionary<Type, Form> _cache = new Dictionary<Type, Form>();
        private Form _currentForm;

        // ===== THÔNG TIN LOGIN (CÓ THỂ GÁN TỪ LOGIN) =====
        private readonly string _username;    // MN0001
        private readonly string _employeeId;  // EMP0001 (nếu có)
        private readonly string _fullName;
        private readonly string _role;

        // ===== MENU STYLE =====
        private List<Button> _menuButtons;
        private readonly Color _sidebarColor = Color.ForestGreen;
        private readonly Color _activeColor = Color.SaddleBrown;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED (chống flicker)
                return cp;
            }
        }

        // ===== CTOR DESIGNER =====
        public ManagerMainFrom()
        {
            InitializeComponent();

            panelMenu.Dock = DockStyle.Left;
            panelMain.Dock = DockStyle.Fill;
            panelMenu.SendToBack();
            panelMain.BringToFront();
            panelMain.Padding = new Padding(12, 0, 0, 0);

            EnableDoubleBuffer(panelMain);

            // ✅ gom button menu (đúng tên của m)
            _menuButtons = new List<Button>
            {
                button9, // Employees
                button8,
                button7,
                button6,
                button5,
                button4,
                button3,
                button2
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

            this.Shown += ManagerMainFrom_Shown;
        }

        // ===== CTOR LOGIN (GIỐNG MẪU) =====
        // Nếu login m chỉ có MN0001 thì truyền username là đủ, còn employeeId/role/fullName muốn giữ thì truyền.
        public ManagerMainFrom(string employeeId, string fullName, string role, string username) : this()
        {
            _employeeId = employeeId;
            _fullName = fullName;
            _role = role;
            _username = username;
        }

        // ===== GETTER CHO FORM CON (LẤY TỪ LABEL) =====
        public string CurrentUsername => Username.Text;     // MN0001
        public string CurrentEmployeeID => EmployeeID.Text; // EMP0001

        // ===== APPLY USER INFO (GIỐNG MẪU) =====
        private void ApplyUserInfo()
        {
            // Demo/fallback nếu login chưa truyền
            EmployeeID.Text = string.IsNullOrWhiteSpace(_employeeId) ? "EMP0001" : _employeeId;
            Username.Text = string.IsNullOrWhiteSpace(_username) ? "MN0001" : _username;
            EmployeeName.Text = string.IsNullOrWhiteSpace(_fullName) ? "N/A" : _fullName;
            Role.Text = string.IsNullOrWhiteSpace(_role) ? "N/A" : _role;
        }

        private void ManagerMainFrom_Shown(object sender, EventArgs e)
        {
            ApplyUserInfo();

            // ✅ mặc định mở Employees (button9)
            SetActiveButton(button9);
            BeginInvoke(new Action(() => LoadForm<ManagerEmployees>()));
        }

        private static void EnableDoubleBuffer(Control c)
        {
            typeof(Control)
                .GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(c, true, null);
        }

        // ===== ACTIVE BUTTON (GIỐNG MẪU) =====
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

        // ===== LOAD CHILD FORM GENERIC + CACHE + INJECT MAIN (GIỐNG MẪU) =====
        public void LoadForm<T>() where T : Form
        {
            if (panelMain == null) return;

            var key = typeof(T);

            if (!_cache.TryGetValue(key, out var frm) || frm == null || frm.IsDisposed)
            {
                // ưu tiên ctor(ManagerMainFrom), fallback ctor mặc định
                var ctor = typeof(T).GetConstructor(new[] { typeof(ManagerMainFrom) });
                frm = ctor != null
                    ? (Form)ctor.Invoke(new object[] { this })
                    : (Form)Activator.CreateInstance(typeof(T));

                frm.TopLevel = false;
                frm.FormBorderStyle = FormBorderStyle.None;
                frm.Dock = DockStyle.Fill;
                frm.Visible = false;

                // Ẩn panel2 thừa (placeholder sidebar) và cho panel1 fill trọn vẹn
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

            // Đảm bảo panel1 fill và panel2 ẩn kể cả khi lấy từ cache
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

        // ===== MENU EVENTS (GIỐNG MẪU) =====
        private void button9_Click(object sender, EventArgs e) // Employees
        {
            SetActiveButton(button9);
            LoadForm<ManagerEmployees>();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SetActiveButton(button8);
            LoadForm<ManagerWorkSchedule>();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SetActiveButton(button7);
            LoadForm<ManagerDiscount>();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SetActiveButton(button6);
            LoadForm<ManagerReport>();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SetActiveButton(button5);
            LoadForm<ManagerApproveRequest>();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SetActiveButton(button4);
            LoadForm<ManagerPayroll>();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SetActiveButton(button3);
            LoadForm<ManagerChangePassword>();
        }

        private void button2_Click(object sender, EventArgs e) // Logout
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

            // NO => quay lại Employees
            SetActiveButton(button9);
            LoadForm<ManagerEmployees>();
        }

        private void panelMain_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}




