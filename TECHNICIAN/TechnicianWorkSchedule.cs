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

namespace TECHNICIAN
{
    public partial class TechnicianWorkSchedule : Form
    {
        private readonly string _connStr = "Data Source=.;Initial Catalog=VIET_SPORT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";

        private readonly TechnicianMainForm _main;
        private string _techEmployeeID;
        private readonly DataTable _dt = new DataTable();

        // ✅ ctor nhận main (giống các form khác)
        public TechnicianWorkSchedule(TechnicianMainForm main)
        {
            InitializeComponent();
            _main = main;
            this.Load += TechnicianWorkSchedule_Load;
        }

        // ✅ ctor default (designer)
        public TechnicianWorkSchedule()
        {
            InitializeComponent();
            this.Load += TechnicianWorkSchedule_Load;
        }

        private void TechnicianWorkSchedule_Load(object sender, EventArgs e)
        {
            SetupDatePicker();
            SetupGrid();

            if (_main == null)
            {
                MessageBox.Show("Form chưa nhận MainForm (TechnicianMainForm).");
                return;
            }

            _techEmployeeID = _main.CurrentEmployeeID;
            if (string.IsNullOrWhiteSpace(_techEmployeeID) || _techEmployeeID == "N/A")
            {
                MessageBox.Show("Không lấy được EmployeeID của Technician từ MainForm.");
                return;
            }

            // mặc định hôm nay
            dtpDate.Value = DateTime.Today;

        }

        // ================= UI =================
        private void SetupDatePicker()
        {
            dtpDate.ShowCheckBox = false;
            dtpDate.Format = DateTimePickerFormat.Custom;
            dtpDate.CustomFormat = "dd/MM/yyyy";
        }

        private void SetupGrid()
        {
            dgvSchedule.AutoGenerateColumns = true;
            dgvSchedule.AllowUserToAddRows = false;
            dgvSchedule.MultiSelect = false;
            dgvSchedule.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSchedule.RowHeadersVisible = false;
            dgvSchedule.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // ================= LOAD DATA =================
        private void LoadSchedule()
        {
            DateTime workDate = dtpDate.Value.Date;

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand("sp_TN2_Tech_GetWorkScheduleByDate", conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmployeeID", _techEmployeeID);
                cmd.Parameters.AddWithValue("@WorkDate", workDate);

                _dt.Clear();
                conn.Open();
                da.Fill(_dt);
            }

            dgvSchedule.DataSource = _dt;

            // header đẹp
            if (dgvSchedule.Columns.Contains("EmployeeID")) dgvSchedule.Columns["EmployeeID"].HeaderText = "Employee";
            if (dgvSchedule.Columns.Contains("WorkDate"))
            {
                dgvSchedule.Columns["WorkDate"].HeaderText = "Work Date";
                dgvSchedule.Columns["WorkDate"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            if (dgvSchedule.Columns.Contains("WorkTime"))
            {
                dgvSchedule.Columns["WorkTime"].HeaderText = "Work Time";
                dgvSchedule.Columns["WorkTime"].DefaultCellStyle.Format = @"hh\:mm";
            }
            if (dgvSchedule.Columns.Contains("Shift")) dgvSchedule.Columns["Shift"].HeaderText = "Shift";
            if (dgvSchedule.Columns.Contains("MID")) dgvSchedule.Columns["MID"].HeaderText = "Manager ID";
            if (dgvSchedule.Columns.Contains("ManagerName")) dgvSchedule.Columns["ManagerName"].HeaderText = "Manager";
        }

        // ================= EVENTS =================
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                LoadSchedule();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search failed:\n" + ex.Message);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}