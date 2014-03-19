using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using RSAS.Networking;
using RSAS.Plugins;
using RSAS.Plugins.Frameworks;

namespace RSAS.ClientSide
{
    class Node
    {
        string name;
        PluginLoader pluginLoader;
        Connection connection;
        string username;
        string password;

        public string Name { get { return this.name; } }
        public PluginLoader PluginLoader { get { return this.pluginLoader; } }
        public Connection Connection { get { return this.connection; } }
        public string Username { get { return this.username; } }
        public string Password { get { return this.password; } }

        public Node(string name, Connection connection, string username, string password, PluginLoader pluginLoader)
        {
            this.name = name;
            this.connection = connection;
            this.username = username;
            this.password = password;
            this.pluginLoader = pluginLoader;
        }
    }
}
