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
using RSAS.Utilities;

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

            string consoleInput = Console.ReadLine();
            while (!Regex.IsMatch(consoleInput, @"\A(exit|quit|q)\Z"))
            {
                HandleConsoleInput(consoleInput);
                consoleInput = Console.ReadLine();
            }

            server.Stop();

        }

        static void TextLogger_MessageLogged(object sender, TextLoggerMessageLoggedEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        static void HandleConsoleInput(string input)
        {
            string[] consoleArgs = input.Split(' ');

            if (consoleArgs.Count() < 1)
                return;

            if (consoleArgs[0] == "adduser" && consoleArgs.Length == 3)
            {
                try
                {
                    UserAuthenticator.CreateCredentials(Settings.USERPATH, consoleArgs[1], consoleArgs[2]);
                    TextLogger.TimestampedLog(LogType.Information, "Added user " + consoleArgs[1]);
                }
                catch (Exception e)
                {
                    //No specific error handling can be performed, log the error
                    TextLogger.TimestampedLog(LogType.Error, e.ToString());
                }
            }
            else if(consoleArgs[0] == "moduser" && consoleArgs.Length == 3)
            {
                try
                {
                    UserAuthenticator.ModifyCredentials(Settings.USERPATH, consoleArgs[1], consoleArgs[2]);
                    TextLogger.TimestampedLog(LogType.Information, "Modified credentials for user " + consoleArgs[1]);
                }
                catch(Exception e)
                {
                    TextLogger.TimestampedLog(LogType.Error, e.ToString());
                }
            }
            else if(consoleArgs[0] == "deluser" && consoleArgs.Length == 2)
            {
                try
                {
                    UserAuthenticator.DeleteCredentials(Settings.USERPATH, consoleArgs[1]);
                    TextLogger.TimestampedLog(LogType.Information, "Deleted user " + consoleArgs[1]);
                }
                catch (Exception e)
                {
                    TextLogger.TimestampedLog(LogType.Error, e.ToString());
                }
            }
        }
    }
}
