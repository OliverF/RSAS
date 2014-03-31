using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using RSAS.Networking;
using RSAS.Networking.Messages;
using RSAS.Logging;

namespace RSAS.ServerSide
{
    class Program
    {
        static void Main(string[] args)
        {
            TextLogger.MessageLogged += new TextLoggerMessageLoggedEventHandler(TextLogger_MessageLogged);

            try
            {
                UserAuthenticator.LoadCredentials(Settings.USERPATH);
            }
            catch (Exception e)
            {
                //No specific error handling can be performed, log the error
                TextLogger.TimestampedLog(LogType.Error, e.ToString());
            }

            IPAddress ip = IPAddress.Any;
            Server server = new Server(new System.Net.IPEndPoint(ip, 7070));

            server.ClientConnected += UserAuthenticator.AuthenticateConnection;

            server.Start();

            while (!Regex.IsMatch(Console.ReadLine(), @"\A(exit|quit|q)\Z")) ;

            server.Stop();

        }

        static void TextLogger_MessageLogged(object sender, TextLoggerMessageLoggedEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
