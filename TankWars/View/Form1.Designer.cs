namespace View {
    partial class Form1 {
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
            this.ServerTextBox = new System.Windows.Forms.TextBox();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.NameLabel = new System.Windows.Forms.Label();
            this.ServerLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ServerTextBox
            // 
            this.ServerTextBox.Location = new System.Drawing.Point(69, 6);
            this.ServerTextBox.Name = "ServerTextBox";
            this.ServerTextBox.Size = new System.Drawing.Size(151, 26);
            this.ServerTextBox.TabIndex = 1;
            this.ServerTextBox.Text = "localhost";
            // 
            // NameTextBox
            // 
            this.NameTextBox.Location = new System.Drawing.Point(291, 6);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(193, 26);
            this.NameTextBox.TabIndex = 3;
            this.NameTextBox.Text = "player";
            // 
            // ConnectButton
            // 
            this.ConnectButton.BackColor = System.Drawing.SystemColors.MenuBar;
            this.ConnectButton.Location = new System.Drawing.Point(500, 4);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(85, 30);
            this.ConnectButton.TabIndex = 4;
            this.ConnectButton.Text = "connect";
            this.ConnectButton.UseVisualStyleBackColor = false;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.AutoSize = false;
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 40);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.BackColor = System.Drawing.SystemColors.MenuBar;
            this.NameLabel.Location = new System.Drawing.Point(226, 9);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(59, 20);
            this.NameLabel.TabIndex = 6;
            this.NameLabel.Text = "Name: ";
            // 
            // ServerLabel
            // 
            this.ServerLabel.AutoSize = true;
            this.ServerLabel.BackColor = System.Drawing.SystemColors.MenuBar;
            this.ServerLabel.Location = new System.Drawing.Point(0, 9);
            this.ServerLabel.Name = "ServerLabel";
            this.ServerLabel.Size = new System.Drawing.Size(63, 20);
            this.ServerLabel.TabIndex = 7;
            this.ServerLabel.Text = "Server: ";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ServerLabel);
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.NameTextBox);
            this.Controls.Add(this.ServerTextBox);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox ServerTextBox;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.Label ServerLabel;
    }
}

