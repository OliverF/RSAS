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
using Lua4Net.Types;
using RSAS.Plugins;

namespace RSAS.ClientSide
{
    class GUIFramework : PluginFramework
    {
        enum ControlType { LineChart };

        Dictionary<string, ResizableControlWrapper> controls = new Dictionary<string, ResizableControlWrapper>();

        public GUIFramework(Control parent)
        {
            this.frameworkScriptNames.Add("gui.lua");

            this.registerEvents.Add(delegate(Lua lua)
            {
                lua.RegisterGlobalFunction("_RSAS_GUI_CreateControl", delegate(LuaManagedFunctionArgs args)
                {
                    LuaString controlID = args.Input[1] as LuaString;

                    //return if no unique control ID is specified
                    if (controlID == null)
                    {
                        return;
                    }

                    ControlType type;
                    try
                    {
                        type = (ControlType)Enum.Parse(typeof(ControlType), args.Input[0].ToString(), true);

                        ResizableControlWrapper control;
                        //handle each type of input
                        switch (type)
                        {
                            case ControlType.LineChart:
                                {
                                    control = new ResizableControlWrapper(new Chart());
                                    break;
                                }
                            default:
                                {
                                    control = new ResizableControlWrapper(new Control());
                                    break;
                                }
                        }
                        ConfigureControl(control.Control, controlID.Value);
                        controls.Add(controlID.Value, control);
                        parent.Controls.Add(control.Control);
                        return;
                    }
                    catch (Exception)
                    {
                        return;
                    }
                });
            });
        }

        private void ConfigureControl(Control control, string controlID)
        {
            //default configuration
            control.Location = new Point(10, 10);
            control.Width = 200;
            control.Height = 150;
        }
    }
}
