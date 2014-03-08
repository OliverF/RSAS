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
        Lua lua;
        ThreadSafeLua safeLua;

        public ThreadSafeLua ThreadSafeLua { get { return safeLua; } }

        public PluginLoader()
        {
            this.lua = new Lua();
            this.safeLua = new ThreadSafeLua(lua);
        }
        
        public void LoadPlugins(string pluginDirectory, string entryScriptName, PluginFramework framework)
        {
            lua.LoadStandardLibrary(LuaStandardLibrary.Base);
            lua.LoadStandardLibrary(LuaStandardLibrary.Table);
            lua.LoadStandardLibrary(LuaStandardLibrary.Math);

            if (framework != null)
                framework.InjectInto(safeLua);

            if (Directory.Exists(pluginDirectory))
            {
                string[] paths = Directory.GetDirectories(pluginDirectory);

                foreach (string path in paths)
                {
                    string entry = Path.Combine(path, entryScriptName);
                    if (File.Exists(entry))
                    {
                        safeLua.Execute(File.ReadAllText(entry), entry);
                    }
                }
            }
        }
    }
}
