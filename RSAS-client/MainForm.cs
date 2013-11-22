﻿using System;
using System.Collections.Generic;
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
        public MainForm()
        {
            InitializeComponent();
            //basic test
            TcpClient client = new TcpClient("127.0.0.1", 7070);
            Connection con = new Connection(client);
            con.MessageReceived += new ConnectionMessageReceivedEventHandler(con_MessageReceived);

            Node testNode = new Node(con);
        }

        private void button1_Click(object sender, EventArgs e)
        {
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
