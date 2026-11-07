﻿namespace CASHIER
{
    partial class Cashier_CancellationFee
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
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnSearch = new System.Windows.Forms.Button();
            this.txtPenaltyFee = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtCustomerName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBookingID = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtCancelFormID = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnProcessFee = new System.Windows.Forms.Button();
            this.dgvCancellations = new System.Windows.Forms.DataGridView();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCancellations)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.White;
            this.panelTop.Controls.Add(this.btnRefresh);
            this.panelTop.Controls.Add(this.btnSearch);
            this.panelTop.Controls.Add(this.txtPenaltyFee);
            this.panelTop.Controls.Add(this.label3);
            this.panelTop.Controls.Add(this.txtCustomerName);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.txtBookingID);
            this.panelTop.Controls.Add(this.label5);
            this.panelTop.Controls.Add(this.txtCancelFormID);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(2);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(825, 131);
            this.panelTop.TabIndex = 0;
            this.panelTop.Paint += new System.Windows.Forms.PaintEventHandler(this.panelTop_Paint);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Location = new System.Drawing.Point(702, 60);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(2);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(90, 28);
            this.btnRefresh.TabIndex = 11;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Location = new System.Drawing.Point(702, 24);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(2);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(90, 28);
            this.btnSearch.TabIndex = 10;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // txtPenaltyFee
            // 
            this.txtPenaltyFee.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPenaltyFee.Location = new System.Drawing.Point(525, 28);
            this.txtPenaltyFee.Margin = new System.Windows.Forms.Padding(2);
            this.txtPenaltyFee.Name = "txtPenaltyFee";
            this.txtPenaltyFee.ReadOnly = true;
            this.txtPenaltyFee.Size = new System.Drawing.Size(151, 23);
            this.txtPenaltyFee.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label3.Location = new System.Drawing.Point(412, 31);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "Phí phạt:";
            // 
            // txtCustomerName
            // 
            this.txtCustomerName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtCustomerName.Location = new System.Drawing.Point(165, 98);
            this.txtCustomerName.Margin = new System.Windows.Forms.Padding(2);
            this.txtCustomerName.Name = "txtCustomerName";
            this.txtCustomerName.ReadOnly = true;
            this.txtCustomerName.Size = new System.Drawing.Size(196, 23);
            this.txtCustomerName.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label2.Location = new System.Drawing.Point(22, 100);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Khách hàng:";
            // 
            // txtBookingID
            // 
            this.txtBookingID.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtBookingID.Location = new System.Drawing.Point(165, 65);
            this.txtBookingID.Margin = new System.Windows.Forms.Padding(2);
            this.txtBookingID.Name = "txtBookingID";
            this.txtBookingID.Size = new System.Drawing.Size(196, 23);
            this.txtBookingID.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label5.ForeColor = System.Drawing.Color.Gray;
            this.label5.Location = new System.Drawing.Point(22, 67);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(139, 15);
            this.label5.TabIndex = 2;
            this.label5.Text = "Tìm (ID/BookingID/SĐT):";
            // 
            // txtCancelFormID
            // 
            this.txtCancelFormID.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtCancelFormID.Location = new System.Drawing.Point(165, 28);
            this.txtCancelFormID.Margin = new System.Windows.Forms.Padding(2);
            this.txtCancelFormID.Name = "txtCancelFormID";
            this.txtCancelFormID.ReadOnly = true;
            this.txtCancelFormID.Size = new System.Drawing.Size(196, 23);
            this.txtCancelFormID.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label1.Location = new System.Drawing.Point(22, 31);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Mã phiếu hủy:";
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnProcessFee);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 479);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(2);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(825, 57);
            this.panelBottom.TabIndex = 1;
            // 
            // btnProcessFee
            // 
            this.btnProcessFee.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnProcessFee.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProcessFee.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnProcessFee.ForeColor = System.Drawing.Color.White;
            this.btnProcessFee.Location = new System.Drawing.Point(712, 12);
            this.btnProcessFee.Margin = new System.Windows.Forms.Padding(2);
            this.btnProcessFee.Name = "btnProcessFee";
            this.btnProcessFee.Size = new System.Drawing.Size(90, 32);
            this.btnProcessFee.TabIndex = 0;
            this.btnProcessFee.Text = "APPROVE";
            this.btnProcessFee.UseVisualStyleBackColor = false;
            this.btnProcessFee.Click += new System.EventHandler(this.btnProcessFee_Click);
            // 
            // dgvCancellations
            // 
            this.dgvCancellations.AllowUserToAddRows = false;
            this.dgvCancellations.AllowUserToDeleteRows = false;
            this.dgvCancellations.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvCancellations.BackgroundColor = System.Drawing.Color.White;
            this.dgvCancellations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCancellations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCancellations.Location = new System.Drawing.Point(0, 131);
            this.dgvCancellations.Margin = new System.Windows.Forms.Padding(2);
            this.dgvCancellations.Name = "dgvCancellations";
            this.dgvCancellations.ReadOnly = true;
            this.dgvCancellations.RowHeadersWidth = 51;
            this.dgvCancellations.RowTemplate.Height = 24;
            this.dgvCancellations.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCancellations.Size = new System.Drawing.Size(825, 348);
            this.dgvCancellations.TabIndex = 2;
            this.dgvCancellations.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCancellations_CellClick);
            this.dgvCancellations.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCancellations_CellContentClick);
            // 
            // Cashier_CancellationFee
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(825, 536);
            this.Controls.Add(this.dgvCancellations);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Cashier_CancellationFee";
            this.Text = "Cancellation Fee";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCancellations)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.TextBox txtCancelFormID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBookingID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtCustomerName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPenaltyFee;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnProcessFee;
        private System.Windows.Forms.DataGridView dgvCancellations;
    }
}
