using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lua4Net;

namespace RSAS.Plugins
{
    public class PluginLoader
    {
        private Lua lua = new Lua();

        public void LoadPlugins(string pluginDirectory, string entryScriptName, PluginFramework framework)
        {
            lua.LoadStandardLibrary(LuaStandardLibrary.Base);
            lua.LoadStandardLibrary(LuaStandardLibrary.Table);
            lua.LoadStandardLibrary(LuaStandardLibrary.Math);

            if (framework != null)
                framework.InjectInto(lua);

            if (Directory.Exists(pluginDirectory))
            {
                string[] paths = Directory.GetDirectories(pluginDirectory);

                foreach (string path in paths)
                {
                    string entry = Path.Combine(path, entryScriptName);
                    if (File.Exists(entry))
                    {
                        lua.Execute(File.ReadAllText(entry));
                    }
                }
            }
        }
    }
}
