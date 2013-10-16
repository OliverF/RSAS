using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSAS.Networking.Messages
{
    [Serializable]
    public class AuthenticationRequest : Message
    {
        public AuthenticationRequest()
        {

        }
    }

    [Serializable]
    public class AuthenticationResponse : Message
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public AuthenticationResponse(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }
    }

    [Serializable]
    public class AuthenticationResult : Message
    {
        public bool AuthenticationAccepted { get; set; }

        public AuthenticationResult(bool authenticationAccepted)
        {
            this.AuthenticationAccepted = authenticationAccepted;
        }
    }
}
