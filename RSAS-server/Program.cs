using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using RSAS.Networking;
using RSAS.Networking.Messages;

namespace RSAS.ServerSide
{
    class Program
    {
        static void Main(string[] args)
        {
            //basic test
            IPAddress ip = IPAddress.Any;
            Server test = new Server(new System.Net.IPEndPoint(ip, 7070));

            test.ClientConnected += UserAuthenticator.AuthenticateConnection;

            test.Start();

            Console.ReadLine();
            
        }
    }
}
