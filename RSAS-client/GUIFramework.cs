using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization.Charting.Data;
using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
using Lua4Net;
using RSAS.Plugins;

namespace RSAS.ClientSide
{
    class GUIFramework : PluginFramework
    {
        public GUIFramework(Control parent)
        {
            this.frameworkScriptNames.Add("gui.lua");

            this.registerEvents.Add(delegate(Lua lua)
            {

            });
        }
    }
}
