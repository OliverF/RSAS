using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace RSAS.Networking
{
    public delegate void ConnectionMessageReceivedEventHandler(object sender, ConnectionMessageReceivedEventArgs e);

    public class Connection
    {
        private IFormatter formatter;

        protected TcpClient client;
        protected NetworkStream stream;

        public TcpClient Client
        {
            get { return this.client; }
        }

        public NetworkStream Stream
        {
            get { return this.stream; }
        }

        public event ConnectionMessageReceivedEventHandler MessageReceived;
        public event EventHandler ConnectionClosed;

        public Connection(TcpClient client)
        {
            this.formatter = new BinaryFormatter();

            this.client = client;
            this.stream = client.GetStream();

            Thread checkForData = new Thread(CheckData);
            checkForData.IsBackground = true;
            checkForData.Start();
        }

        public void SendMessage(Message message)
        {
            formatter.Serialize(this.stream, message);
        }

        void CheckData()
        {
            while (true)
            {
                if (!this.client.Connected)
                {
                    if (this.ConnectionClosed != null)
                    {
                        ConnectionClosed(this, new EventArgs());
                    }
                }
                if (this.stream.DataAvailable)
                {
                    Message message = formatter.Deserialize(this.stream) as Message;
                    if (this.MessageReceived != null)
                        MessageReceived(this,  new ConnectionMessageReceivedEventArgs(message));
                }
                Thread.Sleep(10);
            }
        }
    }
}
