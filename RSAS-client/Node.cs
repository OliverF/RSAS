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

        public string Name { get { return this.name; } }
        public PluginLoader PluginLoader { get { return this.pluginLoader; } }
        public Connection Connection { get { return this.connection; } }

        public Node(string name, Connection connection, PluginLoader pluginLoader)
        {
            this.name = name;
            this.connection = connection;
            this.pluginLoader = pluginLoader;
        }
    }
}
