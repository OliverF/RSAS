﻿using System;
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
        static string frameworkScriptName = "gui.lua";

        enum ControlType { Container, Chart, Label, Button };

        Dictionary<string, Control> controls = new Dictionary<string, Control>();

        Control parent;

        delegate void ControlWork();

        ThreadSafeLua lua;

        public GUIFramework(Control parent)
        {
            this.parent = parent;

            this.frameworkScriptNames.Add(frameworkScriptName);

            this.registerEvents.Add(delegate(ThreadSafeLua lua)
            {
                this.lua = lua;
                lua.RegisterGlobalFunction("_RSAS_GUI_CreateControl", this.CreateControl);
                lua.RegisterGlobalFunction("_RSAS_GUI_Control_SetParent", this.ControlSetParent);
                lua.RegisterGlobalFunction("_RSAS_GUI_Control_GetParent", this.ControlGetParent);
                lua.RegisterGlobalFunction("_RSAS_GUI_Control_Remove", this.ControlRemove);
                lua.RegisterGlobalFunction("_RSAS_GUI_Control_SetLocation", this.ControlSetLocation);
                lua.RegisterGlobalFunction("_RSAS_GUI_Control_SetSize", this.ControlSetSize);
                lua.RegisterGlobalFunction("_RSAS_GUI_Control_GetLocation", this.ControlGetLocation);
                lua.RegisterGlobalFunction("_RSAS_GUI_Control_GetSize", this.ControlGetSize);
                lua.RegisterGlobalFunction("_RSAS_GUI_Chart_SetXY", this.ChartSetXY);
                lua.RegisterGlobalFunction("_RSAS_GUI_Chart_CreateSeries", this.ChartCreateSeries);
                lua.RegisterGlobalFunction("_RSAS_GUI_Label_SetText", this.LabelSetText);
                lua.RegisterGlobalFunction("_RSAS_GUI_Button_SetText", this.ButtonSetText);
            });
        }

        private Control ControlFromType(ControlType type)
        {
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

                        return chart;
                    }
                case ControlType.Label:
                    {
                        return new Label();
                    }
                case ControlType.Container:
                    {
                        return new ResizableContainer();
                    }
                case ControlType.Button:
                    {
                        return new Button();
                    }
                default:
                    {
                        return new Control();
                    }
            }
        }

        private void ButtonSetText(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input[0] as LuaString;
            LuaString text = args.Input[1] as LuaString;

            if (controlID == null || text == null || !this.controls.ContainsKey(controlID.Value))
                return;

            Button button = this.controls[controlID.Value] as Button;

            if (button == null)
                return;

            button.Text = text.Value;
        }

        private void LabelSetText(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input[0] as LuaString;
            LuaString text = args.Input[1] as LuaString;

            if (controlID == null || text == null || !this.controls.ContainsKey(controlID.Value))
                return;

            Label label = this.controls[controlID.Value] as Label;

            if (label == null)
                return;

            label.Text = text.Value;
        }

        private void ChartCreateSeries(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input[0] as LuaString;
            LuaString seriesName = args.Input[1] as LuaString;
            LuaString seriesType = args.Input[2] as LuaString;

            if (controlID == null || seriesName == null || !this.controls.ContainsKey(controlID.Value))
                return;

            Chart chart = this.controls[controlID.Value] as Chart;

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
        }

        private void ChartSetXY(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input[0] as LuaString;
            LuaTable values = args.Input[1] as LuaTable;
            LuaString seriesName = args.Input[2] as LuaString;

            if (values == null || controlID == null || seriesName == null || !this.controls.ContainsKey(controlID.Value))
                return;

            Chart chart = this.controls[controlID.Value] as Chart;

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
        }

        private void ControlGetParent(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input[0] as LuaString;

            if (controlID == null || !this.controls.ContainsKey(controlID.Value))
                return;

            Control parent = controls[controlID.Value].Parent;

            if (this.controls.ContainsValue(parent))
                args.Output.Add(new LuaString(parent.Name));
        }

        private void ControlGetSize(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input[0] as LuaString;

            if (controlID == null || !this.controls.ContainsKey(controlID.Value))
                return;

            args.Output.Add(new LuaNumber(controls[controlID.Value].Size.Width));
            args.Output.Add(new LuaNumber(controls[controlID.Value].Size.Height));
        }

        private void ControlGetLocation(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input[0] as LuaString;

            if (controlID == null || !this.controls.ContainsKey(controlID.Value))
                return;

            args.Output.Add(new LuaNumber(controls[controlID.Value].Location.X));
            args.Output.Add(new LuaNumber(controls[controlID.Value].Location.Y));
        }

        private void ControlSetSize(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input[0] as LuaString;
            LuaNumber width = args.Input[1] as LuaNumber;
            LuaNumber height = args.Input[2] as LuaNumber;

            if (controlID == null || !this.controls.ContainsKey(controlID.Value))
                return;

            if (width != null && height != null)
            {
                controls[controlID.Value].Size = new Size((int)width.Value, (int)height.Value);
            }
            else if (width != null)
            {
                controls[controlID.Value].Size = new Size((int)width.Value, controls[controlID.Value].Size.Height);
            }
            else if (height != null)
            {
                controls[controlID.Value].Size = new Size(controls[controlID.Value].Size.Width, (int)height.Value);
            }
        }

        private void ControlSetLocation(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input[0] as LuaString;
            LuaNumber x = args.Input[1] as LuaNumber;
            LuaNumber y = args.Input[2] as LuaNumber;

            if (controlID == null || !this.controls.ContainsKey(controlID.Value))
                return;

            if (x != null && y != null)
            {
                controls[controlID.Value].Location = new Point((int)x.Value, (int)y.Value);
            }
            else if (x != null)
            {
                controls[controlID.Value].Location = new Point((int)x.Value, controls[controlID.Value].Location.Y);
            }
            else if (y != null)
            {
                controls[controlID.Value].Location = new Point(controls[controlID.Value].Location.X, (int)y.Value);
            }
        }

        private void ControlRemove(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input[0] as LuaString;

            if (controlID == null || !this.controls.ContainsKey(controlID.Value))
            {
                return;
            }

            Control control = this.controls[controlID.Value];

            if (control.Parent == null)
                return;

            //need to modify the parent control, created in a different thread
            //use thread-safe invoke

            ControlWork work = delegate()
            {
                control.Parent.Controls.Remove(control);
            };

            if (control.Parent.InvokeRequired)
            {
                control.Parent.Invoke(work);
            }
            else
            {
                work();
            }
        }

        private void ControlSetParent(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input[0] as LuaString;
            LuaString parentControlID = args.Input[1] as LuaString;

            if (controlID == null || parentControlID == null || !this.controls.ContainsKey(controlID.Value) || !this.controls.ContainsKey(parentControlID.Value))
                return;

            Control parentControl = this.controls[parentControlID.Value];

            ControlWork work = delegate()
            {
                parentControl.Controls.Add(this.controls[controlID.Value]);
            };

            if (parentControl.InvokeRequired)
            {
                parentControl.Invoke(work);
            }
            else
            {
                work();
            }
        }

        private void CreateControl(LuaManagedFunctionArgs args)
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

                Control control = ControlFromType(type);
                control.Name = controlID.Value;

                control.MouseClick += delegate(object sender, MouseEventArgs e)
                {
                    lua.Execute("RSAS.GUI.Trigger('" + controlID + "', 'OnMouseClick')", GUIFramework.frameworkScriptName);
                };
                control.MouseEnter += delegate(object sender, EventArgs e)
                {
                    lua.Execute("RSAS.GUI.Trigger('" + controlID + "', 'OnMouseEnter')", GUIFramework.frameworkScriptName);
                };
                control.MouseLeave += delegate(object sender, EventArgs e)
                {
                    lua.Execute("RSAS.GUI.Trigger('" + controlID + "', 'OnMouseLeave')", GUIFramework.frameworkScriptName);
                };

                controls.Add(controlID.Value, control);
                parent.Controls.Add(control);
                return;
            }
            catch (ArgumentException)
            {
                return;
            }
        }
    }
}
