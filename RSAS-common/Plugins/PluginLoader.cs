using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RSAS.Plugins
{
    public class PluginLoader
    {
        private List<Plugin> plugins = new List<Plugin>();

        public void LoadPlugins(string pluginDirectory, string entryScriptName, PluginFramework framework)
        {
            if (Directory.Exists(pluginDirectory))
            {
                string[] paths = Directory.GetDirectories(pluginDirectory);

                foreach (string path in paths)
                {
                    string entry = Path.Combine(path, entryScriptName);
                    if (File.Exists(entry))
                    {
                        Plugin p = new Plugin(entry, framework);
                        plugins.Add(p);
                    }
                }
            }
        }
    }
}
