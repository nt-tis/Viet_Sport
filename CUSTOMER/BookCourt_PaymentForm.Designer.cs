﻿namespace CUSTOMER
{
    partial class BookCourt_PaymentForm
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
            this.comboMethod = new System.Windows.Forms.ComboBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.TotalPrice = new System.Windows.Forms.TextBox();
            this.Quantity = new System.Windows.Forms.TextBox();
            this.ServiceName = new System.Windows.Forms.TextBox();
            this.BookingID = new System.Windows.Forms.TextBox();
            this.CourtID = new System.Windows.Forms.TextBox();
            this.btnPay = new System.Windows.Forms.Button();
            this.EndTime = new System.Windows.Forms.DateTimePicker();
            this.StartTime = new System.Windows.Forms.DateTimePicker();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.CenterID = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.CustomerID = new System.Windows.Forms.TextBox();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.comboMethod);
            this.panelMain.Controls.Add(this.textBox5);
            this.panelMain.Controls.Add(this.TotalPrice);
            this.panelMain.Controls.Add(this.Quantity);
            this.panelMain.Controls.Add(this.ServiceName);
            this.panelMain.Controls.Add(this.CustomerID);
            this.panelMain.Controls.Add(this.BookingID);
            this.panelMain.Controls.Add(this.CenterID);
            this.panelMain.Controls.Add(this.CourtID);
            this.panelMain.Controls.Add(this.btnPay);
            this.panelMain.Controls.Add(this.EndTime);
            this.panelMain.Controls.Add(this.StartTime);
            this.panelMain.Controls.Add(this.label8);
            this.panelMain.Controls.Add(this.label7);
            this.panelMain.Controls.Add(this.label9);
            this.panelMain.Controls.Add(this.label6);
            this.panelMain.Controls.Add(this.label10);
            this.panelMain.Controls.Add(this.label5);
            this.panelMain.Controls.Add(this.label1);
            this.panelMain.Controls.Add(this.label2);
            this.panelMain.Controls.Add(this.label4);
            this.panelMain.Controls.Add(this.label3);
            this.panelMain.Controls.Add(this.textBox1);
            this.panelMain.Location = new System.Drawing.Point(2, 3);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1067, 641);
            this.panelMain.TabIndex = 42;
            // 
            // comboMethod
            // 
            this.comboMethod.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboMethod.FormattingEnabled = true;
            this.comboMethod.Items.AddRange(new object[] {
            "Online",
            "Tiền mặt"});
            this.comboMethod.Location = new System.Drawing.Point(505, 420);
            this.comboMethod.Name = "comboMethod";
            this.comboMethod.Size = new System.Drawing.Size(113, 24);
            this.comboMethod.TabIndex = 42;
            this.comboMethod.Text = "Choose...";
            // 
            // textBox5
            // 
            this.textBox5.BackColor = System.Drawing.SystemColors.Control;
            this.textBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox5.Location = new System.Drawing.Point(226, 338);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(428, 13);
            this.textBox5.TabIndex = 41;
            this.textBox5.Text = "_________________________________________________________________________________" +
    "________________________________________________________________________________" +
    "______________________";
            // 
            // TotalPrice
            // 
            this.TotalPrice.BackColor = System.Drawing.SystemColors.Control;
            this.TotalPrice.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TotalPrice.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TotalPrice.ForeColor = System.Drawing.Color.Red;
            this.TotalPrice.Location = new System.Drawing.Point(490, 378);
            this.TotalPrice.Name = "TotalPrice";
            this.TotalPrice.Size = new System.Drawing.Size(128, 23);
            this.TotalPrice.TabIndex = 40;
            this.TotalPrice.Text = "100,000 VND";
            // 
            // Quantity
            // 
            this.Quantity.Location = new System.Drawing.Point(585, 270);
            this.Quantity.Name = "Quantity";
            this.Quantity.Size = new System.Drawing.Size(200, 20);
            this.Quantity.TabIndex = 39;
            this.Quantity.Text = "2";
            // 
            // ServiceName
            // 
            this.ServiceName.Location = new System.Drawing.Point(203, 269);
            this.ServiceName.Name = "ServiceName";
            this.ServiceName.Size = new System.Drawing.Size(200, 20);
            this.ServiceName.TabIndex = 39;
            this.ServiceName.Text = "Ball";
            // 
            // BookingID
            // 
            this.BookingID.Location = new System.Drawing.Point(203, 90);
            this.BookingID.Name = "BookingID";
            this.BookingID.Size = new System.Drawing.Size(200, 20);
            this.BookingID.TabIndex = 39;
            this.BookingID.Text = "MF_01";
            // 
            // CourtID
            // 
            this.CourtID.Location = new System.Drawing.Point(203, 127);
            this.CourtID.Name = "CourtID";
            this.CourtID.Size = new System.Drawing.Size(200, 20);
            this.CourtID.TabIndex = 39;
            this.CourtID.Text = "MF_01";
            // 
            // btnPay
            // 
            this.btnPay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.btnPay.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPay.Location = new System.Drawing.Point(376, 463);
            this.btnPay.Name = "btnPay";
            this.btnPay.Size = new System.Drawing.Size(90, 35);
            this.btnPay.TabIndex = 38;
            this.btnPay.Text = "OK";
            this.btnPay.UseVisualStyleBackColor = false;
            this.btnPay.Click += new System.EventHandler(this.btnPay_Click);
            // 
            // EndTime
            // 
            this.EndTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.EndTime.Location = new System.Drawing.Point(203, 225);
            this.EndTime.Name = "EndTime";
            this.EndTime.Size = new System.Drawing.Size(200, 20);
            this.EndTime.TabIndex = 35;
            this.EndTime.Value = new System.DateTime(2025, 11, 23, 15, 0, 0, 0);
            // 
            // StartTime
            // 
            this.StartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.StartTime.Location = new System.Drawing.Point(203, 181);
            this.StartTime.Name = "StartTime";
            this.StartTime.Size = new System.Drawing.Size(200, 20);
            this.StartTime.TabIndex = 36;
            this.StartTime.Value = new System.DateTime(2025, 11, 23, 13, 0, 0, 0);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Red;
            this.label8.Location = new System.Drawing.Point(262, 378);
            this.label8.Name = "label8";
            this.label8.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label8.Size = new System.Drawing.Size(116, 25);
            this.label8.TabIndex = 27;
            this.label8.Text = "Total Price";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(506, 272);
            this.label7.Name = "label7";
            this.label7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label7.Size = new System.Drawing.Size(46, 13);
            this.label7.TabIndex = 27;
            this.label7.Text = "Quantity";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(264, 423);
            this.label9.Name = "label9";
            this.label9.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label9.Size = new System.Drawing.Size(59, 17);
            this.label9.TabIndex = 28;
            this.label9.Text = "Method:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(36, 273);
            this.label6.Name = "label6";
            this.label6.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label6.Size = new System.Drawing.Size(48, 13);
            this.label6.TabIndex = 28;
            this.label6.Text = "Services";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(36, 187);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 13);
            this.label5.TabIndex = 29;
            this.label5.Text = "Start Time";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 93);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 31;
            this.label1.Text = "BookingID";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(36, 230);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 30;
            this.label4.Text = "End Time";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(37, 131);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 31;
            this.label3.Text = "CourtID";
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(318, 48);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(250, 23);
            this.textBox1.TabIndex = 26;
            this.textBox1.Text = "PAYMENT FORM";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(449, 131);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 31;
            this.label2.Text = "CenterID";
            // 
            // CenterID
            // 
            this.CenterID.Location = new System.Drawing.Point(615, 127);
            this.CenterID.Name = "CenterID";
            this.CenterID.Size = new System.Drawing.Size(200, 20);
            this.CenterID.TabIndex = 39;
            this.CenterID.Text = "MF_01";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(448, 93);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(62, 13);
            this.label10.TabIndex = 31;
            this.label10.Text = "CustomerID";
            // 
            // CustomerID
            // 
            this.CustomerID.Location = new System.Drawing.Point(615, 90);
            this.CustomerID.Name = "CustomerID";
            this.CustomerID.Size = new System.Drawing.Size(200, 20);
            this.CustomerID.TabIndex = 39;
            this.CustomerID.Text = "MF_01";
            // 
            // BookCourt_PaymentForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClientSize = new System.Drawing.Size(1284, 641);
            this.Controls.Add(this.panelMain);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "BookCourt_PaymentForm";
            this.Text = "BookCourt_PaymentForm";
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.ComboBox comboMethod;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.TextBox TotalPrice;
        private System.Windows.Forms.TextBox Quantity;
        private System.Windows.Forms.TextBox ServiceName;
        private System.Windows.Forms.TextBox CourtID;
        private System.Windows.Forms.Button btnPay;
        private System.Windows.Forms.DateTimePicker EndTime;
        private System.Windows.Forms.DateTimePicker StartTime;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox BookingID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox CenterID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox CustomerID;
        private System.Windows.Forms.Label label10;
    }
}
