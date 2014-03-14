using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;

namespace RSAS.ClientSide
{
    public partial class AddServerForm : Form
    {
        public delegate void AddServerFormDetailsSubmittedEventHandler(object sender, AddServerFormDetailsSubmittedEventArgs e);

        public event AddServerFormDetailsSubmittedEventHandler DetailsSubmitted;

        ErrorProvider errorProvider = new ErrorProvider();

        bool serverNameValid = false;
        bool hostAddressValid = false;
        bool hostPortValid = false;
        bool usernameValid = false;
        bool passwordValid = false;

        UInt16 hostPort;
        IPAddress hostAddress;

        public AddServerForm()
        {
            InitializeComponent();
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            this.serverNameTextBox.Validating += new CancelEventHandler(serverNameTextBox_Validating);
            this.hostAddressTextBox.Validating += new CancelEventHandler(hostAddressTextBox_Validating);
            this.hostPortTextBox.Validating += new CancelEventHandler(hostPortTextBox_Validating);
            this.usernameTextBox.Validating += new CancelEventHandler(usernameTextBox_Validating);
            this.passwordTextBox.Validating += new CancelEventHandler(passwordTextBox_Validating);
        }

        void passwordTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (passwordTextBox.Text.Length != 0)
            {
                passwordValid = true;
                errorProvider.SetError(passwordTextBox, null);
            }
            else
            {
                passwordValid = false;
                errorProvider.SetError(passwordTextBox, "Password must be at least 1 character long.");
            }
        }

        void usernameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (usernameTextBox.Text.Length != 0)
            {
                usernameValid = true;
                errorProvider.SetError(usernameTextBox, null);
            }
            else
            {
                usernameValid = false;
                errorProvider.SetError(usernameTextBox, "Username must be at least 1 character long.");
            }
        }

        void hostPortTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (UInt16.TryParse(hostPortTextBox.Text, out hostPort))
            {
                hostPortValid = true;
                errorProvider.SetError(hostPortTextBox, null);
            }
            else
            {
                hostPortValid = false;
                errorProvider.SetError(hostPortTextBox, "Port must be a positive numerical value less than " + UInt16.MaxValue + ".");
            }
        }

        void hostAddressTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (IPAddress.TryParse(hostAddressTextBox.Text, out hostAddress))
            {
                hostAddressValid = true;
                errorProvider.SetError(hostAddressTextBox, null);
            }
            else
            {
                hostAddressValid = false;
                errorProvider.SetError(hostAddressTextBox, "Invalid IP address.");
            }
        }

        void serverNameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (serverNameTextBox.Text.Length != 0)
            {
                serverNameValid = true;
                errorProvider.SetError(serverNameTextBox, null);
            }
            else
            {
                serverNameValid = false;
                errorProvider.SetError(serverNameTextBox, "Server name must be at least 1 character long.");
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void addServerButton_Click(object sender, EventArgs e)
        {
            if (serverNameValid && hostAddressValid && hostPortValid && usernameValid && passwordValid)
            {
                DetailsSubmitted(this, new AddServerFormDetailsSubmittedEventArgs(
                    this.serverNameTextBox.Text,
                    this.hostAddress,
                    this.hostPort,
                    this.usernameTextBox.Text,
                    this.passwordTextBox.Text));
            }
        }
    }
}
