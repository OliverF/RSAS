using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RSAS.ClientSide
{
    class Settings
    {
        public static string ENTRYSCRIPTNAME = "cl_entry.lua";
        public static string PLUGINPATH = "plugins";
        public static string APPDATAPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RSAS");
        public static string COREDATAPATH = Path.Combine(APPDATAPATH, "core");
        public static string FRAMEWORKDATAPATH = Path.Combine(APPDATAPATH, "frameworks");
        public static string SERVERDATAPATH = Path.Combine(COREDATAPATH, "servers.json");

        static string BADCREDENTIALSMESSAGE = "Server '{servername}' refused login credentials";
        static string CONNECTIONERRORMESSAGE = "Could not connect to server '{servername}' at {remoteendpoint}";
        static string LUAERRORMESSAGE = "Lua error in '{script}' @ line {line}: {error}";
        static string SERVERLOADERRORMESSAGE = "Error loading server definitions in " + SERVERDATAPATH;

        public static string BuildBadCredentialsMessage(string serverName)
        {
            return BADCREDENTIALSMESSAGE.Replace("{servername}", serverName);
        }

        public static string BuildConnectionErrorMessage(string serverName, string remoteEndPoint)
        {
            return CONNECTIONERRORMESSAGE.Replace("{servername}", serverName).Replace("{remoteendpoint}", remoteEndPoint);
        }

        public static string BuildLuaErrorMessage(string script, string line, string error)
        {
            return LUAERRORMESSAGE.Replace("{script}", script).Replace("{line}", line).Replace("{error}", error);
        }

        public static string BuildServerLoadError()
        {
            return SERVERLOADERRORMESSAGE;
        }
    }
}
