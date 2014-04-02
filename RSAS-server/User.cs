using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using RSAS.Plugins;
using RSAS.Plugins.Frameworks;
using RSAS.Networking;
using RSAS.Logging;

namespace RSAS.ServerSide
{
    class User
    {
        PluginLoader pluginLoader;
        string username;
        string authenticationKey;
        ObservableCollection<Connection> connections = new ObservableCollection<Connection>();

        public string Username { get{return this.username;} }
        public string AuthenticationKey { get { return this.authenticationKey; } }

        public User(string username, string authenticationKey)
        {
            this.username = username;
            this.authenticationKey = authenticationKey;

            //setup plugin frameworks
            this.pluginLoader = new PluginLoader();

            pluginLoader.ThreadSafeLua.ExecutionError += new ThreadSafeLuaErrorEventHandler(delegate(object sender, ThreadSafeLuaExecutionErrorEventArgs e)
            {
                TextLogger.TimestampedLog(LogType.Warning, "Lua error in " + e.Source + " @ line " + e.Line + ": " + e.Message);
            });

            Base framework = new Base();

            framework.MessagePrinted += new BaseMessagePrintedEventHandler(delegate(object sender, BaseMessagePrintedEventArgs e)
            {
                TextLogger.TimestampedLog(LogType.Information, e.Message);
            });

            framework.MergeWith(new RSAS.Plugins.Frameworks.Networking(connections));
            framework.MergeWith(new RSAS.Plugins.Frameworks.Timer());
            framework.MergeWith(new RSAS.Plugins.Frameworks.IO());

            //load plugins from directory
            this.pluginLoader.LoadPlugins(Settings.BuildUserPluginPath(username), Settings.ENTRYSCRIPTNAME, framework);
        }


        public void AssociateWithConnection(Connection con)
        {
            this.connections.Add(con);
        }
    }
}
