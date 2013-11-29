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
        ObservableCollection<Connection> cons = new ObservableCollection<Connection>();

        public Node(Connection con, PluginFramework framework)
        {
            cons.Add(con);

            framework.MergeWith(new Base());
            framework.MergeWith(new Plugins.Frameworks.Networking(cons));

            pluginLoader.LoadPlugins(Settings.PLUGINPATH, Settings.ENTRYSCRIPTNAME, framework);
        }
    }
}
