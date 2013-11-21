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
        protected delegate void RegisterPluginFrameworkHandler(Lua lua);

        protected List<RegisterPluginFrameworkHandler> registerEvents = new List<RegisterPluginFrameworkHandler>();

        protected List<string> frameworkScriptNames = new List<string>();

        public void MergeWith(PluginFramework existingFramework)
        {
            this.registerEvents = this.registerEvents.Concat(existingFramework.registerEvents).ToList();
            this.frameworkScriptNames = this.frameworkScriptNames.Concat(existingFramework.frameworkScriptNames).ToList();
        }

        public static PluginFramework Merge(PluginFramework a, PluginFramework b)
        {
            a.MergeWith(b);

            return a;
        }

        public void InjectInto(Lua lua)
        {
            foreach (string frameworkScriptName in this.frameworkScriptNames)
            {
                string frameworkScript = null;
                string frameworkScriptPath = Path.Combine(Settings.FRAMEWORKSCRIPTSPATH, frameworkScriptName);
                if (frameworkScriptName != "" && File.Exists(frameworkScriptPath))
                    frameworkScript = File.ReadAllText(frameworkScriptPath);

                if (frameworkScript != null)
                    lua.Execute(frameworkScript);
            }

            foreach (RegisterPluginFrameworkHandler callback in registerEvents)
            {
                callback(lua);
            }
        }
    }
}
