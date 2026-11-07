namespace CUSTOMER
{
    partial class View_Court
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
            this.dgvCourt = new System.Windows.Forms.DataGridView();
            this.btnSearchCourt = new System.Windows.Forms.Button();
            this.dtpEndTime = new System.Windows.Forms.DateTimePicker();
            this.dtpStartTime = new System.Windows.Forms.DateTimePicker();
            this.cbSportCenter = new System.Windows.Forms.ComboBox();
            this.cbCourtType = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button17 = new System.Windows.Forms.Button();
            this.button16 = new System.Windows.Forms.Button();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCourt)).BeginInit();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.dgvCourt);
            this.panelMain.Controls.Add(this.btnSearchCourt);
            this.panelMain.Controls.Add(this.dtpEndTime);
            this.panelMain.Controls.Add(this.dtpStartTime);
            this.panelMain.Controls.Add(this.cbSportCenter);
            this.panelMain.Controls.Add(this.cbCourtType);
            this.panelMain.Controls.Add(this.button1);
            this.panelMain.Controls.Add(this.button2);
            this.panelMain.Controls.Add(this.button17);
            this.panelMain.Controls.Add(this.button16);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1284, 641);
            this.panelMain.TabIndex = 42;
            // 
            // dgvCourt
            // 
            this.dgvCourt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCourt.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCourt.Location = new System.Drawing.Point(12, 62);
            this.dgvCourt.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvCourt.Name = "dgvCourt";
            this.dgvCourt.RowHeadersWidth = 62;
            this.dgvCourt.RowTemplate.Height = 32;
            this.dgvCourt.Size = new System.Drawing.Size(1260, 568);
            this.dgvCourt.TabIndex = 63;
            // 
            // btnSearchCourt
            // 
            this.btnSearchCourt.BackColor = System.Drawing.Color.ForestGreen;
            this.btnSearchCourt.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchCourt.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSearchCourt.ForeColor = System.Drawing.Color.White;
            this.btnSearchCourt.Location = new System.Drawing.Point(886, 25);
            this.btnSearchCourt.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSearchCourt.Name = "btnSearchCourt";
            this.btnSearchCourt.Size = new System.Drawing.Size(95, 26);
            this.btnSearchCourt.TabIndex = 62;
            this.btnSearchCourt.Text = "Tra Cứu";
            this.btnSearchCourt.UseVisualStyleBackColor = false;
            this.btnSearchCourt.Click += new System.EventHandler(this.btnSearchCourt_Click);
            // 
            // dtpEndTime
            // 
            this.dtpEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEndTime.Location = new System.Drawing.Point(710, 27);
            this.dtpEndTime.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dtpEndTime.Name = "dtpEndTime";
            this.dtpEndTime.Size = new System.Drawing.Size(162, 20);
            this.dtpEndTime.TabIndex = 55;
            this.dtpEndTime.UseWaitCursor = false;
            // 
            // dtpStartTime
            // 
            this.dtpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartTime.Location = new System.Drawing.Point(470, 27);
            this.dtpStartTime.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dtpStartTime.Name = "dtpStartTime";
            this.dtpStartTime.Size = new System.Drawing.Size(155, 20);
            this.dtpStartTime.TabIndex = 55;
            this.dtpStartTime.UseWaitCursor = false;
            // 
            // cbSportCenter
            // 
            this.cbSportCenter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSportCenter.FormattingEnabled = true;
            this.cbSportCenter.Location = new System.Drawing.Point(280, 27);
            this.cbSportCenter.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cbSportCenter.Name = "cbSportCenter";
            this.cbSportCenter.Size = new System.Drawing.Size(95, 21);
            this.cbSportCenter.TabIndex = 54;
            // 
            // cbCourtType
            // 
            this.cbCourtType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCourtType.FormattingEnabled = true;
            this.cbCourtType.Location = new System.Drawing.Point(85, 27);
            this.cbCourtType.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cbCourtType.Name = "cbCourtType";
            this.cbCourtType.Size = new System.Drawing.Size(100, 21);
            this.cbCourtType.TabIndex = 54;
            // 
            // button1
            // 
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(635, 25);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(73, 23);
            this.button1.TabIndex = 58;
            this.button1.Text = "Đến Giờ:";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.FlatAppearance.BorderSize = 0;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.Location = new System.Drawing.Point(190, 26);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(88, 23);
            this.button2.TabIndex = 59;
            this.button2.Text = "Trung Tâm:";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button17
            // 
            this.button17.FlatAppearance.BorderSize = 0;
            this.button17.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button17.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button17.ForeColor = System.Drawing.Color.Black;
            this.button17.Location = new System.Drawing.Point(390, 25);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(75, 23);
            this.button17.TabIndex = 58;
            this.button17.Text = "Từ Giờ:";
            this.button17.UseVisualStyleBackColor = true;
            // 
            // button16
            // 
            this.button16.FlatAppearance.BorderSize = 0;
            this.button16.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button16.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button16.ForeColor = System.Drawing.Color.Black;
            this.button16.Location = new System.Drawing.Point(8, 26);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(75, 23);
            this.button16.TabIndex = 59;
            this.button16.Text = "Loại Sân:";
            this.button16.UseVisualStyleBackColor = true;
            // 
            // View_Court
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClientSize = new System.Drawing.Size(1284, 641);
            this.Controls.Add(this.panelMain);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "View_Court";
            this.Text = "View_Court";
            this.Load += new System.EventHandler(this.View_Court_Load);
            this.panelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCourt)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Button btnSearchCourt;
        private System.Windows.Forms.DateTimePicker dtpStartTime;
        private System.Windows.Forms.ComboBox cbCourtType;
        private System.Windows.Forms.Button button17;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.DateTimePicker dtpEndTime;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox cbSportCenter;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.DataGridView dgvCourt;
    }
}
