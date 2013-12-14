using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace RSAS.Networking
{
    public delegate void ConnectionMessageReceivedEventHandler(object sender, ConnectionMessageReceivedEventArgs e);

    public class Connection
    {
        private IFormatter formatter;

        protected TcpClient client;
        protected NetworkStream stream;

        public event ConnectionMessageReceivedEventHandler MessageReceived;
        public event EventHandler ConnectionClosed;

        public bool Connected { get { return this.client.Connected; } }

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
            if (!this.client.Connected)
                throw new System.InvalidOperationException("Stream is not connected.");
            try
            {
                formatter.Serialize(this.stream, message);
            }
            catch (Exception e)
            {
                if (e is IOException || e is ArgumentException)
                    this.Disconnect();
            }
        }

        public void Disconnect()
        {
            this.client.Close();
        }

        void CheckData()
        {
            while (this.client.Connected)
            {
                if (this.stream.DataAvailable)
                {
                    Message message = formatter.Deserialize(this.stream) as Message;
                    if (this.MessageReceived != null)
                        MessageReceived(this,  new ConnectionMessageReceivedEventArgs(message));
                }
                Thread.Sleep(10);
            }

            if (this.ConnectionClosed != null)
            {
                ConnectionClosed(this, new EventArgs());
            }
        }
    }
}
