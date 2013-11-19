using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lua4Net;

namespace RSAS.Plugins
{
    class Plugin
    {
        private Lua script = new Lua();

        public Plugin(string entryScriptPath) : this(entryScriptPath, null)
        {
        }

        public Plugin(string entryScriptPath, PluginFramework framework)
        {
            if (framework != null)
                framework.InjectInto(script);

            if (File.Exists(entryScriptPath))
                script.Execute(File.ReadAllText(entryScriptPath));
        }
    }
}
