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

        private User(string username)
        {
            this.username = username;
        }

        public static User CreateFromUsername(string username)
        {
            if (Directory.Exists(User.GetProfileDirectory(username)))
                return new User(username);
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
