namespace RSAS.ClientSide
{
    partial class AddServerForm
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
            this.hostAddressTextBox = new System.Windows.Forms.TextBox();
            this.hostPortTextBox = new System.Windows.Forms.TextBox();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.addServerButton = new System.Windows.Forms.Button();
            this.hostAddressLabel = new System.Windows.Forms.Label();
            this.hostPortLabel = new System.Windows.Forms.Label();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.serverNameTextBox = new System.Windows.Forms.TextBox();
            this.serverNameLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // hostAddressTextBox
            // 
            this.hostAddressTextBox.Location = new System.Drawing.Point(84, 46);
            this.hostAddressTextBox.Name = "hostAddressTextBox";
            this.hostAddressTextBox.Size = new System.Drawing.Size(195, 20);
            this.hostAddressTextBox.TabIndex = 1;
            // 
            // hostPortTextBox
            // 
            this.hostPortTextBox.Location = new System.Drawing.Point(84, 73);
            this.hostPortTextBox.Name = "hostPortTextBox";
            this.hostPortTextBox.Size = new System.Drawing.Size(195, 20);
            this.hostPortTextBox.TabIndex = 2;
            // 
            // usernameTextBox
            // 
            this.usernameTextBox.Location = new System.Drawing.Point(84, 100);
            this.usernameTextBox.Name = "usernameTextBox";
            this.usernameTextBox.Size = new System.Drawing.Size(195, 20);
            this.usernameTextBox.TabIndex = 3;
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(84, 127);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(195, 20);
            this.passwordTextBox.TabIndex = 4;
            this.passwordTextBox.UseSystemPasswordChar = true;
            // 
            // addServerButton
            // 
            this.addServerButton.Location = new System.Drawing.Point(204, 164);
            this.addServerButton.Name = "addServerButton";
            this.addServerButton.Size = new System.Drawing.Size(75, 23);
            this.addServerButton.TabIndex = 6;
            this.addServerButton.Text = "Submit";
            this.addServerButton.UseVisualStyleBackColor = true;
            this.addServerButton.Click += new System.EventHandler(this.addServerButton_Click);
            // 
            // hostAddressLabel
            // 
            this.hostAddressLabel.AutoSize = true;
            this.hostAddressLabel.Location = new System.Drawing.Point(6, 49);
            this.hostAddressLabel.Name = "hostAddressLabel";
            this.hostAddressLabel.Size = new System.Drawing.Size(72, 13);
            this.hostAddressLabel.TabIndex = 8;
            this.hostAddressLabel.Text = "Host address:";
            // 
            // hostPortLabel
            // 
            this.hostPortLabel.AutoSize = true;
            this.hostPortLabel.Location = new System.Drawing.Point(25, 76);
            this.hostPortLabel.Name = "hostPortLabel";
            this.hostPortLabel.Size = new System.Drawing.Size(53, 13);
            this.hostPortLabel.TabIndex = 9;
            this.hostPortLabel.Text = "Host port:";
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.Location = new System.Drawing.Point(20, 103);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(58, 13);
            this.usernameLabel.TabIndex = 10;
            this.usernameLabel.Text = "Username:";
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(22, 130);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(56, 13);
            this.passwordLabel.TabIndex = 11;
            this.passwordLabel.Text = "Password:";
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(123, 164);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // serverNameTextBox
            // 
            this.serverNameTextBox.Location = new System.Drawing.Point(84, 20);
            this.serverNameTextBox.Name = "serverNameTextBox";
            this.serverNameTextBox.Size = new System.Drawing.Size(195, 20);
            this.serverNameTextBox.TabIndex = 0;
            // 
            // serverNameLabel
            // 
            this.serverNameLabel.AutoSize = true;
            this.serverNameLabel.Location = new System.Drawing.Point(6, 23);
            this.serverNameLabel.Name = "serverNameLabel";
            this.serverNameLabel.Size = new System.Drawing.Size(72, 13);
            this.serverNameLabel.TabIndex = 7;
            this.serverNameLabel.Text = "Server Name:";
            // 
            // AddServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 204);
            this.Controls.Add(this.serverNameLabel);
            this.Controls.Add(this.serverNameTextBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.usernameLabel);
            this.Controls.Add(this.hostPortLabel);
            this.Controls.Add(this.hostAddressLabel);
            this.Controls.Add(this.addServerButton);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.usernameTextBox);
            this.Controls.Add(this.hostPortTextBox);
            this.Controls.Add(this.hostAddressTextBox);
            this.Name = "AddServerForm";
            this.Text = "Server Details";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox hostAddressTextBox;
        private System.Windows.Forms.TextBox hostPortTextBox;
        private System.Windows.Forms.TextBox usernameTextBox;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Button addServerButton;
        private System.Windows.Forms.Label hostAddressLabel;
        private System.Windows.Forms.Label hostPortLabel;
        private System.Windows.Forms.Label usernameLabel;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox serverNameTextBox;
        private System.Windows.Forms.Label serverNameLabel;
    }
}