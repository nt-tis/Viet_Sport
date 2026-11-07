﻿namespace ADMIN
{
    partial class DeleteSystemParameterForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblSystemPID;
        private System.Windows.Forms.TextBox txtSystemPID;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblInstruction;

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
            this.components = new System.ComponentModel.Container();

            this.lblSystemPID = new System.Windows.Forms.Label();
            this.txtSystemPID = new System.Windows.Forms.TextBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblInstruction = new System.Windows.Forms.Label();

            this.SuspendLayout();

            // 
            // Form properties
            // 
            this.Text = "Xóa Tham Số Hệ Thống";
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ClientSize = new System.Drawing.Size(400, 180);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // 
            // lblInstruction
            // 
            this.lblInstruction.AutoSize = false;
            this.lblInstruction.Location = new System.Drawing.Point(20, 15);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Size = new System.Drawing.Size(360, 30);
            this.lblInstruction.TabIndex = 0;
            this.lblInstruction.Text = "Nhập mã tham số hệ thống (SystemPID) cần xóa:";
            this.lblInstruction.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);

            // 
            // lblSystemPID
            // 
            this.lblSystemPID.AutoSize = true;
            this.lblSystemPID.Location = new System.Drawing.Point(20, 60);
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
            this.txtSystemPID.Location = new System.Drawing.Point(120, 57);
            this.txtSystemPID.MaxLength = 50;
            this.txtSystemPID.Name = "txtSystemPID";
            this.txtSystemPID.Size = new System.Drawing.Size(250, 23);
            this.txtSystemPID.TabIndex = 2;
            this.txtSystemPID.TextChanged += new System.EventHandler(this.TxtSystemPID_TextChanged);

            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.Crimson;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(90, 110);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(100, 35);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "Xóa";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);

            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Gray;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(210, 110);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Hủy";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);

            // 
            // Add controls to form
            // 
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.lblSystemPID);
            this.Controls.Add(this.txtSystemPID);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnCancel);

            this.AcceptButton = this.btnDelete;
            this.CancelButton = this.btnCancel;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}