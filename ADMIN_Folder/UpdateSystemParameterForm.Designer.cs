﻿namespace ADMIN
{
    partial class UpdateSystemParameterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblSystemPID;
        private System.Windows.Forms.TextBox txtSystemPID;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Label lblPrice;
        private System.Windows.Forms.Label lblMinBookingTime;
        private System.Windows.Forms.Label lblMaxBookingTime;
        private System.Windows.Forms.Label lblMaxCourts;
        private System.Windows.Forms.NumericUpDown numPrice;
        private System.Windows.Forms.NumericUpDown numMinBookingTime;
        private System.Windows.Forms.NumericUpDown numMaxBookingTime;
        private System.Windows.Forms.NumericUpDown numMaxCourts;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblInstruction;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
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

            this.lblSystemPID = new System.Windows.Forms.Label();
            this.txtSystemPID = new System.Windows.Forms.TextBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.lblPrice = new System.Windows.Forms.Label();
            this.lblMinBookingTime = new System.Windows.Forms.Label();
            this.lblMaxBookingTime = new System.Windows.Forms.Label();
            this.lblMaxCourts = new System.Windows.Forms.Label();
            this.numPrice = new System.Windows.Forms.NumericUpDown();
            this.numMinBookingTime = new System.Windows.Forms.NumericUpDown();
            this.numMaxBookingTime = new System.Windows.Forms.NumericUpDown();
            this.numMaxCourts = new System.Windows.Forms.NumericUpDown();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblInstruction = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)(this.numPrice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinBookingTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxBookingTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxCourts)).BeginInit();
            this.SuspendLayout();

            // 
            // Form properties
            // 
            this.Text = "Cập Nhật Tham Số Hệ Thống";
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ClientSize = new System.Drawing.Size(420, 380);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.UpdateSystemParameterForm_Load);

            // 
            // lblInstruction
            // 
            this.lblInstruction.AutoSize = false;
            this.lblInstruction.Location = new System.Drawing.Point(20, 15);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Size = new System.Drawing.Size(380, 30);
            this.lblInstruction.TabIndex = 0;
            this.lblInstruction.Text = "Nhập mã tham số hệ thống và nhấn 'Tải' để hiển thị dữ liệu hiện tại:";
            this.lblInstruction.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);

            // 
            // lblSystemPID
            // 
            this.lblSystemPID.AutoSize = true;
            this.lblSystemPID.Location = new System.Drawing.Point(20, 55);
            this.lblSystemPID.Name = "lblSystemPID";
            this.lblSystemPID.Size = new System.Drawing.Size(70, 13);
            this.lblSystemPID.TabIndex = 1;
            this.lblSystemPID.Text = "SystemPID:";
            this.lblSystemPID.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // 
            // txtSystemPID
            // 
            this.txtSystemPID.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtSystemPID.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSystemPID.Location = new System.Drawing.Point(120, 52);
            this.txtSystemPID.MaxLength = 50;
            this.txtSystemPID.Name = "txtSystemPID";
            this.txtSystemPID.Size = new System.Drawing.Size(160, 23);
            this.txtSystemPID.TabIndex = 2;
            this.txtSystemPID.TextChanged += new System.EventHandler(this.TxtSystemPID_TextChanged);

            // 
            // btnLoad
            // 
            this.btnLoad.BackColor = System.Drawing.Color.DodgerBlue;
            this.btnLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoad.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnLoad.ForeColor = System.Drawing.Color.White;
            this.btnLoad.Location = new System.Drawing.Point(295, 50);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(100, 27);
            this.btnLoad.TabIndex = 3;
            this.btnLoad.Text = "Tải";
            this.btnLoad.UseVisualStyleBackColor = false;
            this.btnLoad.Click += new System.EventHandler(this.BtnLoad_Click);

            // 
            // lblPrice
            // 
            this.lblPrice.AutoSize = true;
            this.lblPrice.Location = new System.Drawing.Point(20, 100);
            this.lblPrice.Name = "lblPrice";
            this.lblPrice.Size = new System.Drawing.Size(120, 13);
            this.lblPrice.TabIndex = 4;
            this.lblPrice.Text = "Đơn giá sân (Price):";

            // 
            // numPrice
            // 
            this.numPrice.DecimalPlaces = 2;
            this.numPrice.Increment = new decimal(new int[] { 10000, 0, 0, 0 });
            this.numPrice.Location = new System.Drawing.Point(195, 98);
            this.numPrice.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
            this.numPrice.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            this.numPrice.Name = "numPrice";
            this.numPrice.Size = new System.Drawing.Size(200, 20);
            this.numPrice.TabIndex = 5;
            this.numPrice.Value = new decimal(new int[] { 100000, 0, 0, 0 });
            this.numPrice.ThousandsSeparator = true;

            // 
            // lblMinBookingTime
            // 
            this.lblMinBookingTime.AutoSize = true;
            this.lblMinBookingTime.Location = new System.Drawing.Point(20, 140);
            this.lblMinBookingTime.Name = "lblMinBookingTime";
            this.lblMinBookingTime.Size = new System.Drawing.Size(170, 13);
            this.lblMinBookingTime.TabIndex = 6;
            this.lblMinBookingTime.Text = "Thời gian đặt tối thiểu (phút):";

            // 
            // numMinBookingTime
            // 
            this.numMinBookingTime.Location = new System.Drawing.Point(195, 138);
            this.numMinBookingTime.Maximum = new decimal(new int[] { 480, 0, 0, 0 });
            this.numMinBookingTime.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMinBookingTime.Name = "numMinBookingTime";
            this.numMinBookingTime.Size = new System.Drawing.Size(200, 20);
            this.numMinBookingTime.TabIndex = 7;
            this.numMinBookingTime.Value = new decimal(new int[] { 30, 0, 0, 0 });

            // 
            // lblMaxBookingTime
            // 
            this.lblMaxBookingTime.AutoSize = true;
            this.lblMaxBookingTime.Location = new System.Drawing.Point(20, 180);
            this.lblMaxBookingTime.Name = "lblMaxBookingTime";
            this.lblMaxBookingTime.Size = new System.Drawing.Size(170, 13);
            this.lblMaxBookingTime.TabIndex = 8;
            this.lblMaxBookingTime.Text = "Thời gian đặt tối đa (phút):";

            // 
            // numMaxBookingTime
            // 
            this.numMaxBookingTime.Location = new System.Drawing.Point(195, 178);
            this.numMaxBookingTime.Maximum = new decimal(new int[] { 480, 0, 0, 0 });
            this.numMaxBookingTime.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMaxBookingTime.Name = "numMaxBookingTime";
            this.numMaxBookingTime.Size = new System.Drawing.Size(200, 20);
            this.numMaxBookingTime.TabIndex = 9;
            this.numMaxBookingTime.Value = new decimal(new int[] { 120, 0, 0, 0 });

            // 
            // lblMaxCourts
            // 
            this.lblMaxCourts.AutoSize = true;
            this.lblMaxCourts.Location = new System.Drawing.Point(20, 220);
            this.lblMaxCourts.Name = "lblMaxCourts";
            this.lblMaxCourts.Size = new System.Drawing.Size(160, 13);
            this.lblMaxCourts.TabIndex = 10;
            this.lblMaxCourts.Text = "Số sân đặt tối đa/khách:";

            // 
            // numMaxCourts
            // 
            this.numMaxCourts.Location = new System.Drawing.Point(195, 218);
            this.numMaxCourts.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numMaxCourts.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMaxCourts.Name = "numMaxCourts";
            this.numMaxCourts.Size = new System.Drawing.Size(200, 20);
            this.numMaxCourts.TabIndex = 11;
            this.numMaxCourts.Value = new decimal(new int[] { 2, 0, 0, 0 });

            // 
            // btnUpdate
            // 
            this.btnUpdate.BackColor = System.Drawing.Color.ForestGreen;
            this.btnUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnUpdate.ForeColor = System.Drawing.Color.White;
            this.btnUpdate.Location = new System.Drawing.Point(90, 280);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(110, 40);
            this.btnUpdate.TabIndex = 12;
            this.btnUpdate.Text = "Cập Nhật";
            this.btnUpdate.UseVisualStyleBackColor = false;
            this.btnUpdate.Click += new System.EventHandler(this.BtnUpdate_Click);

            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Gray;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(220, 280);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(110, 40);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Hủy";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);

            // 
            // Add controls to form
            // 
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.lblSystemPID);
            this.Controls.Add(this.txtSystemPID);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.lblPrice);
            this.Controls.Add(this.numPrice);
            this.Controls.Add(this.lblMinBookingTime);
            this.Controls.Add(this.numMinBookingTime);
            this.Controls.Add(this.lblMaxBookingTime);
            this.Controls.Add(this.numMaxBookingTime);
            this.Controls.Add(this.lblMaxCourts);
            this.Controls.Add(this.numMaxCourts);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnCancel);

            this.AcceptButton = this.btnUpdate;
            this.CancelButton = this.btnCancel;

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