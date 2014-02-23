using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization.Charting.Data;
using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
using System.ComponentModel;
using Lua4Net;
using Lua4Net.Types;
using RSAS.Plugins;
using RSAS.Utilities;

namespace RSAS.ClientSide
{
    class GUIFramework : PluginFramework
    {
        enum ControlType { Chart, Label };

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
                            case ControlType.Chart:
                                {
                                    Chart chart = new Chart();

                                    ChartArea chartArea = new ChartArea();
                                    chart.ChartAreas.Add(chartArea);

                                    Legend legend = new Legend();
                                    legend.Name = "legend";
                                    chart.Legends.Add(legend);

                                    control = new ResizableControlWrapper(chart);
                                    break;
                                }
                            case ControlType.Label:
                                {
                                    control = new ResizableControlWrapper(new Label());
                                    break;
                                }
                            default:
                                {
                                    control = new ResizableControlWrapper(new Control());
                                    break;
                                }
                        }
                        ConfigureControl(control.BaseControl, controlID.Value);
                        controls.Add(controlID.Value, control);
                        parent.Controls.Add(control.BaseControl);
                        return;
                    }
                    catch (ArgumentException)
                    {
                        return;
                    }
                });

                lua.RegisterGlobalFunction("_RSAS_GUI_Control_SetParent", delegate(LuaManagedFunctionArgs args)
                {
                    LuaString controlID = args.Input[0] as LuaString;
                    LuaString parentControlID = args.Input[1] as LuaString;

                    if (controlID == null || parentControlID == null || !this.controls.ContainsKey(controlID.Value) || !this.controls.ContainsKey(parentControlID.Value))
                        return;

                    Control parentControl = this.controls[parentControlID.Value].BaseControl;

                    parentControl.Controls.Add(this.controls[controlID.Value].BaseControl);

                });

                lua.RegisterGlobalFunction("_RSAS_GUI_Chart_SetXY", delegate(LuaManagedFunctionArgs args)
                {
                    LuaString controlID = args.Input[0] as LuaString;
                    LuaTable values = args.Input[1] as LuaTable;
                    LuaString seriesName = args.Input[2] as LuaString;

                    if (values == null || controlID == null || seriesName == null || !this.controls.ContainsKey(controlID.Value))
                        return;

                    Chart chart = this.controls[controlID.Value].BaseControl as Chart;

                    if (chart == null)
                        return;
                    
                    Series series = chart.Series.FindByName(seriesName.Value);

                    if (series == null)
                        return;

                    //reset
                    series.Points.Clear();

                    Dictionary<LuaValueType, DataPoint> points = new Dictionary<LuaValueType, DataPoint>();

                    LuaUtilities.RecurseLuaTableCallback callback = new LuaUtilities.RecurseLuaTableCallback(delegate(List<LuaValueType> path, LuaValueType key, LuaType value)
                    {
                        if (path.Count <= 0)
                            return;

                        //set new point if this key doesn't exist
                        if (!points.ContainsKey(path[0]))
                            points.Add(path[0], new DataPoint());

                        LuaNumber component = value as LuaNumber;

                        if (component == null)
                            return;

                        //check type of element - x or y
                        if (key.ToString() == "x")
                        {
                            points[path[0]].XValue = component.Value;
                        }
                        else if (key.ToString() == "y")
                        {
                            double[] yValues = { component.Value };
                            points[path[0]].YValues = yValues;
                        }
                    });

                    LuaUtilities.RecurseLuaTable(values, callback);

                    foreach (KeyValuePair<LuaValueType, DataPoint> kv in points)
                        series.Points.Add(kv.Value);
                });

                lua.RegisterGlobalFunction("_RSAS_GUI_Chart_CreateSeries", delegate(LuaManagedFunctionArgs args)
                {
                    LuaString controlID = args.Input[0] as LuaString;
                    LuaString seriesName = args.Input[1] as LuaString;
                    LuaString seriesType = args.Input[2] as LuaString;

                    if (controlID == null || seriesName == null || !this.controls.ContainsKey(controlID.Value))
                        return;

                    Chart chart = this.controls[controlID.Value].BaseControl as Chart;

                    if (chart == null)
                        return;

                    Series series = new Series(seriesName.Value);
                    series.ChartType = SeriesChartType.Line;
                    

                    if (seriesType != null)
                    {
                        try
                        {
                            SeriesChartType type = (SeriesChartType)Enum.Parse(typeof(SeriesChartType), seriesType.ToString(), true);
                            series.ChartType = type;
                        }
                        catch (ArgumentException) { }
                    }

                    chart.Series.Add(series);
                });

                lua.RegisterGlobalFunction("_RSAS_GUI_Label_SetText", delegate(LuaManagedFunctionArgs args)
                {
                    LuaString controlID = args.Input[0] as LuaString;
                    LuaString text = args.Input[1] as LuaString;

                    if (controlID == null || text == null || !this.controls.ContainsKey(controlID.Value))
                        return;

                    Label label = this.controls[controlID.Value].BaseControl as Label;

                    if (label == null)
                        return;

                    label.Text = text.Value;
                    label.Update();
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
