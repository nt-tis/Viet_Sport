namespace CUSTOMER
{
    partial class Search_ChangeCourt
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
            this.panelMain.Location = new System.Drawing.Point(1, 3);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1271, 641);
            this.panelMain.TabIndex = 43;
            // 
            // dgvCourt
            // 
            this.dgvCourt.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCourt.Location = new System.Drawing.Point(12, 62);
            this.dgvCourt.Margin = new System.Windows.Forms.Padding(2);
            this.dgvCourt.Name = "dgvCourt";
            this.dgvCourt.RowHeadersWidth = 62;
            this.dgvCourt.RowTemplate.Height = 28;
            this.dgvCourt.Size = new System.Drawing.Size(1249, 568);
            this.dgvCourt.TabIndex = 63;
            // 
            // btnSearchCourt
            // 
            this.btnSearchCourt.BackColor = System.Drawing.Color.Green;
            this.btnSearchCourt.ForeColor = System.Drawing.Color.White;
            this.btnSearchCourt.Location = new System.Drawing.Point(896, 27);
            this.btnSearchCourt.Margin = new System.Windows.Forms.Padding(2);
            this.btnSearchCourt.Name = "btnSearchCourt";
            this.btnSearchCourt.Size = new System.Drawing.Size(74, 22);
            this.btnSearchCourt.TabIndex = 62;
            this.btnSearchCourt.Text = "Search";
            this.btnSearchCourt.UseVisualStyleBackColor = false;
            this.btnSearchCourt.Click += new System.EventHandler(this.btnSearchCourt_Click);
            // 
            // dtpEndTime
            // 
            this.dtpEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpEndTime.Location = new System.Drawing.Point(670, 28);
            this.dtpEndTime.Margin = new System.Windows.Forms.Padding(2);
            this.dtpEndTime.Name = "dtpEndTime";
            this.dtpEndTime.Size = new System.Drawing.Size(162, 20);
            this.dtpEndTime.TabIndex = 55;
            this.dtpEndTime.UseWaitCursor = true;
            this.dtpEndTime.Value = new System.DateTime(2025, 11, 22, 0, 0, 0, 0);
            // 
            // dtpStartTime
            // 
            this.dtpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpStartTime.Location = new System.Drawing.Point(450, 27);
            this.dtpStartTime.Margin = new System.Windows.Forms.Padding(2);
            this.dtpStartTime.Name = "dtpStartTime";
            this.dtpStartTime.Size = new System.Drawing.Size(137, 20);
            this.dtpStartTime.TabIndex = 55;
            this.dtpStartTime.UseWaitCursor = true;
            this.dtpStartTime.Value = new System.DateTime(2025, 11, 22, 0, 0, 0, 0);
            // 
            // cbSportCenter
            // 
            this.cbSportCenter.FormattingEnabled = true;
            this.cbSportCenter.Items.AddRange(new object[] {
            "All",
            "CEN001",
            "CEN002",
            "CEN003",
            "CEN004",
            "CEN005",
            "CEN006",
            "CEN007",
            "CEN008",
            "CEN009",
            "CEN010"});
            this.cbSportCenter.Location = new System.Drawing.Point(278, 28);
            this.cbSportCenter.Margin = new System.Windows.Forms.Padding(2);
            this.cbSportCenter.Name = "cbSportCenter";
            this.cbSportCenter.Size = new System.Drawing.Size(82, 21);
            this.cbSportCenter.TabIndex = 54;
            this.cbSportCenter.Text = "All";
            // 
            // cbCourtType
            // 
            this.cbCourtType.FormattingEnabled = true;
            this.cbCourtType.Items.AddRange(new object[] {
            "All",
            "BONGDA",
            "CAULONG",
            "BONGRO",
            "TENNIS",
            "FUTSAL"});
            this.cbCourtType.Location = new System.Drawing.Point(101, 29);
            this.cbCourtType.Margin = new System.Windows.Forms.Padding(2);
            this.cbCourtType.Name = "cbCourtType";
            this.cbCourtType.Size = new System.Drawing.Size(82, 21);
            this.cbCourtType.TabIndex = 54;
            this.cbCourtType.Text = "All";
            // 
            // button1
            // 
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(592, 25);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(73, 23);
            this.button1.TabIndex = 58;
            this.button1.Text = "To Date";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.FlatAppearance.BorderSize = 0;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.Location = new System.Drawing.Point(187, 27);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(95, 23);
            this.button2.TabIndex = 59;
            this.button2.Text = "Court Center";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button17
            // 
            this.button17.FlatAppearance.BorderSize = 0;
            this.button17.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button17.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button17.ForeColor = System.Drawing.Color.Black;
            this.button17.Location = new System.Drawing.Point(365, 25);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(93, 23);
            this.button17.TabIndex = 58;
            this.button17.Text = "From Date";
            this.button17.UseVisualStyleBackColor = true;
            // 
            // button16
            // 
            this.button16.FlatAppearance.BorderSize = 0;
            this.button16.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button16.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button16.ForeColor = System.Drawing.Color.Black;
            this.button16.Location = new System.Drawing.Point(12, 26);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(85, 23);
            this.button16.TabIndex = 59;
            this.button16.Text = "Court Type";
            this.button16.UseVisualStyleBackColor = true;
            // 
            // Search_ChangeCourt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClientSize = new System.Drawing.Size(1284, 641);
            this.Controls.Add(this.panelMain);
            this.Name = "Search_ChangeCourt";
            this.Text = "Search_ChangeCourt";
            this.panelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCourt)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.DataGridView dgvCourt;
        private System.Windows.Forms.Button btnSearchCourt;
        private System.Windows.Forms.DateTimePicker dtpEndTime;
        private System.Windows.Forms.DateTimePicker dtpStartTime;
        private System.Windows.Forms.ComboBox cbSportCenter;
        private System.Windows.Forms.ComboBox cbCourtType;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button17;
        private System.Windows.Forms.Button button16;
    }
}
