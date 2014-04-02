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
using RSAS.Logging;
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
            LoadNodes();
            TextLogger.MessageLogged += new TextLoggerMessageLoggedEventHandler(delegate(object sender, TextLoggerMessageLoggedEventArgs e)
            {
                ControlWork work = delegate()
                {
                    this.consoleLogTextBox.AppendText(e.Message);
                };

                if (this.consoleLogTextBox.InvokeRequired)
                    this.consoleLogTextBox.Invoke(work);
                else
                    work();
            });
        }

        private void showLogConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.splitContainer.Panel2Collapsed = !showLogConsoleToolStripMenuItem.Checked;
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
            if (ServerAlreadyDefined(remoteHost))
            {
                DialogResult result = MessageBox.Show("A server has already been defined with the same address.", "Input error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                if (result == DialogResult.Cancel)
                    (sender as AddServerForm).Close();
                return;
            }

            try
            {
                AddNode(e.HostAddress, e.HostPort, e.ServerName, e.Username, e.Password);
            }
            catch (ServerUnreachableException unreachableException)
            {
                DialogResult result = MessageBox.Show(unreachableException.Message, "Connection error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                if (result == DialogResult.Cancel)
                    (sender as AddServerForm).Close();
                return;
            }
            catch (ServerBadCredentialsException badCredentialsException)
            {
                DialogResult result = MessageBox.Show(badCredentialsException.Message, "Connection error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                if (result == DialogResult.Cancel)
                    (sender as AddServerForm).Close();
                return;
            }

            (sender as AddServerForm).Close();
        }

        void SaveNodes()
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

        void LoadNodes()
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
                    TextLogger.TimestampedLog(LogType.Error, Settings.BuildServerLoadError());
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
                                AddNode(IPAddress.Parse(remoteEndPoint[0]), int.Parse(remoteEndPoint[1]), serverName, username, password);
                            }
                            catch (SocketException)
                            {
                                TextLogger.TimestampedLog(LogType.Warning, Settings.BuildConnectionErrorMessage(serverName, node.Name));
                            }
                        }
                        catch (Exception)
                        {
                            //No specific error handling can be made here
                            TextLogger.TimestampedLog(LogType.Error, Settings.BuildServerLoadError());
                            return;
                        }
                    });
                    t.Start();
                }
            }
        }

        bool ServerAlreadyDefined(string remoteEndPoint)
        {
            foreach (Node node in nodes)
                if (node.Connection.RemoteEndPoint == remoteEndPoint)
                    return true;
            return false;
        }

        void AddNode(IPAddress hostAddress, int hostPort, string name, string username, string password)
        {
            string remoteHost = hostAddress.ToString() + ":" + hostPort.ToString();
            TcpClient client;
            try
            {
                client = new TcpClient(hostAddress.ToString(), hostPort);
            }
            catch (SocketException e)
            {
                throw new ServerUnreachableException("Connection error: Could not connect to " + remoteHost + ".", e);
            }

            Connection con;
            try
            {
                con = SetupNodeConnection(client, name, username, password);
            }
            catch (ServerBadCredentialsException)
            {
                //cannot handle here, throw higher
                throw;
            }

            PluginLoader pluginLoader = SetupNodePluginLoader(con, this.primaryDisplayPanel);

            Node node = new Node(name, con, username, password, pluginLoader);
            nodes.Add(node);

            ControlWork work = delegate()
            {
                serversToolStripMenuItem.DropDownItems.Add(SetupNodeMenuItem(node));
            };

            if (this.InvokeRequired)
                this.Invoke(work);
            else
                work();

            SaveNodes();
        }

        void RemoveNode(Node node, ToolStripMenuItem nodeMenuItem)
        {
            //remove this server from the list
            nodeMenuItem.OwnerItem.Owner = null;
            node.Connection.Disconnect();
            nodes.Remove(node);
            SaveNodes();
        }

        PluginLoader SetupNodePluginLoader(Connection networkingFrameworkConnection, Control guiFrameworkParent)
        {
            ObservableCollection<Connection> connections = new ObservableCollection<Connection>();
            connections.Add(networkingFrameworkConnection);

            Plugins.Frameworks.Base baseFramework = new Plugins.Frameworks.Base();
            baseFramework.MessagePrinted += new Plugins.Frameworks.BaseMessagePrintedEventHandler(delegate(object sender, Plugins.Frameworks.BaseMessagePrintedEventArgs e)
            {
                TextLogger.TimestampedLog(LogType.Information, e.Message);
            });
            baseFramework.MergeWith(new GUIFramework(guiFrameworkParent));
            baseFramework.MergeWith(new Plugins.Frameworks.Timer());
            baseFramework.MergeWith(new Plugins.Frameworks.Networking(connections));
            baseFramework.MergeWith(new Plugins.Frameworks.IO());

            Plugins.PluginLoader pluginLoader = new Plugins.PluginLoader();

            pluginLoader.ThreadSafeLua.ExecutionError += new ThreadSafeLuaErrorEventHandler(delegate(object sender, ThreadSafeLuaExecutionErrorEventArgs e)
            {
                TextLogger.TimestampedLog(LogType.Warning, Settings.BuildLuaErrorMessage(e.Source, e.Line.ToString(), e.Message));
            });

            pluginLoader.LoadPlugins(Settings.PLUGINPATH, Settings.ENTRYSCRIPTNAME, baseFramework);

            return pluginLoader;
        }

        Connection SetupNodeConnection(TcpClient client, string serverName, string username, string password)
        {
            bool timedOut = false;
            bool authed = false;
            bool authFail = false;
            System.Timers.Timer authTimeout = new System.Timers.Timer();
            authTimeout.Interval = 5000;
            authTimeout.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs e)
            {
                timedOut = true;
                authTimeout.Stop();
            });
            authTimeout.Start();

            Connection con = new Connection(client);
            con.MessageReceived += new ConnectionMessageReceivedEventHandler(delegate(object connectionSender, ConnectionMessageReceivedEventArgs connectionMessageArgs)
            {
                if (connectionMessageArgs.Message.GetType() == typeof(AuthenticationRequest))
                {
                    (connectionSender as Connection).SendMessage(new AuthenticationResponse(username, SecurityUtilities.MD5Hash(password)));
                }
                else if (connectionMessageArgs.Message.GetType() == typeof(AuthenticationResult))
                {
                    authTimeout.Stop();
                    AuthenticationResult authenticationResult = connectionMessageArgs.Message as AuthenticationResult;
                    authed = authenticationResult.AuthenticationAccepted;
                    authFail = !authed;
                }
            });

            //wait for the auth result
            while (!timedOut)
                if (authed)
                    return con;
                else if (authFail)
                    throw new ServerBadCredentialsException("'" + serverName + "' rejected credentials.");

            //throw exception in the event of a timeout
            throw new ServerUnreachableException("Connection error: Could not authenticate to " + serverName + ".");
        }

        ToolStripMenuItem SetupNodeMenuItem(Node node)
        {
            ToolStripMenuItem nodeMenuItem = new ToolStripMenuItem(node.Name);
            nodeMenuItem.Tag = node;

            ToolStripMenuItem nodeEditMenuItem = new ToolStripMenuItem("Edit...");
            nodeEditMenuItem.Tag = node;
            nodeEditMenuItem.Click += new EventHandler(nodeEditMenuItem_Click);

            ToolStripMenuItem nodeDeleteMenuItem = new ToolStripMenuItem("Delete...");
            nodeDeleteMenuItem.Tag = node;
            nodeDeleteMenuItem.Click += new EventHandler(nodeDeleteMenuItem_Click);

            nodeMenuItem.DropDownItems.AddRange(new ToolStripItem[] { nodeEditMenuItem, nodeDeleteMenuItem });

            return nodeMenuItem;
        }

        void nodeEditMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            Node node = item.Tag as Node;

            string[] endPoint = node.Connection.RemoteEndPoint.Split(':');

            AddServerForm addServerForm = new AddServerForm(node.Name, endPoint[0], endPoint[1], node.Username, node.Password);
            addServerForm.DetailsSubmitted += new AddServerForm.AddServerFormDetailsSubmittedEventHandler(delegate(object addServerSender, AddServerFormDetailsSubmittedEventArgs addServerArgs)
            {
                RemoveNode(node, item);
                //handle as usual
                addServerForm_DetailsSubmitted(addServerSender, addServerArgs);
            });
            addServerForm.Show();
        }

        void nodeDeleteMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            Node node = item.Tag as Node;
            DialogResult result = MessageBox.Show("You are about to delete a server configuration. The connection will be terminated (if connected) and any data associated will be deleted.",
                "Delete Server",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                RemoveNode(node, item);
            }
        }
    }
}
