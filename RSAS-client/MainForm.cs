using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RSAS.Networking;
using System.Net;
using System.Net.Sockets;

namespace RSAS.ClientSide
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //basic test
            TcpClient client = new TcpClient("127.0.0.1", 7070);
            Connection con = new Connection(client);
            List<string> temp = new List<string>();
            temp.Add("hello");
            temp.Add("there");

            List<string> temp2 = new List<string>();
            temp2.Add("don't forget");
            temp2.Add("about me!");
            con.SendMessage(new RSAS.Networking.Message("hello", temp));
            con.SendMessage(new RSAS.Networking.Message("hello", temp2));
        }
    }
}
