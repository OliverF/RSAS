using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RSAS.Plugins;

namespace RSAS.ServerSide
{
    class User
    {
        PluginLoader pluginLoader;
        string username;

        public string Username { get{return this.username;} }

        private User(string username, PluginFramework pluginFramework)
        {
            this.username = username;
            this.pluginLoader = new PluginLoader();
            this.pluginLoader.LoadPlugins(Path.Combine(Settings.USERPATH, username, Settings.USERPLUGINPATH), Settings.ENTRYSCRIPTNAME, pluginFramework);
        }

        public static User CreateFromUsername(string username, PluginFramework pluginFramework)
        {
            if (Directory.Exists(User.GetProfileDirectory(username)))
                return new User(username, pluginFramework);
            else
                return null;
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
