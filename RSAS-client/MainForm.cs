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
        public MainForm()
        {
            InitializeComponent();
            //basic test
            TcpClient client = new TcpClient("127.0.0.1", 7070);
            con = new Connection(client);
            con.MessageReceived += new ConnectionMessageReceivedEventHandler(con_MessageReceived);

            ObservableCollection<Connection> connections = new ObservableCollection<Connection>();
            connections.Add(con);

            Plugins.Frameworks.Timer framework = new Plugins.Frameworks.Timer();
            framework.MergeWith(new GUIFramework(this.primaryDisplayPanel));
            framework.MergeWith(new Plugins.Frameworks.Base());
            framework.MergeWith(new Plugins.Frameworks.Networking(connections));

            Plugins.PluginLoader pluginLoader = new Plugins.PluginLoader();



            pluginLoader.LoadPlugins(Settings.PLUGINPATH, Settings.ENTRYSCRIPTNAME, framework);

            Node testNode = new Node(connections, pluginLoader);
        }

        void con_MessageReceived(object sender, ConnectionMessageReceivedEventArgs e)
        {
            if (e.Message.GetType() == typeof(AuthenticationRequest))
            {
                Connection con = sender as Connection;
                con.SendMessage(new AuthenticationResponse("username", SecurityUtilities.MD5Hash("password")));
            }
        }
    }
}
