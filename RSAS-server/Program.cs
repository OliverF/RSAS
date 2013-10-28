using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using RSAS.Networking;
using RSAS.Networking.Messages;

namespace RSAS.ServerSide
{
    class Program
    {
        static void Main(string[] args)
        {
            //basic test
            UserAuthenticator.LoadCredentials();

            IPAddress ip = IPAddress.Any;
            Server server = new Server(new System.Net.IPEndPoint(ip, 7070));

            server.ClientConnected += UserAuthenticator.AuthenticateConnection;

            server.Start();

            while (!Regex.IsMatch(Console.ReadLine(), @"\A(exit|quit|q)\Z")) ;

            server.Stop();

        }
    }
}
