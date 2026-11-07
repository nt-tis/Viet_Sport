﻿namespace ADMIN
{
    partial class AddSystemParameterForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblPrice;
        private System.Windows.Forms.Label lblMinBookingTime;
        private System.Windows.Forms.Label lblMaxBookingTime;
        private System.Windows.Forms.Label lblMaxCourts;
        private System.Windows.Forms.NumericUpDown numPrice;
        private System.Windows.Forms.NumericUpDown numMinBookingTime;
        private System.Windows.Forms.NumericUpDown numMaxBookingTime;
        private System.Windows.Forms.NumericUpDown numMaxCourts;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblPrice = new System.Windows.Forms.Label();
            this.lblMinBookingTime = new System.Windows.Forms.Label();
            this.lblMaxBookingTime = new System.Windows.Forms.Label();
            this.lblMaxCourts = new System.Windows.Forms.Label();
            this.numPrice = new System.Windows.Forms.NumericUpDown();
            this.numMinBookingTime = new System.Windows.Forms.NumericUpDown();
            this.numMaxBookingTime = new System.Windows.Forms.NumericUpDown();
            this.numMaxCourts = new System.Windows.Forms.NumericUpDown();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numPrice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinBookingTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxBookingTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxCourts)).BeginInit();
            this.SuspendLayout();
            // 
            // lblPrice
            // 
            this.lblPrice.AutoSize = true;
            this.lblPrice.Location = new System.Drawing.Point(20, 20);
            this.lblPrice.Name = "lblPrice";
            this.lblPrice.Size = new System.Drawing.Size(100, 13);
            this.lblPrice.TabIndex = 0;
            this.lblPrice.Text = "Đơn giá sân (Price):";
            // 
            // lblMinBookingTime
            // 
            this.lblMinBookingTime.AutoSize = true;
            this.lblMinBookingTime.Location = new System.Drawing.Point(20, 60);
            this.lblMinBookingTime.Name = "lblMinBookingTime";
            this.lblMinBookingTime.Size = new System.Drawing.Size(143, 13);
            this.lblMinBookingTime.TabIndex = 2;
            this.lblMinBookingTime.Text = "Thời gian đặt tối thiểu (phút):";
            // 
            // lblMaxBookingTime
            // 
            this.lblMaxBookingTime.AutoSize = true;
            this.lblMaxBookingTime.Location = new System.Drawing.Point(20, 100);
            this.lblMaxBookingTime.Name = "lblMaxBookingTime";
            this.lblMaxBookingTime.Size = new System.Drawing.Size(133, 13);
            this.lblMaxBookingTime.TabIndex = 4;
            this.lblMaxBookingTime.Text = "Thời gian đặt tối đa (phút):";
            // 
            // lblMaxCourts
            // 
            this.lblMaxCourts.AutoSize = true;
            this.lblMaxCourts.Location = new System.Drawing.Point(20, 140);
            this.lblMaxCourts.Name = "lblMaxCourts";
            this.lblMaxCourts.Size = new System.Drawing.Size(127, 13);
            this.lblMaxCourts.TabIndex = 6;
            this.lblMaxCourts.Text = "Số sân đặt tối đa/khách:";
            // 
            // numPrice
            // 
            this.numPrice.DecimalPlaces = 2;
            this.numPrice.Increment = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numPrice.Location = new System.Drawing.Point(180, 18);
            this.numPrice.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numPrice.Name = "numPrice";
            this.numPrice.Size = new System.Drawing.Size(200, 20);
            this.numPrice.TabIndex = 1;
            this.numPrice.ThousandsSeparator = true;
            this.numPrice.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            // 
            // numMinBookingTime
            // 
            this.numMinBookingTime.Location = new System.Drawing.Point(180, 58);
            this.numMinBookingTime.Maximum = new decimal(new int[] {
            480,
            0,
            0,
            0});
            this.numMinBookingTime.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMinBookingTime.Name = "numMinBookingTime";
            this.numMinBookingTime.Size = new System.Drawing.Size(200, 20);
            this.numMinBookingTime.TabIndex = 3;
            this.numMinBookingTime.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // numMaxBookingTime
            // 
            this.numMaxBookingTime.Location = new System.Drawing.Point(180, 98);
            this.numMaxBookingTime.Maximum = new decimal(new int[] {
            480,
            0,
            0,
            0});
            this.numMaxBookingTime.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMaxBookingTime.Name = "numMaxBookingTime";
            this.numMaxBookingTime.Size = new System.Drawing.Size(200, 20);
            this.numMaxBookingTime.TabIndex = 5;
            this.numMaxBookingTime.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
            // 
            // numMaxCourts
            // 
            this.numMaxCourts.Location = new System.Drawing.Point(180, 138);
            this.numMaxCourts.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numMaxCourts.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMaxCourts.Name = "numMaxCourts";
            this.numMaxCourts.Size = new System.Drawing.Size(200, 20);
            this.numMaxCourts.TabIndex = 7;
            this.numMaxCourts.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.ForestGreen;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(90, 190);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 35);
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "Lưu";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Gray;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(210, 190);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Hủy";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // AddSystemParameterForm
            // 
            this.AcceptButton = this.btnSave;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(400, 250);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Controls.Add(this.lblPrice);
            this.Controls.Add(this.numPrice);
            this.Controls.Add(this.lblMinBookingTime);
            this.Controls.Add(this.numMinBookingTime);
            this.Controls.Add(this.lblMaxBookingTime);
            this.Controls.Add(this.numMaxBookingTime);
            this.Controls.Add(this.lblMaxCourts);
            this.Controls.Add(this.numMaxCourts);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddSystemParameterForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thêm Tham Số Hệ Thống";
            this.Load += new System.EventHandler(this.AddSystemParameterForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numPrice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinBookingTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxBookingTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxCourts)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}