﻿namespace CUSTOMER
{
    partial class Change_Court
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
            this.panelMain = new System.Windows.Forms.Panel();
            this.OldCenterID = new System.Windows.Forms.TextBox();
            this.OldCourtID = new System.Windows.Forms.TextBox();
            this.btnBook = new System.Windows.Forms.Button();
            this.OldEnd = new System.Windows.Forms.DateTimePicker();
            this.OldStart = new System.Windows.Forms.DateTimePicker();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.NewStart = new System.Windows.Forms.DateTimePicker();
            this.NewEnd = new System.Windows.Forms.DateTimePicker();
            this.NewCourtID = new System.Windows.Forms.TextBox();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.OldCenterID);
            this.panelMain.Controls.Add(this.NewCourtID);
            this.panelMain.Controls.Add(this.OldCourtID);
            this.panelMain.Controls.Add(this.NewEnd);
            this.panelMain.Controls.Add(this.btnBook);
            this.panelMain.Controls.Add(this.NewStart);
            this.panelMain.Controls.Add(this.OldEnd);
            this.panelMain.Controls.Add(this.OldStart);
            this.panelMain.Controls.Add(this.label6);
            this.panelMain.Controls.Add(this.label8);
            this.panelMain.Controls.Add(this.label2);
            this.panelMain.Controls.Add(this.label5);
            this.panelMain.Controls.Add(this.label1);
            this.panelMain.Controls.Add(this.label4);
            this.panelMain.Controls.Add(this.label3);
            this.panelMain.Controls.Add(this.textBox1);
            this.panelMain.Location = new System.Drawing.Point(113, 64);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1058, 565);
            this.panelMain.TabIndex = 43;
            // 
            // OldCenterID
            // 
            this.OldCenterID.Location = new System.Drawing.Point(595, 175);
            this.OldCenterID.Name = "OldCenterID";
            this.OldCenterID.Size = new System.Drawing.Size(200, 20);
            this.OldCenterID.TabIndex = 26;
            this.OldCenterID.Text = "CEN001";
            // 
            // OldCourtID
            // 
            this.OldCourtID.Location = new System.Drawing.Point(185, 175);
            this.OldCourtID.Name = "OldCourtID";
            this.OldCourtID.Size = new System.Drawing.Size(200, 20);
            this.OldCourtID.TabIndex = 25;
            this.OldCourtID.Text = "MF_01";
            // 
            // btnBook
            // 
            this.btnBook.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.btnBook.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBook.Location = new System.Drawing.Point(388, 492);
            this.btnBook.Name = "btnBook";
            this.btnBook.Size = new System.Drawing.Size(90, 35);
            this.btnBook.TabIndex = 24;
            this.btnBook.Text = "BOOK";
            this.btnBook.UseVisualStyleBackColor = false;
            this.btnBook.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // OldEnd
            // 
            this.OldEnd.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.OldEnd.Location = new System.Drawing.Point(595, 223);
            this.OldEnd.Name = "OldEnd";
            this.OldEnd.Size = new System.Drawing.Size(200, 20);
            this.OldEnd.TabIndex = 21;
            // 
            // OldStart
            // 
            this.OldStart.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.OldStart.Location = new System.Drawing.Point(185, 224);
            this.OldStart.Name = "OldStart";
            this.OldStart.Size = new System.Drawing.Size(200, 20);
            this.OldStart.TabIndex = 22;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(492, 177);
            this.label8.Name = "label8";
            this.label8.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label8.Size = new System.Drawing.Size(49, 13);
            this.label8.TabIndex = 12;
            this.label8.Text = "CenterID";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 230);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Old Start Time";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(492, 229);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Old End Time";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 177);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Old CourtID";
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(304, 71);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(264, 23);
            this.textBox1.TabIndex = 11;
            this.textBox1.Text = "CHANGE COURT/ HOUR";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 287);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "New CourtID (Optional)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 396);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "New End Time";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(18, 340);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "New Start Time";
            // 
            // NewStart
            // 
            this.NewStart.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.NewStart.Location = new System.Drawing.Point(185, 334);
            this.NewStart.Name = "NewStart";
            this.NewStart.Size = new System.Drawing.Size(200, 20);
            this.NewStart.TabIndex = 22;
            // 
            // NewEnd
            // 
            this.NewEnd.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.NewEnd.Location = new System.Drawing.Point(185, 391);
            this.NewEnd.Name = "NewEnd";
            this.NewEnd.Size = new System.Drawing.Size(200, 20);
            this.NewEnd.TabIndex = 21;
            // 
            // NewCourtID
            // 
            this.NewCourtID.Location = new System.Drawing.Point(185, 285);
            this.NewCourtID.Name = "NewCourtID";
            this.NewCourtID.Size = new System.Drawing.Size(200, 20);
            this.NewCourtID.TabIndex = 25;
            this.NewCourtID.Text = "MF_01";
            // 
            // Change_Court
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClientSize = new System.Drawing.Size(1284, 641);
            this.Controls.Add(this.panelMain);
            this.Name = "Change_Court";
            this.Text = "Change_Court";
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.TextBox OldCenterID;
        private System.Windows.Forms.TextBox OldCourtID;
        private System.Windows.Forms.Button btnBook;
        private System.Windows.Forms.DateTimePicker OldEnd;
        private System.Windows.Forms.DateTimePicker OldStart;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox NewCourtID;
        private System.Windows.Forms.DateTimePicker NewEnd;
        private System.Windows.Forms.DateTimePicker NewStart;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}
