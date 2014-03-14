using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace RSAS.ClientSide
{
    public class AddServerFormDetailsSubmittedEventArgs : EventArgs
    {
        public string ServerName { get; set; }
        public IPAddress HostAddress { get; set; }
        public int HostPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public AddServerFormDetailsSubmittedEventArgs(string serverName, IPAddress hostAddress, int hostPort, string username, string password)
        {
            this.ServerName = serverName;
            this.HostAddress = hostAddress;
            this.HostPort = hostPort;
            this.Username = username;
            this.Password = password;
        }
    }
}
