using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RSAS.ServerSide
{
    static class Settings
    {
        public const string USERPATH = "users";
        public const string ENTRYSCRIPTNAME = "sv_entry.lua";
        public const string USERPLUGINPATH = "plugins";
        public const string USERCREDENTIALSFILENAME = "access.hash";

        public static string BuildUserPluginPath(string username)
        {
            return Path.Combine(USERPATH, username, USERPLUGINPATH);
        }

        public static string BuildUserCredentialsFilePath(string username)
        {
            return Path.Combine(USERPATH, username, USERCREDENTIALSFILENAME);
        }

        public static string BuildUserPath(string username)
        {
            return Path.Combine(USERPATH, username);
        }
    }
}
