﻿namespace ADMIN
{
    partial class AddEmployeeForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblFullName;
        private System.Windows.Forms.TextBox txtFullName;
        private System.Windows.Forms.Label lblBirthday;
        private System.Windows.Forms.DateTimePicker dtpBirthday;
        private System.Windows.Forms.Label lblGender;
        private System.Windows.Forms.ComboBox cmbGender;
        private System.Windows.Forms.Label lblCCCD;
        private System.Windows.Forms.TextBox txtCCCD;
        private System.Windows.Forms.Label lblStreet;
        private System.Windows.Forms.TextBox txtStreet;
        private System.Windows.Forms.Label lblDistrict;
        private System.Windows.Forms.TextBox txtDistrict;
        private System.Windows.Forms.Label lblCity;
        private System.Windows.Forms.TextBox txtCity;
        private System.Windows.Forms.Label lblPhone;
        private System.Windows.Forms.TextBox txtPhone;
        private System.Windows.Forms.Label lblBasicSalary;
        private System.Windows.Forms.NumericUpDown numBasicSalary;
        private System.Windows.Forms.Label lblManagerID;
        private System.Windows.Forms.TextBox txtManagerID;
        private System.Windows.Forms.Label lblCenterID;
        private System.Windows.Forms.TextBox txtCenterID;
        private System.Windows.Forms.Label lblRole;
        private System.Windows.Forms.ComboBox cmbRole;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnCancel;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.lblFullName = new System.Windows.Forms.Label();
            this.txtFullName = new System.Windows.Forms.TextBox();
            this.lblBirthday = new System.Windows.Forms.Label();
            this.dtpBirthday = new System.Windows.Forms.DateTimePicker();
            this.lblGender = new System.Windows.Forms.Label();
            this.cmbGender = new System.Windows.Forms.ComboBox();
            this.lblCCCD = new System.Windows.Forms.Label();
            this.txtCCCD = new System.Windows.Forms.TextBox();
            this.lblStreet = new System.Windows.Forms.Label();
            this.txtStreet = new System.Windows.Forms.TextBox();
            this.lblDistrict = new System.Windows.Forms.Label();
            this.txtDistrict = new System.Windows.Forms.TextBox();
            this.lblCity = new System.Windows.Forms.Label();
            this.txtCity = new System.Windows.Forms.TextBox();
            this.lblPhone = new System.Windows.Forms.Label();
            this.txtPhone = new System.Windows.Forms.TextBox();
            this.lblBasicSalary = new System.Windows.Forms.Label();
            this.numBasicSalary = new System.Windows.Forms.NumericUpDown();
            this.lblManagerID = new System.Windows.Forms.Label();
            this.txtManagerID = new System.Windows.Forms.TextBox();
            this.lblCenterID = new System.Windows.Forms.Label();
            this.txtCenterID = new System.Windows.Forms.TextBox();
            this.lblRole = new System.Windows.Forms.Label();
            this.cmbRole = new System.Windows.Forms.ComboBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.numBasicSalary)).BeginInit();
            this.SuspendLayout();

            // Form
            this.Text = "Thêm mới nhân viên";
            this.ClientSize = new System.Drawing.Size(520, 520);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int xLabel = 20, xControl = 140, y = 20, dy = 32;

            // FullName
            this.lblFullName.AutoSize = true;
            this.lblFullName.Location = new System.Drawing.Point(xLabel, y);
            this.lblFullName.Text = "Full Name:";
            this.txtFullName.Location = new System.Drawing.Point(xControl, y);
            this.txtFullName.Size = new System.Drawing.Size(340, 22);
            y += dy;

            // Birthday
            this.lblBirthday.AutoSize = true;
            this.lblBirthday.Location = new System.Drawing.Point(xLabel, y);
            this.lblBirthday.Text = "Birthday:";
            this.dtpBirthday.Location = new System.Drawing.Point(xControl, y);
            this.dtpBirthday.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            y += dy;

            // Gender
            this.lblGender.AutoSize = true;
            this.lblGender.Location = new System.Drawing.Point(xLabel, y);
            this.lblGender.Text = "Gender:";
            this.cmbGender.Location = new System.Drawing.Point(xControl, y);
            this.cmbGender.Size = new System.Drawing.Size(140, 24);
            this.cmbGender.Items.AddRange(new object[] { "Male", "Female", "Other" });
            y += dy;

            // CCCD
            this.lblCCCD.AutoSize = true;
            this.lblCCCD.Location = new System.Drawing.Point(xLabel, y);
            this.lblCCCD.Text = "CCCD:";
            this.txtCCCD.Location = new System.Drawing.Point(xControl, y);
            this.txtCCCD.Size = new System.Drawing.Size(200, 22);
            y += dy;

            // Street
            this.lblStreet.AutoSize = true;
            this.lblStreet.Location = new System.Drawing.Point(xLabel, y);
            this.lblStreet.Text = "Street:";
            this.txtStreet.Location = new System.Drawing.Point(xControl, y);
            this.txtStreet.Size = new System.Drawing.Size(340, 22);
            y += dy;

            // District
            this.lblDistrict.AutoSize = true;
            this.lblDistrict.Location = new System.Drawing.Point(xLabel, y);
            this.lblDistrict.Text = "District:";
            this.txtDistrict.Location = new System.Drawing.Point(xControl, y);
            this.txtDistrict.Size = new System.Drawing.Size(260, 22);
            y += dy;

            // City
            this.lblCity.AutoSize = true;
            this.lblCity.Location = new System.Drawing.Point(xLabel, y);
            this.lblCity.Text = "City:";
            this.txtCity.Location = new System.Drawing.Point(xControl, y);
            this.txtCity.Size = new System.Drawing.Size(260, 22);
            y += dy;

            // Phone
            this.lblPhone.AutoSize = true;
            this.lblPhone.Location = new System.Drawing.Point(xLabel, y);
            this.lblPhone.Text = "Phone:";
            this.txtPhone.Location = new System.Drawing.Point(xControl, y);
            this.txtPhone.Size = new System.Drawing.Size(200, 22);
            y += dy;

            // BasicSalary
            this.lblBasicSalary.AutoSize = true;
            this.lblBasicSalary.Location = new System.Drawing.Point(xLabel, y);
            this.lblBasicSalary.Text = "Basic Salary:";
            this.numBasicSalary.Location = new System.Drawing.Point(xControl, y);
            this.numBasicSalary.DecimalPlaces = 2;
            this.numBasicSalary.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
            this.numBasicSalary.Size = new System.Drawing.Size(140, 22);
            this.numBasicSalary.Value = 100000;
            y += dy;

            // ManagerID
            this.lblManagerID.AutoSize = true;
            this.lblManagerID.Location = new System.Drawing.Point(xLabel, y);
            this.lblManagerID.Text = "ManagerID (opt):";
            this.txtManagerID.Location = new System.Drawing.Point(xControl, y);
            this.txtManagerID.Size = new System.Drawing.Size(200, 22);
            y += dy;

            // CenterID
            this.lblCenterID.AutoSize = true;
            this.lblCenterID.Location = new System.Drawing.Point(xLabel, y);
            this.lblCenterID.Text = "CenterID:";
            this.txtCenterID.Location = new System.Drawing.Point(xControl, y);
            this.txtCenterID.Size = new System.Drawing.Size(200, 22);
            y += dy;

            // Role
            this.lblRole.AutoSize = true;
            this.lblRole.Location = new System.Drawing.Point(xLabel, y);
            this.lblRole.Text = "Role:";
            this.cmbRole.Location = new System.Drawing.Point(xControl, y);
            this.cmbRole.Size = new System.Drawing.Size(140, 24);
            this.cmbRole.Items.AddRange(new object[] { "Admin", "Manager", "Receptionist", "Technician", "Cashier" });
            y += dy + 6;

            // Buttons
            this.btnAdd.Location = new System.Drawing.Point(140, y);
            this.btnAdd.Size = new System.Drawing.Size(100, 32);
            this.btnAdd.Text = "Add";
            this.btnAdd.BackColor = System.Drawing.Color.ForestGreen;
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);

            this.btnCancel.Location = new System.Drawing.Point(260, y);
            this.btnCancel.Size = new System.Drawing.Size(100, 32);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.BackColor = System.Drawing.Color.Gray;
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);

            // Add controls
            this.Controls.Add(this.lblFullName);
            this.Controls.Add(this.txtFullName);
            this.Controls.Add(this.lblBirthday);
            this.Controls.Add(this.dtpBirthday);
            this.Controls.Add(this.lblGender);
            this.Controls.Add(this.cmbGender);
            this.Controls.Add(this.lblCCCD);
            this.Controls.Add(this.txtCCCD);
            this.Controls.Add(this.lblStreet);
            this.Controls.Add(this.txtStreet);
            this.Controls.Add(this.lblDistrict);
            this.Controls.Add(this.txtDistrict);
            this.Controls.Add(this.lblCity);
            this.Controls.Add(this.txtCity);
            this.Controls.Add(this.lblPhone);
            this.Controls.Add(this.txtPhone);
            this.Controls.Add(this.lblBasicSalary);
            this.Controls.Add(this.numBasicSalary);
            this.Controls.Add(this.lblManagerID);
            this.Controls.Add(this.txtManagerID);
            this.Controls.Add(this.lblCenterID);
            this.Controls.Add(this.txtCenterID);
            this.Controls.Add(this.lblRole);
            this.Controls.Add(this.cmbRole);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnCancel);

            ((System.ComponentModel.ISupportInitialize)(this.numBasicSalary)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}