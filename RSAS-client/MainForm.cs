using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using RSAS.Networking;
using RSAS.Networking.Messages;
using RSAS.Utilities;

namespace RSAS.ClientSide
{
    public partial class MainForm : Form
    {
        Connection con;
        List<Node> nodes = new List<Node>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void addServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddServerForm addServerForm = new AddServerForm();
            addServerForm.Show();
            addServerForm.DetailsSubmitted += new AddServerForm.AddServerFormDetailsSubmittedEventHandler(addServerForm_DetailsSubmitted);
        }

        void addServerForm_DetailsSubmitted(object sender, AddServerFormDetailsSubmittedEventArgs e)
        {
            TcpClient client;
            try
            {
                client = new TcpClient(e.HostAddress.ToString(), e.HostPort);
            }
            catch (SocketException)
            {
                string remoteHost = e.HostAddress.ToString() + ":" + e.HostPort;
                string errorMessage = "Connection error: Could not connect to " + remoteHost + ".";
                DialogResult result = MessageBox.Show(errorMessage, "Connection error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                if (result == DialogResult.Cancel)
                    (sender as AddServerForm).Close();
                return;
            }

            con = new Connection(client);
            con.MessageReceived += new ConnectionMessageReceivedEventHandler(delegate(object connectionSender, ConnectionMessageReceivedEventArgs connectionMessageArgs)
            {
                if (connectionMessageArgs.Message.GetType() == typeof(AuthenticationRequest))
                {
                    (connectionSender as Connection).SendMessage(new AuthenticationResponse(e.Username, SecurityUtilities.MD5Hash(e.Password)));
                }
                else if (connectionMessageArgs.Message.GetType() == typeof(AuthenticationResult))
                {
                    AuthenticationResult authenticationResult = connectionMessageArgs.Message as AuthenticationResult;
                    if (authenticationResult.AuthenticationAccepted)
                    {
                        ObservableCollection<Connection> connections = new ObservableCollection<Connection>();
                        connections.Add(con);

                        Plugins.Frameworks.Timer framework = new Plugins.Frameworks.Timer();
                        framework.MergeWith(new GUIFramework(this.primaryDisplayPanel));
                        framework.MergeWith(new Plugins.Frameworks.Base());
                        framework.MergeWith(new Plugins.Frameworks.Networking(connections));

                        Plugins.PluginLoader pluginLoader = new Plugins.PluginLoader();

                        pluginLoader.LoadPlugins(Settings.PLUGINPATH, Settings.ENTRYSCRIPTNAME, framework);

                        nodes.Add(new Node(e.ServerName, con, pluginLoader));
                    }
                }
            });

            (sender as AddServerForm).Close();
        }
    }
}
