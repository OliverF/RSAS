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
        PluginLoader pluginLoader = new PluginLoader();

        public Node(Connection con)
        {
            ObservableCollection<Connection> cons = new ObservableCollection<Connection>();
            cons.Add(con);

            Base framework = new Base();
            framework.MergeWith(new Plugins.Frameworks.Networking(cons));

            pluginLoader.LoadPlugins(Settings.PLUGINPATH, Settings.ENTRYSCRIPTNAME, framework);
        }
    }
}
