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
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using RSAS.Networking;
using RSAS.Networking.Messages;
using RSAS.Utilities;
using RSAS.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RSAS.ClientSide
{
    public partial class MainForm : Form
    {
        delegate void ControlWork();

        List<Node> nodes = new List<Node>();

        public MainForm()
        {
            InitializeComponent();
            LoadServers();
        }

        private void addServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddServerForm addServerForm = new AddServerForm();
            addServerForm.Show();
            addServerForm.DetailsSubmitted += new AddServerForm.AddServerFormDetailsSubmittedEventHandler(addServerForm_DetailsSubmitted);
        }

        void addServerForm_DetailsSubmitted(object sender, AddServerFormDetailsSubmittedEventArgs e)
        {
            string remoteHost = e.HostAddress + ":" + e.HostPort.ToString();
            foreach (Node node in nodes)
            {
                if (node.Connection.RemoteEndPoint.ToString() == remoteHost)
                {
                    DialogResult result = MessageBox.Show("A server has already been defined with the same address.", "Input error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                    if (result == DialogResult.Cancel)
                        (sender as AddServerForm).Close();
                    return;
                }
            }

            TcpClient client;
            try
            {
                client = new TcpClient(e.HostAddress.ToString(), e.HostPort);
            }
            catch (SocketException)
            {
                string errorMessage = "Connection error: Could not connect to " + remoteHost + ".";
                DialogResult result = MessageBox.Show(errorMessage, "Connection error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                if (result == DialogResult.Cancel)
                    (sender as AddServerForm).Close();
                return;
            }

            SetupNodeConnection(client, e.ServerName, e.Username, e.Password);

            (sender as AddServerForm).Close();
        }

        void SaveServers()
        {
            if (!Directory.Exists(Directory.GetParent(Settings.SERVERDATAPATH).FullName))
                Directory.CreateDirectory(Directory.GetParent(Settings.SERVERDATAPATH).FullName);

            JObject root = new JObject();

            foreach (Node node in nodes)
            {
                JObject jNode = new JObject();
                jNode.Add("name", node.Name);
                jNode.Add("username", node.Username);
                byte[] protectedPasswordBytes = ProtectedData.Protect(Encoding.Unicode.GetBytes(node.Password), null, DataProtectionScope.CurrentUser);
                int[] protectedPassword = Array.ConvertAll(protectedPasswordBytes, b => (int)b);
                jNode.Add("password", new JArray(protectedPassword));
                root.Add(node.Connection.RemoteEndPoint, jNode);
            }

            StreamWriter sw = new StreamWriter(Settings.SERVERDATAPATH);
            sw.Write(root.ToString(Formatting.Indented, new JsonConverter[0]));
            sw.Close();
        }

        void LoadServers()
        {
            if (File.Exists(Settings.SERVERDATAPATH))
            {
                StreamReader sr = new StreamReader(Settings.SERVERDATAPATH);
                string json = sr.ReadToEnd();
                JObject nodes;
                sr.Close();
                try
                {
                    nodes = JObject.Parse(json);
                }
                catch(JsonReaderException)
                {
                    LogConsoleMessage(Settings.LogMessageType.Error, Settings.BuildServerLoadError());
                    return;
                }

                foreach (JProperty node in nodes.Children())
                {
                    //start handling server in new thread to allow GUI thread to continue
                    Thread t = new Thread(delegate()
                    {
                        JObject jNode = node.Value as JObject;
                        List<int> protectedPasswordList = new List<int>();

                        try
                        {
                            byte[] protectedPassword = Array.ConvertAll(jNode["password"].Value<JArray>().ToArray(), token => (byte)int.Parse((token as JValue).Value.ToString()));
                            byte[] passwordBytes = ProtectedData.Unprotect(protectedPassword, null, DataProtectionScope.CurrentUser);
                            string password = Encoding.Unicode.GetString(passwordBytes);
                            string username = jNode["username"].Value<JValue>().Value.ToString();
                            string serverName = jNode["name"].Value<JValue>().Value.ToString();
                            string[] remoteEndPoint = node.Name.Split(':');

                            TcpClient client;
                            try
                            {
                                client = new TcpClient(remoteEndPoint[0], int.Parse(remoteEndPoint[1]));
                                SetupNodeConnection(client, serverName, username, password);
                            }
                            catch (SocketException)
                            {
                                LogConsoleMessage(Settings.LogMessageType.Warning, Settings.BuildConnectionErrorMessage(serverName, node.Name));
                            }
                        }
                        catch (Exception)
                        {
                            //No specific error handling can be made here
                            LogConsoleMessage(Settings.LogMessageType.Error, Settings.BuildServerLoadError());
                            return;
                        }
                    });
                    t.Start();
                }
            }
        }

        void SetupNodeConnection(TcpClient client, string serverName, string username, string password)
        {
            Connection con = new Connection(client);
            con.MessageReceived += new ConnectionMessageReceivedEventHandler(delegate(object connectionSender, ConnectionMessageReceivedEventArgs connectionMessageArgs)
            {
                if (connectionMessageArgs.Message.GetType() == typeof(AuthenticationRequest))
                {
                    (connectionSender as Connection).SendMessage(new AuthenticationResponse(username, SecurityUtilities.MD5Hash(password)));
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

                        pluginLoader.ThreadSafeLua.ExecutionError += new ThreadSafeLuaErrorEventHandler(delegate(object sender, ThreadSafeLuaExecutionErrorEventArgs e)
                        {
                            LogConsoleMessage(Settings.LogMessageType.Warning, Settings.BuildLuaErrorMessage(e.Source, e.Line.ToString(), e.Message));
                        });

                        pluginLoader.LoadPlugins(Settings.PLUGINPATH, Settings.ENTRYSCRIPTNAME, framework);

                        nodes.Add(new Node(serverName, con, username, password, pluginLoader));

                        SaveServers();
                    }
                    else
                    {
                        LogConsoleMessage(Settings.LogMessageType.Warning, Settings.BuildBadCredentialsMessage(serverName));
                    }
                }
            });
        }

        void LogConsoleMessage(Settings.LogMessageType type, string message)
        {
            string logMessage = "[" + type.ToString() + "] " + message + " " + System.DateTime.Now.ToString("H:m:s dd/MM/yyyy") + Environment.NewLine;

            ControlWork work = delegate()
            {
                this.consoleLogTextBox.AppendText(logMessage);
            };

            if (this.consoleLogTextBox.InvokeRequired)
                this.consoleLogTextBox.Invoke(work);
            else
                work();
        }

        private void showLogConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.splitContainer.Panel2Collapsed = !showLogConsoleToolStripMenuItem.Checked;
        }
    }
}
