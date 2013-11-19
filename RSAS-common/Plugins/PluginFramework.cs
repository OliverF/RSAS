using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lua4Net;
using Lua4Net.Types;

namespace RSAS.Plugins
{
    public abstract class PluginFramework
    {
        protected Dictionary<string, LuaManagedFunctionHandler> luaFunctions = new Dictionary<string, LuaManagedFunctionHandler>();

        protected string frameworkScriptName = "";

        public void MergeWith(PluginFramework existingFramework)
        {
            this.luaFunctions.Concat(existingFramework.luaFunctions);
        }

        public static PluginFramework Merge(PluginFramework a, PluginFramework b)
        {
            a.MergeWith(b);

            return a;
        }

        public void InjectInto(Lua lua)
        {
            string frameworkScript = null;
            string frameworkScriptPath = Path.Combine(Settings.FRAMEWORKSCRIPTSPATH, this.frameworkScriptName);
            if (frameworkScriptName != "" && File.Exists(frameworkScriptPath))
                frameworkScript = File.ReadAllText(frameworkScriptPath);

            if (frameworkScript != null)
                lua.Execute(frameworkScript);

            foreach (KeyValuePair<string, LuaManagedFunctionHandler> function in luaFunctions)
            {
                lua.RegisterGlobalFunction(function.Key, function.Value);
            }
        }
    }
}
