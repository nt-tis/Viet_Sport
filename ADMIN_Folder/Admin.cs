using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADMIN
{
    public partial class Admin : Form
    {
        private Form currentChildForm;
        private readonly Dictionary<Button, Color> originalButtonBackColors = new Dictionary<Button, Color>();

        public Admin()
        {
            InitializeComponent();

            // Cache original menu button colors so we can restore them later
            CacheMenuButtonColors();

            // Kiểm tra kết nối database
            if (!DatabaseConnection.TestConnection())
            {
                MessageBox.Show("Không thể kết nối đến database!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CacheMenuButtonColors()
        {
            // Collect direct children buttons of panel2 (left menu)
            if (panel2 == null) return;

            foreach (Control c in panel2.Controls)
            {
                if (c is Button b)
                {
                    // Store original BackColor (if duplicate keys exist, ignore)
                    if (!originalButtonBackColors.ContainsKey(b))
                        originalButtonBackColors.Add(b, b.BackColor);
                }
            }
        }

        private void ResetMenuHighlights()
        {
            foreach (var kv in originalButtonBackColors)
            {
                if (!kv.Key.IsDisposed)
                    kv.Key.BackColor = kv.Value;
            }
        }

        private void HighlightMenuButton(Button btn)
        {
            if (btn == null) return;
            ResetMenuHighlights();

            // Use a muted orange to avoid overly bright yellow
            btn.BackColor = Color.FromArgb(255, 165, 0); // orange
        }

        // Phương thức để hiển thị form trong panel
        private void OpenChildForm(Form childForm)
        {
            // Đóng form con hiện tại nếu có
            if (currentChildForm != null)
            {
                currentChildForm.Close();
            }

            currentChildForm = childForm;

            // Xóa tất cả control hiện tại trong panelMain
            panelMain.Controls.Clear();
            
            // Cấu hình form con
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            // Ẩn panel menu bên trái (panel2) trong form con
            Control panel2Child = childForm.Controls.Find("panel2", false).FirstOrDefault();
            if (panel2Child != null)
            {
                panel2Child.Visible = false;
            }

            // Ẩn panel header (panel1) trong form con
            Control panel1Child = childForm.Controls.Find("panel1", false).FirstOrDefault();
            if (panel1Child != null)
            {
                panel1Child.Visible = false;
            }

            // Điều chỉnh panelMain của form con để fill toàn bộ
            Control panelMainChild = childForm.Controls.Find("panelMain", false).FirstOrDefault();
            if (panelMainChild != null)
            {
                panelMainChild.Dock = DockStyle.Fill;
                panelMainChild.Location = new Point(0, 0);
            }
            
            // Thêm form con vào panel và hiển thị
            panelMain.Controls.Add(childForm);
            childForm.BringToFront();
            childForm.Show();
        }

        // Event handler cho nút View
        private void button16_Click(object sender, EventArgs e)
        {
            HighlightMenuButton(button16);
            OpenChildForm(new Admin_SystemInformationView());
        }

        // Event handler cho nút Notify
        private void button4_Click(object sender, EventArgs e)
        {
            HighlightMenuButton(button4);
            OpenChildForm(new Admin_Notify());
        }

        // Event handler cho nút Payroll
        private void button9_Click(object sender, EventArgs e)
        {
            HighlightMenuButton(button9);
            OpenChildForm(new Admin_Payroll());
        }

        // Event handler cho nút System Parameters (button10)
        private void button10_Click(object sender, EventArgs e)
        {
            HighlightMenuButton(button10);
            OpenChildForm(new Admin_SystemParameters());
        }

        // Event handler cho nút System Parameters (alternate, if present)
        private void button15_Click(object sender, EventArgs e)
        {
            // If button15 is not the System Parameters button, remove or change accordingly.
            HighlightMenuButton(button15);
            OpenChildForm(new Admin_SystemParameters());
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {


        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Highlight the clicked menu button and open the Account Employees form inside panelMain
            HighlightMenuButton(button5);
            OpenChildForm(new Admin_AccountEmployees());
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void button14_Click(object sender, EventArgs e)
        {

        }

        private void panelMain_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
