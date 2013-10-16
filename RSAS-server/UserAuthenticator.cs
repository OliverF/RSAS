using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RSAS.Networking;
using RSAS.Networking.Messages;

namespace RSAS.ServerSide
{
    class UserAuthenticator
    {
        static List<Connection> unauthenticatedConnections = new List<Connection>();

        public static void AuthenticateConnection(object sender, ServerClientConnectedEventArgs e)
        {
            Connection con = e.Connection;
            UserAuthenticator.unauthenticatedConnections.Add(con);
            con.MessageReceived += CheckAuthenticationResponse;
            con.SendMessage(new AuthenticationRequest());
            
        }

        static void CheckAuthenticationResponse(object sender, ConnectionMessageReceivedEventArgs e)
        {
            if (e.Message.GetType() == typeof(AuthenticationResponse))
            {
                AuthenticationResponse message = e.Message as AuthenticationResponse;
            }
        }
    }
}
