using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RSAS.Networking;
using RSAS.Networking.Messages;
using RSAS.Utilities;
using RSAS.Plugins.Frameworks;

namespace RSAS.ServerSide
{
    class UserAuthenticator
    {
        static List<Connection> unauthenticatedConnections = new List<Connection>();
        static Dictionary<string, User> users = new Dictionary<string, User>();

        public static void LoadCredentials()
        {
            //hashedUserCredentials.Add("username", SecurityUtilities.MD5Hash("password"));
            User testUser = User.CreateFromUsername("username", SecurityUtilities.MD5Hash("password"));
            users.Add("username", testUser);
        }

        static bool CheckCredentials(string username, string hashedPassword)
        {
            if (users.ContainsKey(username))
            {
                return (users[username].AuthenticationKey == hashedPassword);
            }
            else
            {
                return false;
            }
        }

        public static void AuthenticateConnection(object sender, ServerClientConnectedEventArgs e)
        {
            Connection con = e.Connection;
            UserAuthenticator.unauthenticatedConnections.Add(con);
            con.MessageReceived += CheckAuthenticationResponse;
            con.SendMessage(new AuthenticationRequest());
            
        }

        static void CheckAuthenticationResponse(object sender, ConnectionMessageReceivedEventArgs e)
        {
            Connection con = sender as Connection;
            if (e.Message.GetType() == typeof(AuthenticationResponse))
            {
                AuthenticationResponse message = e.Message as AuthenticationResponse;
                if (CheckCredentials(message.Username, message.Password))
                {
                    //unhook from future messages, work here is done
                    con.MessageReceived -= CheckAuthenticationResponse;
                    unauthenticatedConnections.Remove(con);

                    //let the client know the result was successful
                    con.SendMessage(new AuthenticationResult(true));

                    //associate new connection with user instance
                    users[message.Username].AssociateWithConnection(con);
                }
                else
                {
                    //let the client know the result was unsuccessful
                    con.SendMessage(new AuthenticationResult(false));

                    //disconnect
                    con.Disconnect();
                }
            }
        }
    }
}
