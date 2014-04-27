using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading;

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

        public AddServerForm(string name, string hostAddress, string hostPort, string username, string password):this()
        {
            this.serverNameTextBox.Text = name;
            this.hostAddressTextBox.Text = hostAddress;
            this.hostPortTextBox.Text = hostPort;
            this.usernameTextBox.Text = username;
            this.passwordTextBox.Text = password;

            //force the validation of the textboxes
            this.ValidateChildren();
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
                errorProvider.SetError(hostAddressTextBox, "Performing DNS lookup...");

                Thread t = new Thread(delegate()
                {
                    IPAddress[] addresses;

                    try
                    {
                        addresses = Dns.GetHostAddresses(hostAddressTextBox.Text);
                    }
                    catch (System.Net.Sockets.SocketException socketException)
                    {
                        hostAddressValid = false;
                        MethodInvoker errorWork = delegate()
                        {
                            errorProvider.SetError(hostAddressTextBox, "Invalid hostname: " + socketException.Message);
                        };

                        if (this.InvokeRequired)
                            this.Invoke(errorWork);
                        else
                            errorWork();
                        return;
                    }

                    hostAddress = addresses[0];
                    hostAddressValid = true;

                    MethodInvoker work = delegate()
                    {
                        hostAddressTextBox.Text = addresses[0].ToString();
                        errorProvider.SetError(hostAddressTextBox, null);
                    };

                    if (hostAddressTextBox.InvokeRequired)
                        hostAddressTextBox.Invoke(work);
                    else
                        work();

                });

                t.Start();
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
            if (serverNameValid && hostAddressValid && hostPortValid && usernameValid && passwordValid && DetailsSubmitted != null)
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
