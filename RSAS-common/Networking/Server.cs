using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Net.Sockets;


namespace RSAS.Networking
{
    public delegate void ClientConnectedHandler(object sender, ServerClientConnectedEventArgs e);

    public class Server
    {
        public event ClientConnectedHandler ClientConnected;

        TcpListener listener;
        Thread acceptConnections;
        volatile bool shouldAcceptConnections = false;

        public Server(IPEndPoint ip)
        {
            this.listener = new TcpListener(ip);
        }

        public void Start()
        {
            this.listener.Start();

            //start listening for new connections
            this.shouldAcceptConnections = true;
            this.acceptConnections = new Thread(AcceptNewConnections);
            acceptConnections.IsBackground = true;
            acceptConnections.Start();
        }

        public void Stop()
        {
            this.shouldAcceptConnections = false;
        }

        private void AcceptNewConnections()
        {
            while (this.shouldAcceptConnections)
            {
                TcpClient newClient = listener.AcceptTcpClient();
                if (ClientConnected != null && this.shouldAcceptConnections)
                    ClientConnected(this, new ServerClientConnectedEventArgs(new Connection(newClient)));
            }

            this.listener.Stop();
        }
    }
}
