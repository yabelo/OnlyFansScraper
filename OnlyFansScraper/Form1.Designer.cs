using System;

namespace OnlyFansScraper {
    partial class OnlyFansScraper {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OnlyFansScraper));
            this.queryTextBox = new System.Windows.Forms.TextBox();
            this.queryTxt = new System.Windows.Forms.Label();
            this.searchBtn = new System.Windows.Forms.Button();
            this.pagesLabel = new System.Windows.Forms.Label();
            this.numPagesNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.saveVideosNameCheckBox = new System.Windows.Forms.CheckBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.tag = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.userAgentTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numPagesNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // queryTextBox
            // 
            this.queryTextBox.Location = new System.Drawing.Point(252, 170);
            this.queryTextBox.Name = "queryTextBox";
            this.queryTextBox.Size = new System.Drawing.Size(421, 19);
            this.queryTextBox.TabIndex = 1;
            this.queryTextBox.TextChanged += new System.EventHandler(this.query_TextChanged);
            // 
            // queryTxt
            // 
            this.queryTxt.AutoSize = true;
            this.queryTxt.Font = new System.Drawing.Font("Monocraft", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.queryTxt.Location = new System.Drawing.Point(156, 164);
            this.queryTxt.Name = "queryTxt";
            this.queryTxt.Size = new System.Drawing.Size(90, 25);
            this.queryTxt.TabIndex = 2;
            this.queryTxt.Text = "query:";
            this.queryTxt.Click += new System.EventHandler(this.queryTxt_Click);
            // 
            // searchBtn
            // 
            this.searchBtn.BackColor = System.Drawing.SystemColors.ControlLight;
            this.searchBtn.Location = new System.Drawing.Point(361, 422);
            this.searchBtn.Name = "searchBtn";
            this.searchBtn.Size = new System.Drawing.Size(75, 23);
            this.searchBtn.TabIndex = 3;
            this.searchBtn.Text = "Search";
            this.searchBtn.UseVisualStyleBackColor = false;
            this.searchBtn.Click += new System.EventHandler(this.searchBtn_Click_1);
            // 
            // pagesLabel
            // 
            this.pagesLabel.AutoSize = true;
            this.pagesLabel.Font = new System.Drawing.Font("Monocraft", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.pagesLabel.Location = new System.Drawing.Point(156, 192);
            this.pagesLabel.Name = "pagesLabel";
            this.pagesLabel.Size = new System.Drawing.Size(90, 25);
            this.pagesLabel.TabIndex = 5;
            this.pagesLabel.Text = "pages:";
            this.pagesLabel.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // numPagesNumericUpDown
            // 
            this.numPagesNumericUpDown.Location = new System.Drawing.Point(252, 195);
            this.numPagesNumericUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numPagesNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPagesNumericUpDown.Name = "numPagesNumericUpDown";
            this.numPagesNumericUpDown.Size = new System.Drawing.Size(120, 19);
            this.numPagesNumericUpDown.TabIndex = 6;
            this.numPagesNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPagesNumericUpDown.ValueChanged += new System.EventHandler(this.numPagesNumericUpDown_ValueChanged);
            // 
            // saveVideosNameCheckBox
            // 
            this.saveVideosNameCheckBox.AutoSize = true;
            this.saveVideosNameCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.saveVideosNameCheckBox.Checked = true;
            this.saveVideosNameCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.saveVideosNameCheckBox.Font = new System.Drawing.Font("Monocraft", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.saveVideosNameCheckBox.Location = new System.Drawing.Point(156, 220);
            this.saveVideosNameCheckBox.Name = "saveVideosNameCheckBox";
            this.saveVideosNameCheckBox.Size = new System.Drawing.Size(252, 29);
            this.saveVideosNameCheckBox.TabIndex = 7;
            this.saveVideosNameCheckBox.Text = "save videos name:";
            this.saveVideosNameCheckBox.UseVisualStyleBackColor = true;
            this.saveVideosNameCheckBox.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // progressBar
            // 
            this.progressBar.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.progressBar.ForeColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.progressBar.Location = new System.Drawing.Point(161, 323);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(512, 23);
            this.progressBar.TabIndex = 8;
            // 
            // tag
            // 
            this.tag.AutoSize = true;
            this.tag.Font = new System.Drawing.Font("Monocraft", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.tag.Location = new System.Drawing.Point(91, 39);
            this.tag.Name = "tag";
            this.tag.Size = new System.Drawing.Size(647, 49);
            this.tag.TabIndex = 9;
            this.tag.Text = "OnlyFans Scraper By Yablo";
            this.tag.Click += new System.EventHandler(this.tag_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Monocraft", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label1.Location = new System.Drawing.Point(156, 249);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(298, 25);
            this.label1.TabIndex = 11;
            this.label1.Text = "user-agent (optional):";
            this.label1.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // userAgentTextBox
            // 
            this.userAgentTextBox.Location = new System.Drawing.Point(451, 255);
            this.userAgentTextBox.Name = "userAgentTextBox";
            this.userAgentTextBox.Size = new System.Drawing.Size(222, 19);
            this.userAgentTextBox.TabIndex = 10;
            this.userAgentTextBox.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // OnlyFansScraper
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(884, 484);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.userAgentTextBox);
            this.Controls.Add(this.tag);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.saveVideosNameCheckBox);
            this.Controls.Add(this.numPagesNumericUpDown);
            this.Controls.Add(this.pagesLabel);
            this.Controls.Add(this.searchBtn);
            this.Controls.Add(this.queryTxt);
            this.Controls.Add(this.queryTextBox);
            this.Font = new System.Drawing.Font("Monocraft", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OnlyFansScraper";
            this.Text = "OnlyFans Scraper By Yablo";
            ((System.ComponentModel.ISupportInitialize)(this.numPagesNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
        }

        private void numPagesNumericUpDown_ValueChanged(object sender, EventArgs e) {
        }

        private void label1_Click_1(object sender, EventArgs e) {
        }

        private void queryTxt_Click(object sender, EventArgs e) {
        }

        private void query_TextChanged(object sender, EventArgs e) {
            
        }

        #endregion

        private System.Windows.Forms.TextBox queryTextBox;
        private System.Windows.Forms.Label queryTxt;
        private System.Windows.Forms.Button searchBtn;
        private System.Windows.Forms.Label pagesLabel;
        private System.Windows.Forms.NumericUpDown numPagesNumericUpDown;
        private System.Windows.Forms.CheckBox saveVideosNameCheckBox;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label tag;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox userAgentTextBox;
    }
}

