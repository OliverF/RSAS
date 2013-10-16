using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using RSAS.Networking;

namespace RSAS.ServerSide
{
    class Program
    {
        static void Main(string[] args)
        {
            //basic test
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            Server test = new Server(new System.Net.IPEndPoint(ip, 7070));

            test.ClientConnected += new ClientConnectedHandler(test_ClientConnected);

            test.Start();

            Console.ReadLine();
            
        }

        static void test_ClientConnected(Connection newClient)
        {
            newClient.MessageReceived += new MessageReceivedHandler(newClient_MessageRecieved);
        }

        static void newClient_MessageRecieved(Message message)
        {
            Console.WriteLine(message);
        }
    }
}
