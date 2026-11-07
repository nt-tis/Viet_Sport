﻿namespace TECHNICIAN
{
    partial class TechnicianMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TechnicianMainForm));
            this.panelMain = new System.Windows.Forms.Panel();
            this.panelMenu = new System.Windows.Forms.Panel();
            this.Username = new System.Windows.Forms.Label();
            this.Role = new System.Windows.Forms.Label();
            this.EmployeeName = new System.Windows.Forms.Label();
            this.EmployeeID = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.btnReports = new System.Windows.Forms.Button();
            this.btnWorkSchedule = new System.Windows.Forms.Button();
            this.btnMaintenanceList = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panelMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(218, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1116, 641);
            this.panelMain.TabIndex = 3;
            this.panelMain.Paint += new System.Windows.Forms.PaintEventHandler(this.panelMain_Paint);
            // 
            // panelMenu
            // 
            this.panelMenu.BackColor = System.Drawing.Color.Green;
            this.panelMenu.Controls.Add(this.Username);
            this.panelMenu.Controls.Add(this.Role);
            this.panelMenu.Controls.Add(this.EmployeeName);
            this.panelMenu.Controls.Add(this.EmployeeID);
            this.panelMenu.Controls.Add(this.button2);
            this.panelMenu.Controls.Add(this.button3);
            this.panelMenu.Controls.Add(this.button4);
            this.panelMenu.Controls.Add(this.button5);
            this.panelMenu.Controls.Add(this.btnReports);
            this.panelMenu.Controls.Add(this.btnWorkSchedule);
            this.panelMenu.Controls.Add(this.btnMaintenanceList);
            this.panelMenu.Controls.Add(this.panel3);
            this.panelMenu.Cursor = System.Windows.Forms.Cursors.Default;
            this.panelMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelMenu.Location = new System.Drawing.Point(0, 0);
            this.panelMenu.Name = "panelMenu";
            this.panelMenu.Size = new System.Drawing.Size(218, 641);
            this.panelMenu.TabIndex = 2;
            // 
            // Username
            // 
            this.Username.AutoSize = true;
            this.Username.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.Username.ForeColor = System.Drawing.Color.White;
            this.Username.Location = new System.Drawing.Point(62, 86);
            this.Username.Name = "Username";
            this.Username.Size = new System.Drawing.Size(54, 17);
            this.Username.TabIndex = 15;
            this.Username.Text = "TN0026";
            // 
            // Role
            // 
            this.Role.AutoSize = true;
            this.Role.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.Role.ForeColor = System.Drawing.Color.White;
            this.Role.Location = new System.Drawing.Point(62, 59);
            this.Role.Name = "Role";
            this.Role.Size = new System.Drawing.Size(73, 17);
            this.Role.TabIndex = 14;
            this.Role.Text = "Technician";
            // 
            // EmployeeName
            // 
            this.EmployeeName.AutoSize = true;
            this.EmployeeName.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.EmployeeName.ForeColor = System.Drawing.Color.White;
            this.EmployeeName.Location = new System.Drawing.Point(62, 33);
            this.EmployeeName.Name = "EmployeeName";
            this.EmployeeName.Size = new System.Drawing.Size(120, 17);
            this.EmployeeName.TabIndex = 13;
            this.EmployeeName.Text = "Nguyễn Hồng Nhi";
            // 
            // EmployeeID
            // 
            this.EmployeeID.AutoSize = true;
            this.EmployeeID.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.EmployeeID.ForeColor = System.Drawing.Color.White;
            this.EmployeeID.Location = new System.Drawing.Point(62, 9);
            this.EmployeeID.Name = "EmployeeID";
            this.EmployeeID.Size = new System.Drawing.Size(63, 17);
            this.EmployeeID.TabIndex = 0;
            this.EmployeeID.Text = "EMP0128";
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.ForestGreen;
            this.button2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.button2.ForeColor = System.Drawing.Color.White;
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button2.Location = new System.Drawing.Point(0, 485);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(218, 63);
            this.button2.TabIndex = 1;
            this.button2.Text = "Log out";
            this.button2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.ForestGreen;
            this.button3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.button3.ForeColor = System.Drawing.Color.White;
            this.button3.Image = ((System.Drawing.Image)(resources.GetObject("button3.Image")));
            this.button3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button3.Location = new System.Drawing.Point(0, 423);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(218, 63);
            this.button3.TabIndex = 2;
            this.button3.Text = "Change Password";
            this.button3.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.ForestGreen;
            this.button4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.button4.ForeColor = System.Drawing.Color.White;
            this.button4.Image = ((System.Drawing.Image)(resources.GetObject("button4.Image")));
            this.button4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button4.Location = new System.Drawing.Point(0, 362);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(218, 63);
            this.button4.TabIndex = 3;
            this.button4.Text = "Payroll";
            this.button4.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.Color.ForestGreen;
            this.button5.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.button5.ForeColor = System.Drawing.Color.White;
            this.button5.Image = ((System.Drawing.Image)(resources.GetObject("button5.Image")));
            this.button5.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button5.Location = new System.Drawing.Point(0, 301);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(218, 63);
            this.button5.TabIndex = 4;
            this.button5.Text = "Leave Application";
            this.button5.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button5.UseVisualStyleBackColor = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // btnReports
            // 
            this.btnReports.BackColor = System.Drawing.Color.ForestGreen;
            this.btnReports.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnReports.ForeColor = System.Drawing.Color.White;
            this.btnReports.Image = ((System.Drawing.Image)(resources.GetObject("btnReports.Image")));
            this.btnReports.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReports.Location = new System.Drawing.Point(0, 239);
            this.btnReports.Name = "btnReports";
            this.btnReports.Size = new System.Drawing.Size(218, 63);
            this.btnReports.TabIndex = 5;
            this.btnReports.Text = "Reports";
            this.btnReports.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnReports.UseVisualStyleBackColor = false;
            this.btnReports.Click += new System.EventHandler(this.btnReports_Click_1);
            // 
            // btnWorkSchedule
            // 
            this.btnWorkSchedule.BackColor = System.Drawing.Color.ForestGreen;
            this.btnWorkSchedule.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnWorkSchedule.ForeColor = System.Drawing.Color.White;
            this.btnWorkSchedule.Image = ((System.Drawing.Image)(resources.GetObject("btnWorkSchedule.Image")));
            this.btnWorkSchedule.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnWorkSchedule.Location = new System.Drawing.Point(0, 177);
            this.btnWorkSchedule.Name = "btnWorkSchedule";
            this.btnWorkSchedule.Size = new System.Drawing.Size(218, 63);
            this.btnWorkSchedule.TabIndex = 7;
            this.btnWorkSchedule.Text = "Work Schedule";
            this.btnWorkSchedule.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnWorkSchedule.UseVisualStyleBackColor = false;
            this.btnWorkSchedule.Click += new System.EventHandler(this.button8_Click);
            // 
            // btnMaintenanceList
            // 
            this.btnMaintenanceList.BackColor = System.Drawing.Color.ForestGreen;
            this.btnMaintenanceList.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnMaintenanceList.ForeColor = System.Drawing.Color.White;
            this.btnMaintenanceList.Image = ((System.Drawing.Image)(resources.GetObject("btnMaintenanceList.Image")));
            this.btnMaintenanceList.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMaintenanceList.Location = new System.Drawing.Point(0, 116);
            this.btnMaintenanceList.Name = "btnMaintenanceList";
            this.btnMaintenanceList.Size = new System.Drawing.Size(218, 63);
            this.btnMaintenanceList.TabIndex = 8;
            this.btnMaintenanceList.Text = "Maintenance List";
            this.btnMaintenanceList.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnMaintenanceList.UseVisualStyleBackColor = false;
            this.btnMaintenanceList.Click += new System.EventHandler(this.btnMaintenanceList_Click_1);
            // 
            // panel3
            // 
            this.panel3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel3.BackgroundImage")));
            this.panel3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel3.Location = new System.Drawing.Point(12, 24);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(44, 41);
            this.panel3.TabIndex = 12;
            // 
            // TechnicianMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClientSize = new System.Drawing.Size(1334, 641);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelMenu);
            this.Name = "TechnicianMainForm";
            this.Text = "TechnicianMainForm";
            this.panelMenu.ResumeLayout(false);
            this.panelMenu.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelMenu;
        private System.Windows.Forms.Label Username;
        private System.Windows.Forms.Label Role;
        private System.Windows.Forms.Label EmployeeName;
        private System.Windows.Forms.Label EmployeeID;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button btnReports;
        private System.Windows.Forms.Button btnWorkSchedule;
        private System.Windows.Forms.Button btnMaintenanceList;
        private System.Windows.Forms.Panel panel3;
    }
}

