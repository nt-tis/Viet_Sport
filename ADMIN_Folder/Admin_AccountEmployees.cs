using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ADMIN
{
	public partial class Admin_AccountEmployees : Form
	{
		public Admin_AccountEmployees()
		{
			InitializeComponent();

			// Make scaling and font explicit to match main form
			this.AutoScaleMode = AutoScaleMode.None;
			this.Font = new Font("Segoe UI", 9F);
			this.ClientSize = new Size(1100, 700); // same client size as main form (or desired size)
			this.StartPosition = FormStartPosition.CenterParent;

			// design-time guard -> must be immediately after InitializeComponent()
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				return;

			// runtime-only wiring
			this.Load += Admin_AccountEmployees_Load;
			this.button5.Click += button5_Click;
			this.button18.Click += Button18_Click;
			this.button19.Click += Button19_Click;
			this.button21.Click += Button21_Click;
			this.button20.Click -= Button20_Click;
			this.button20.Click += Button20_Click;

			// Apply modern styling at runtime (safe for .NET Framework 4.7.2)
			ApplyModernStyling();
		}

		private void ApplyModernStyling()
		{
			// Form
			this.Font = new Font("Segoe UI", 9F);
			this.BackColor = Color.FromArgb(245, 247, 250);

			// Left sidebar (panel2)
			if (panel2 != null)
			{
				panel2.BackColor = Color.FromArgb(24, 116, 64); // deeper green
				panel2.Width = 240;
				panel2.Padding = new Padding(12, 12, 12, 12);
			}

			// Main content
			if (panelMain != null)
			{
				panelMain.BackColor = Color.FromArgb(240, 243, 246);
				panelMain.Padding = new Padding(18);

				// Create (or reuse) a right-side actions FlowLayoutPanel to hold action buttons with spacing
				FlowLayoutPanel panelActions = panelMain.Controls.ContainsKey("panelActionsRuntime")
					? panelMain.Controls["panelActionsRuntime"] as FlowLayoutPanel
					: null;

				if (panelActions == null)
				{
					panelActions = new FlowLayoutPanel
					{
						Name = "panelActionsRuntime",
						BackColor = Color.Transparent,
						Dock = DockStyle.Right,
						Width = 140,                       // smaller width so buttons sit closer to grid
						FlowDirection = FlowDirection.TopDown,
						WrapContents = false,
						AutoScroll = true,
						Padding = new Padding(8),
						Margin = new Padding(0)
					};
					panelMain.Controls.Add(panelActions);
					panelActions.BringToFront();
				}

				// Ensure the grid fills remaining area
				if (dataGridView1 != null)
				{
					dataGridView1.Dock = DockStyle.Fill;
					dataGridView1.Margin = new Padding(0);
				}

				// Action buttons to place in the right panel (top -> bottom order)
				var actionButtons = new Button[] { button18, button19, button21, button20 }; // Create, Add, Update, Delete

				// Add buttons in order to FlowLayoutPanel; FlowLayoutPanel will respect Margin for spacing
				foreach (var b in actionButtons)
				{
					if (b == null) continue;

					// Move button into action panel
					if (b.Parent != panelActions)
					{
						b.Parent = panelActions;
					}

					b.AutoSize = false;
					b.Height = 44;
					// use panelActions.Width rather than ClientSize to avoid zero during initial layout
					b.Width = Math.Max(100, panelActions.Width - panelActions.Padding.Horizontal - 4);
					b.Margin = new Padding(0, 12, 0, 0); // vertical spacing between buttons (12px)
					b.TextAlign = ContentAlignment.MiddleCenter;
					b.FlatStyle = FlatStyle.Flat;
					b.FlatAppearance.BorderSize = 0;
					b.UseVisualStyleBackColor = false; // ensure BackColor is used

					// Keep background fixed on hover/press
					var fixedBg = b.BackColor;
					b.FlatAppearance.MouseOverBackColor = fixedBg;
					b.FlatAppearance.MouseDownBackColor = fixedBg;
				}
			}

			// Avatar + labels
			if (EmployeeName != null)
			{
				EmployeeName.ForeColor = Color.White;
				EmployeeName.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			}
			if (label1 != null)
			{
				label1.ForeColor = Color.WhiteSmoke;
				label1.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
			}

			// Generic sidebar/menu buttons styling (kept left, fixed color)
			var menuButtons = new Button[] {
				button4, button5, button6, button7, button8, button9, button10,
				button11, button12, button13, button14, button15, button16,
				button17
			};
			var inactiveText = Color.WhiteSmoke;
			Color defaultBtnBg = panel2 != null ? panel2.BackColor : SystemColors.Control;

			foreach (var b in menuButtons)
			{
				if (b == null) continue;
				b.FlatStyle = FlatStyle.Flat;
				b.FlatAppearance.BorderSize = 0;
				b.UseVisualStyleBackColor = false;
				b.BackColor = defaultBtnBg;
				b.ForeColor = inactiveText;
				b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
				b.TextAlign = ContentAlignment.MiddleLeft;
				b.Padding = new Padding(8, 0, 0, 0);
				b.Cursor = Cursors.Hand;
				b.FlatAppearance.MouseOverBackColor = b.BackColor;
				b.FlatAppearance.MouseDownBackColor = b.BackColor;
			}

			// Highlight the active menu button with a distinct solid color (fixed)
			if (button5 != null)
			{
				button5.UseVisualStyleBackColor = false;
				button5.BackColor = Color.FromArgb(34, 139, 79);
				button5.ForeColor = Color.White;
				button5.FlatAppearance.MouseOverBackColor = button5.BackColor;
				button5.FlatAppearance.MouseDownBackColor = button5.BackColor;
				button5.Cursor = Cursors.Hand;
			}

			// --- Set distinct, fixed colors for Create (button18), Add (button19), Update (button21), Delete (button20) ---
			if (button18 != null) // Create -> blue
			{
				var createColor = Color.FromArgb(0, 123, 255);
				button18.UseVisualStyleBackColor = false;
				button18.BackColor = createColor;
				button18.ForeColor = Color.White;
				button18.FlatAppearance.MouseOverBackColor = createColor;
				button18.FlatAppearance.MouseDownBackColor = createColor;
				button18.Text = "Create";
			}

			if (button19 != null) // Add -> teal
			{
				var addColor = Color.FromArgb(40, 167, 69);
				button19.UseVisualStyleBackColor = false;
				button19.BackColor = addColor;
				button19.ForeColor = Color.White;
				button19.FlatAppearance.MouseOverBackColor = addColor;
				button19.FlatAppearance.MouseDownBackColor = addColor;
				button19.Text = "Add";
			}

			if (button20 != null) // Delete -> red
			{
				var deleteColor = Color.FromArgb(220, 53, 69);
				button20.UseVisualStyleBackColor = false;
				button20.BackColor = deleteColor;
				button20.ForeColor = Color.White;
				button20.FlatAppearance.MouseOverBackColor = deleteColor;
				button20.FlatAppearance.MouseDownBackColor = deleteColor;
				button20.Text = "Delete";
			}

			if (button21 != null) // Update -> amber/orange
			{
				var updateColor = Color.FromArgb(255, 193, 7);
				button21.UseVisualStyleBackColor = false;
				button21.BackColor = updateColor;
				button21.ForeColor = Color.FromArgb(33, 37, 41); // dark text for contrast
				button21.FlatAppearance.MouseOverBackColor = updateColor;
				button21.FlatAppearance.MouseDownBackColor = updateColor;
				button21.Text = "Update";
			}

			// DataGridView modern styling (unchanged)
			if (dataGridView1 != null)
			{
				dataGridView1.BorderStyle = BorderStyle.None;
				dataGridView1.BackgroundColor = Color.White;
				dataGridView1.GridColor = Color.FromArgb(230, 230, 230);

				// Ensure header styling is enforced regardless of OS theme
				dataGridView1.EnableHeadersVisualStyles = false;

				var headerStyle = new DataGridViewCellStyle
				{
					BackColor = Color.FromArgb(34, 139, 79),
					ForeColor = Color.White,
					Font = new Font("Segoe UI", 9F, FontStyle.Bold),
					Alignment = DataGridViewContentAlignment.MiddleLeft,
					SelectionBackColor = Color.FromArgb(34, 139, 79),
					SelectionForeColor = Color.White
				};

				dataGridView1.ColumnHeadersDefaultCellStyle = headerStyle;
				dataGridView1.RowHeadersDefaultCellStyle = headerStyle;
				dataGridView1.TopLeftHeaderCell.Style = headerStyle;
				dataGridView1.ColumnHeadersHeight = 36;
				dataGridView1.EnableHeadersVisualStyles = false;

				dataGridView1.RowHeadersVisible = false;
				dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
				dataGridView1.RowTemplate.Height = 32;

				dataGridView1.DefaultCellStyle = new DataGridViewCellStyle
				{
					BackColor = Color.White,
					ForeColor = Color.FromArgb(40, 40, 40),
					SelectionBackColor = Color.FromArgb(204, 230, 201),
					SelectionForeColor = Color.Black,
					Padding = new Padding(6, 3, 6, 3)
				};
				dataGridView1.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
				{
					BackColor = Color.FromArgb(250, 250, 250)
				};
				dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dataGridView1.MultiSelect = false;
				dataGridView1.AllowUserToAddRows = false;
				dataGridView1.AllowUserToResizeRows = false;
			}
		}

		// Button20_Click: safely prefill EmployeeID and open DeleteAccountForm
		private void Button20_Click(object sender, EventArgs e)
		{
			// If a row is selected, prefill EmployeeID; otherwise allow manual entry
			string prefillEmployeeId = null;
			if (dataGridView1.CurrentRow != null &&
				dataGridView1.CurrentRow.Cells.Count > 0 &&
				dataGridView1.CurrentRow.Cells[0].Value != null)
			{
				prefillEmployeeId = dataGridView1.CurrentRow.Cells[0].Value.ToString();
			}

			using (var dlg = new DeleteAccountForm())
			{
				// Use the public property on the dialog (DeleteAccountForm.EmployeeID)
				if (!string.IsNullOrEmpty(prefillEmployeeId))
					dlg.EmployeeID = prefillEmployeeId;

				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					LoadAccountEmployees();
				}
			}
		}

		private void Admin_AccountEmployees_Load(object sender, EventArgs e)
		{
			// If this form is hosted inside another form (TopLevel == false), 
			// hide the left menu and expand the main panel to fill available space.
			try
			{
				if (!this.TopLevel)
				{
					if (this.Controls.ContainsKey("panel2"))
					{
						this.panel2.Visible = false;
					}

					if (this.Controls.ContainsKey("panelMain"))
					{
						this.panelMain.Dock = DockStyle.Fill;
					}
				}

				LoadAccountEmployees();
			}
			catch (Exception ex)
			{
				if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
					MessageBox.Show("Load error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Load account employees using stored procedure sp_Admin_AccountEmployees_ViewList
		/// and bind to dataGridView1.
		/// </summary>
		private void LoadAccountEmployees()
		{
			try
			{
				// Clear previous content
				dataGridView1.DataSource = null;
				dataGridView1.Rows.Clear();
				dataGridView1.Columns.Clear();

				// Execute SP and bind results
				DataTable dt = DatabaseConnection.ExecuteQuery("EXEC sp_Admin_AccountEmployees_ViewList");

				if (dt == null || dt.Rows.Count == 0)
				{
					MessageBox.Show("No account employees found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}

				// Bind data
				dataGridView1.AutoGenerateColumns = true;
				dataGridView1.DataSource = dt;

				// Adjust sizing
				dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
				dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

				int totalWidth = dataGridView1.RowHeadersVisible ? dataGridView1.RowHeadersWidth : 0;
				foreach (DataGridViewColumn col in dataGridView1.Columns)
				{
					totalWidth += col.Width;
				}
				const int margin = 8;
				int scrollbarWidth = SystemInformation.VerticalScrollBarWidth;
				totalWidth += margin + scrollbarWidth;

				if (this.panelMain != null)
				{
					panelMain.AutoScroll = true;
					if (totalWidth <= panelMain.ClientSize.Width)
					{
						dataGridView1.Dock = DockStyle.None;
						dataGridView1.Width = totalWidth;
						dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
					}
					else
					{
						dataGridView1.Dock = DockStyle.Fill;
						dataGridView1.Width = panelMain.ClientSize.Width - 4;
					}
				}
				else
				{
					dataGridView1.Dock = DockStyle.None;
					dataGridView1.Width = totalWidth;
					dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to load account employees: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		// Click on left menu "Account Employees" - refresh / show list
		private void button5_Click(object sender, EventArgs e)
		{
			LoadAccountEmployees();
		}

		// Create button click (opens dialog to assign account to an existing employee)
		private void Button18_Click(object sender, EventArgs e)
		{
			using (var dlg = new CreateAccountForm())
			{
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					LoadAccountEmployees();
				}
			}
		}

		// Open AddEmployeeForm
		private void Button19_Click(object sender, EventArgs e)
		{
			using (var dlg = new AddEmployeeForm())
			{
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					LoadAccountEmployees();
				}
			}
		}

		// Open UpdateAccountForm for selected employee
		private void Button21_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow == null)
			{
				MessageBox.Show("Vui lòng chọn nhân viên trong danh sách để cập nhật tài khoản.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			var cell = dataGridView1.CurrentRow.Cells[0].Value;
			if (cell == null)
			{
				MessageBox.Show("Không xác định được EmployeeID của hàng chọn.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			string employeeId = cell.ToString();

			using (var dlg = new UpdateAccountForm(employeeId))
			{
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					LoadAccountEmployees();
				}
			}
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

		private void button4_Click(object sender, EventArgs e)
		{

		}

		private void button9_Click(object sender, EventArgs e)
		{

		}

		private void button15_Click(object sender, EventArgs e)
		{

		}

		private void button6_Click(object sender, EventArgs e)
		{

		}

		private void button14_Click(object sender, EventArgs e)
		{

		}

		private void button11_Click(object sender, EventArgs e)
		{

		}

		private void button8_Click(object sender, EventArgs e)
		{

		}

		private void button10_Click(object sender, EventArgs e)
		{

		}
	}
}