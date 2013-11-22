using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using RSAS.Plugins;
using RSAS.Plugins.Frameworks;
using RSAS.Networking;

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

        private User(string username, string authenticationKey)
        {
            this.username = username;
            this.authenticationKey = authenticationKey;

            //setup plugin frameworks
            this.pluginLoader = new PluginLoader();
            Base framework = new Base();
            framework.MergeWith(new RSAS.Plugins.Frameworks.Networking(connections));

            //load plugins from directory
            this.pluginLoader.LoadPlugins(Path.Combine(Settings.USERPATH, username, Settings.USERPLUGINPATH), Settings.ENTRYSCRIPTNAME, framework);
        }

        public static User CreateFromUsername(string username, string authenticationKey)
        {
            if (Directory.Exists(User.GetProfileDirectory(username)))
                return new User(username, authenticationKey);
            else
                return null;
        }

        public void AssociateWithConnection(Connection con)
        {
            this.connections.Add(con);
        }

        public string GetProfileDirectory()
        {
            return User.GetProfileDirectory(this.username);
        }

        public static string GetProfileDirectory(string username)
        {
            return Path.Combine(Settings.USERPATH, username);
        }
    }
}
