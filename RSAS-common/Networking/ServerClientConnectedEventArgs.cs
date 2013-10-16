using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSAS.Networking
{
    public class ServerClientConnectedEventArgs
    {
        public Connection Connection { get; set; }

        public ServerClientConnectedEventArgs(Connection connection)
        {
            this.Connection = connection;
        }
    }
}
