﻿namespace CASHIER

{

    partial class Cashier_ChangePassword

    {

        private System.ComponentModel.IContainer components = null;



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

            this.panelTop = new System.Windows.Forms.Panel();

            this.lblEmployeeID = new System.Windows.Forms.Label();

            this.label1 = new System.Windows.Forms.Label();

            this.groupBox1 = new System.Windows.Forms.GroupBox();

            this.chkShowPassword = new System.Windows.Forms.CheckBox();

            this.btnCancel = new System.Windows.Forms.Button();

            this.btnChangePassword = new System.Windows.Forms.Button();

            this.txtConfirmPassword = new System.Windows.Forms.TextBox();

            this.label5 = new System.Windows.Forms.Label();

            this.txtNewPassword = new System.Windows.Forms.TextBox();

            this.label4 = new System.Windows.Forms.Label();

            this.txtCurrentPassword = new System.Windows.Forms.TextBox();

            this.label3 = new System.Windows.Forms.Label();

            this.label2 = new System.Windows.Forms.Label();

            this.panelTop.SuspendLayout();

            this.groupBox1.SuspendLayout();

            this.SuspendLayout();

            // 

            // panelTop

            // 

            this.panelTop.BackColor = System.Drawing.Color.White;

            this.panelTop.Controls.Add(this.lblEmployeeID);

            this.panelTop.Controls.Add(this.label1);

            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;

            this.panelTop.Location = new System.Drawing.Point(0, 0);

            this.panelTop.Name = "panelTop";

            this.panelTop.Size = new System.Drawing.Size(1100, 75);

            this.panelTop.TabIndex = 0;

            this.panelTop.Paint += new System.Windows.Forms.PaintEventHandler(this.panelTop_Paint);

            // 

            // lblEmployeeID

            // 

            this.lblEmployeeID.AutoSize = true;

            this.lblEmployeeID.Font = new System.Drawing.Font("Segoe UI", 10F);

            this.lblEmployeeID.Location = new System.Drawing.Point(225, 35);

            this.lblEmployeeID.Name = "lblEmployeeID";

            this.lblEmployeeID.Size = new System.Drawing.Size(69, 19);

            this.lblEmployeeID.TabIndex = 1;

            this.lblEmployeeID.Text = "EMP0016";

            // 

            // label1

            // 

            this.label1.AutoSize = true;

            this.label1.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);

            this.label1.Location = new System.Drawing.Point(30, 30);

            this.label1.Name = "label1";

            this.label1.Size = new System.Drawing.Size(169, 25);

            this.label1.TabIndex = 0;

            this.label1.Text = "Change Password";

            // 

            // groupBox1

            // 

            this.groupBox1.Controls.Add(this.chkShowPassword);

            this.groupBox1.Controls.Add(this.btnCancel);

            this.groupBox1.Controls.Add(this.btnChangePassword);

            this.groupBox1.Controls.Add(this.txtConfirmPassword);

            this.groupBox1.Controls.Add(this.label5);

            this.groupBox1.Controls.Add(this.txtNewPassword);

            this.groupBox1.Controls.Add(this.label4);

            this.groupBox1.Controls.Add(this.txtCurrentPassword);

            this.groupBox1.Controls.Add(this.label3);

            this.groupBox1.Controls.Add(this.label2);

            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);

            this.groupBox1.Location = new System.Drawing.Point(200, 130);

            this.groupBox1.Name = "groupBox1";

            this.groupBox1.Size = new System.Drawing.Size(700, 430);

            this.groupBox1.TabIndex = 1;

            this.groupBox1.TabStop = false;

            this.groupBox1.Text = "Password Information";

            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);

            // 

            // chkShowPassword

            // 

            this.chkShowPassword.AutoSize = true;

            this.chkShowPassword.Font = new System.Drawing.Font("Segoe UI", 10F);

            this.chkShowPassword.Location = new System.Drawing.Point(200, 305);

            this.chkShowPassword.Name = "chkShowPassword";

            this.chkShowPassword.Size = new System.Drawing.Size(123, 23);

            this.chkShowPassword.TabIndex = 9;

            this.chkShowPassword.Text = "Show Password";

            this.chkShowPassword.UseVisualStyleBackColor = true;

            this.chkShowPassword.CheckedChanged += new System.EventHandler(this.chkShowPassword_CheckedChanged);

            // 

            // btnCancel

            // 

            this.btnCancel.BackColor = System.Drawing.Color.Gray;

            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);

            this.btnCancel.ForeColor = System.Drawing.Color.White;

            this.btnCancel.Location = new System.Drawing.Point(330, 360);

            this.btnCancel.Name = "btnCancel";

            this.btnCancel.Size = new System.Drawing.Size(120, 45);

            this.btnCancel.TabIndex = 8;

            this.btnCancel.Text = "Cancel";

            this.btnCancel.UseVisualStyleBackColor = false;

            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // 

            // btnChangePassword

            // 

            this.btnChangePassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));

            this.btnChangePassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.btnChangePassword.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);

            this.btnChangePassword.ForeColor = System.Drawing.Color.White;

            this.btnChangePassword.Location = new System.Drawing.Point(200, 360);

            this.btnChangePassword.Name = "btnChangePassword";

            this.btnChangePassword.Size = new System.Drawing.Size(120, 45);

            this.btnChangePassword.TabIndex = 7;

            this.btnChangePassword.Text = "Change";

            this.btnChangePassword.UseVisualStyleBackColor = false;

            this.btnChangePassword.Click += new System.EventHandler(this.btnChangePassword_Click);

            // 

            // txtConfirmPassword

            // 

            this.txtConfirmPassword.Font = new System.Drawing.Font("Segoe UI", 10F);

            this.txtConfirmPassword.Location = new System.Drawing.Point(200, 250);

            this.txtConfirmPassword.Name = "txtConfirmPassword";

            this.txtConfirmPassword.PasswordChar = '?';

            this.txtConfirmPassword.Size = new System.Drawing.Size(420, 25);

            this.txtConfirmPassword.TabIndex = 6;

            // 

            // label5

            // 

            this.label5.AutoSize = true;

            this.label5.Font = new System.Drawing.Font("Segoe UI", 10F);

            this.label5.Location = new System.Drawing.Point(50, 253);

            this.label5.Name = "label5";

            this.label5.Size = new System.Drawing.Size(123, 19);

            this.label5.TabIndex = 5;

            this.label5.Text = "Confirm Password:";

            // 

            // txtNewPassword

            // 

            this.txtNewPassword.Font = new System.Drawing.Font("Segoe UI", 10F);

            this.txtNewPassword.Location = new System.Drawing.Point(200, 190);

            this.txtNewPassword.Name = "txtNewPassword";

            this.txtNewPassword.PasswordChar = '?';

            this.txtNewPassword.Size = new System.Drawing.Size(420, 25);

            this.txtNewPassword.TabIndex = 4;

            // 

            // label4

            // 

            this.label4.AutoSize = true;

            this.label4.Font = new System.Drawing.Font("Segoe UI", 10F);

            this.label4.Location = new System.Drawing.Point(50, 193);

            this.label4.Name = "label4";

            this.label4.Size = new System.Drawing.Size(101, 19);

            this.label4.TabIndex = 3;

            this.label4.Text = "New Password:";

            // 

            // txtCurrentPassword

            // 

            this.txtCurrentPassword.Font = new System.Drawing.Font("Segoe UI", 10F);

            this.txtCurrentPassword.Location = new System.Drawing.Point(200, 130);

            this.txtCurrentPassword.Name = "txtCurrentPassword";

            this.txtCurrentPassword.PasswordChar = '?';

            this.txtCurrentPassword.Size = new System.Drawing.Size(420, 25);

            this.txtCurrentPassword.TabIndex = 2;

            // 

            // label3

            // 

            this.label3.AutoSize = true;

            this.label3.Font = new System.Drawing.Font("Segoe UI", 10F);

            this.label3.Location = new System.Drawing.Point(50, 133);

            this.label3.Name = "label3";

            this.label3.Size = new System.Drawing.Size(121, 19);

            this.label3.TabIndex = 1;

            this.label3.Text = "Current Password:";

            // 

            // label2

            // 

            this.label2.AutoSize = true;

            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F);

            this.label2.ForeColor = System.Drawing.Color.Gray;

            this.label2.Location = new System.Drawing.Point(50, 45);

            this.label2.Name = "label2";

            this.label2.Size = new System.Drawing.Size(195, 60);

            this.label2.TabIndex = 0;

            this.label2.Text = "Password Requirements:\r\n- Minimum 8 characters\r\n- At least one uppercase letter (" +

    "A-Z)\r\n- At least one number (0-9)";

            // 

            // Cashier_ChangePassword

            // 

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.BackColor = System.Drawing.Color.White;

            this.ClientSize = new System.Drawing.Size(1100, 670);

            this.Controls.Add(this.groupBox1);

            this.Controls.Add(this.panelTop);

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            this.Name = "Cashier_ChangePassword";

            this.Text = "Change Password";

            this.Load += new System.EventHandler(this.Cashier_ChangePassword_Load);

            this.panelTop.ResumeLayout(false);

            this.panelTop.PerformLayout();

            this.groupBox1.ResumeLayout(false);

            this.groupBox1.PerformLayout();

            this.ResumeLayout(false);



        }



        #endregion



        private System.Windows.Forms.Panel panelTop;

        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.Label lblEmployeeID;

        private System.Windows.Forms.GroupBox groupBox1;

        private System.Windows.Forms.Label label2;

        private System.Windows.Forms.TextBox txtCurrentPassword;

        private System.Windows.Forms.Label label3;

        private System.Windows.Forms.TextBox txtNewPassword;

        private System.Windows.Forms.Label label4;

        private System.Windows.Forms.TextBox txtConfirmPassword;

        private System.Windows.Forms.Label label5;

        private System.Windows.Forms.Button btnChangePassword;

        private System.Windows.Forms.Button btnCancel;

        private System.Windows.Forms.CheckBox chkShowPassword;

    }

}

