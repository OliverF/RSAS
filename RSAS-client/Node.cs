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
        PluginLoader pluginLoader;
        ObservableCollection<Connection> connections;

        public Node(ObservableCollection<Connection> connections, PluginLoader pluginLoader)
        {
            this.connections = connections;
            this.pluginLoader = pluginLoader;
        }
    }
}
