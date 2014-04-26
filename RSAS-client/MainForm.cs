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

    struct NodeMetaData
    {
        public ToolStripMenuItem MenuItem;
        public TabPage TabPage;

        public NodeMetaData(ToolStripMenuItem menuItem, TabPage tabPage)
        {
            this.MenuItem = menuItem;
            this.TabPage = tabPage;
        }
    }

    public partial class MainForm : Form
    {
        delegate void ControlWork();

        List<Node> nodes = new List<Node>();
        Dictionary<Node, NodeMetaData> nodeMetaData = new Dictionary<Node, NodeMetaData>();

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
            
            AddNode(e.HostAddress, e.HostPort, e.ServerName, e.Username, e.Password);

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
                root.Add(node.RemoteEndPoint, jNode);
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
                    Thread t = new Thread(delegate(object assignedNode)
                    {
                        JObject jNodeObject = (assignedNode as JProperty).Value as JObject;
                        string jNodePropertyKey = (assignedNode as JProperty).Name;
                        List<int> protectedPasswordList = new List<int>();

                        try
                        {
                            byte[] protectedPassword = Array.ConvertAll(jNodeObject["password"].Value<JArray>().ToArray(), token => (byte)int.Parse((token as JValue).Value.ToString()));
                            byte[] passwordBytes = ProtectedData.Unprotect(protectedPassword, null, DataProtectionScope.CurrentUser);
                            string password = Encoding.Unicode.GetString(passwordBytes);
                            string username = jNodeObject["username"].Value<JValue>().Value.ToString();
                            string serverName = jNodeObject["name"].Value<JValue>().Value.ToString();
                            string[] remoteEndPoint = jNodePropertyKey.Split(':');

                            try
                            {
                                AddNode(IPAddress.Parse(remoteEndPoint[0]), int.Parse(remoteEndPoint[1]), serverName, username, password);
                            }
                            catch (SocketException)
                            {
                                TextLogger.TimestampedLog(LogType.Warning, Settings.BuildConnectionErrorMessage(serverName, jNodePropertyKey));
                            }
                        }
                        catch (Exception)
                        {
                            //No specific error handling can be made here
                            TextLogger.TimestampedLog(LogType.Error, Settings.BuildServerLoadError());
                            return;
                        }
                    });
                    t.Start(node);
                }
            }
        }

        bool ServerAlreadyDefined(string remoteEndPoint)
        {
            foreach (Node node in nodes)
                if (node.RemoteEndPoint == remoteEndPoint)
                    return true;
            return false;
        }

        void AddNode(IPAddress hostAddress, int hostPort, string name, string username, string password)
        {
            string remoteHost = hostAddress.ToString() + ":" + hostPort.ToString();
            TcpClient client = new TcpClient();
            try
            {
                client.Connect(hostAddress.ToString(), hostPort);
            }
            catch (SocketException e)
            {
                TextLogger.TimestampedLog(LogType.Warning, "Connection error: Could not connect to " + remoteHost + " (" + e.Message + ").");
            }

            Connection con = null;
            try
            {
                if (client.Connected)
                {
                    con = SetupNodeConnection(client, name, username, password);
                }
            }
            catch (ServerBadCredentialsException e)
            {
                TextLogger.TimestampedLog(LogType.Warning, "Connection error: Could not connect to " + remoteHost + " (" + e.Message + ").");
            }

            TabPage tabPage = new TabPage(name);

            PluginLoader pluginLoader = SetupNodePluginLoader(con, tabPage);

            Node node = new Node(name, con, remoteHost, username, password, pluginLoader);
            nodes.Add(node);

            ToolStripMenuItem menuItem = SetupNodeMenuItem(node);

            nodeMetaData.Add(node, new NodeMetaData(menuItem, tabPage));

            ControlWork work = delegate()
            {
                serversToolStripMenuItem.DropDownItems.Add(menuItem);
                nodeTabControl.TabPages.Add(tabPage);
            };

            if (this.InvokeRequired)
                this.Invoke(work);
            else
                work();

            SaveNodes();
        }

        void RemoveNode(Node node)
        {
            if (nodeMetaData.ContainsKey(node))
            {
                //remove this server from the menu list
                nodeMetaData[node].MenuItem.Owner = null;
                //remove the server's tab page
                nodeTabControl.TabPages.Remove(nodeMetaData[node].TabPage);
            }
            if (node.Connection != null)
                node.Connection.Disconnect();
            nodes.Remove(node);
            nodeMetaData.Remove(node);
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
                    (connectionSender as Connection).SendMessage(new AuthenticationResponse(username, password));
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

            ToolStripMenuItem nodeReconnectMenuItem = new ToolStripMenuItem("Reconnect");
            nodeReconnectMenuItem.Tag = node;
            nodeReconnectMenuItem.Click += new EventHandler(nodeReconnectMenuItem_Click);

            nodeMenuItem.DropDownItems.AddRange(new ToolStripItem[] { nodeEditMenuItem, nodeDeleteMenuItem, nodeReconnectMenuItem });

            return nodeMenuItem;
        }

        void nodeReconnectMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            Node node = item.Tag as Node;
            string[] endPoint = node.RemoteEndPoint.Split(':');
            RemoveNode(node);
            AddNode(IPAddress.Parse(endPoint[0]), int.Parse(endPoint[1]), node.Name, node.Username, node.Password);
        }

        TabPage SetupNodeTabPage(Node node)
        {
            TabPage page = new TabPage(node.Name);
            return page;
        }

        void nodeEditMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            Node node = item.Tag as Node;

            string[] endPoint = node.RemoteEndPoint.Split(':');

            AddServerForm addServerForm = new AddServerForm(node.Name, endPoint[0], endPoint[1], node.Username, node.Password);
            addServerForm.DetailsSubmitted += new AddServerForm.AddServerFormDetailsSubmittedEventHandler(delegate(object addServerSender, AddServerFormDetailsSubmittedEventArgs addServerArgs)
            {
                RemoveNode(node);
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
                RemoveNode(node);
            }
        }
    }
}
