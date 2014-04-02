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
using System.Threading;
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
                lua.RegisterGlobalFunction("_RSAS_GUI_Chart_SetAxesLimits", this.ChartSetAxesLimits);
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
            LuaString controlID = args.Input.ElementAtOrDefault(0) as LuaString;
            LuaString text = args.Input.ElementAtOrDefault(1) as LuaString;

            if (controlID == null || text == null || !this.controls.ContainsKey(controlID.Value))
                return;

            Button button = this.controls[controlID.Value] as Button;

            if (button == null)
                return;

            ControlWork work = delegate()
            {
                button.Text = text.Value;
            };

            if (button.InvokeRequired)
                button.Invoke(work);
            else
                work();
        }

        private void LabelSetText(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input.ElementAtOrDefault(0) as LuaString;
            LuaString text = args.Input.ElementAtOrDefault(1) as LuaString;

            if (controlID == null || text == null || !this.controls.ContainsKey(controlID.Value))
                return;

            Label label = this.controls[controlID.Value] as Label;

            if (label == null)
                return;

            ControlWork work = delegate()
            {
                label.Text = text.Value;
            };

            if (label.InvokeRequired)
                label.Invoke(work);
            else
                work();
        }

        private void ChartCreateSeries(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input.ElementAtOrDefault(0) as LuaString;
            LuaString seriesName = args.Input.ElementAtOrDefault(1) as LuaString;
            LuaString seriesType = args.Input.ElementAtOrDefault(2) as LuaString;

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

            ControlWork work = delegate()
            {
                chart.Series.Add(series);
            };

            if (chart.InvokeRequired)
                chart.Invoke(work);
            else
                work();
        }

        private void ChartSetXY(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input.ElementAtOrDefault(0) as LuaString;
            LuaTable values = args.Input.ElementAtOrDefault(1) as LuaTable;
            LuaString seriesName = args.Input.ElementAtOrDefault(2) as LuaString;

            if (values == null || controlID == null || seriesName == null || !this.controls.ContainsKey(controlID.Value))
                return;

            Chart chart = this.controls[controlID.Value] as Chart;

            if (chart == null)
                return;

            Series series = chart.Series.FindByName(seriesName.Value);

            if (series == null)
                return;

            ControlWork work = delegate()
            {
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
            };

            if (chart.InvokeRequired)
            {
                chart.Invoke(work);
            }
            else
                work();
        }

        private void ChartSetAxesLimits(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input.ElementAtOrDefault(0) as LuaString;
            LuaNumber xAxisMin = args.Input.ElementAtOrDefault(1) as LuaNumber;
            LuaNumber xAxisMax = args.Input.ElementAtOrDefault(2) as LuaNumber;
            LuaNumber yAxisMin = args.Input.ElementAtOrDefault(3) as LuaNumber;
            LuaNumber yAxisMax = args.Input.ElementAtOrDefault(4) as LuaNumber;

            if (controlID == null || !this.controls.ContainsKey(controlID.Value))
                return;

            Chart chart = this.controls[controlID.Value] as Chart;

            if (chart == null)
                return;

            ControlWork work = delegate()
            {
                if (xAxisMin != null)
                    chart.ChartAreas[0].AxisX.Minimum = xAxisMin.Value;

                if (xAxisMax != null)
                    chart.ChartAreas[0].AxisX.Maximum = xAxisMax.Value;

                if (yAxisMin != null)
                    chart.ChartAreas[0].AxisY.Minimum = yAxisMin.Value;

                if (yAxisMax != null)
                    chart.ChartAreas[0].AxisY.Maximum = yAxisMax.Value;
            };

            if (chart.InvokeRequired)
                chart.Invoke(work);
            else
                work();
        }

        private void ControlGetParent(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input.ElementAtOrDefault(0) as LuaString;

            if (controlID == null || !this.controls.ContainsKey(controlID.Value))
                return;

            Control parent = controls[controlID.Value].Parent;

            if (this.controls.ContainsValue(parent))
                args.Output.Add(new LuaString(parent.Name));
        }

        private void ControlGetSize(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input.ElementAtOrDefault(0) as LuaString;

            if (controlID == null || !this.controls.ContainsKey(controlID.Value))
                return;

            args.Output.Add(new LuaNumber(controls[controlID.Value].Size.Width));
            args.Output.Add(new LuaNumber(controls[controlID.Value].Size.Height));
        }

        private void ControlGetLocation(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input.ElementAtOrDefault(0) as LuaString;

            if (controlID == null || !this.controls.ContainsKey(controlID.Value))
                return;

            args.Output.Add(new LuaNumber(controls[controlID.Value].Location.X));
            args.Output.Add(new LuaNumber(controls[controlID.Value].Location.Y));
        }

        private void ControlSetSize(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input.ElementAtOrDefault(0) as LuaString;
            LuaNumber width = args.Input.ElementAtOrDefault(1) as LuaNumber;
            LuaNumber height = args.Input.ElementAtOrDefault(2) as LuaNumber;

            if (controlID == null || !this.controls.ContainsKey(controlID.Value))
                return;

            Control control = controls[controlID.Value];

            ControlWork work = delegate()
            {
                if (width != null && height != null)
                {
                    control.Size = new Size((int)width.Value, (int)height.Value);
                }
                else if (width != null)
                {
                    control.Size = new Size((int)width.Value, control.Size.Height);
                }
                else if (height != null)
                {
                    control.Size = new Size(control.Size.Width, (int)height.Value);
                }
            };

            if (parent.InvokeRequired)
                parent.Invoke(work);
            else
                work();
        }

        private void ControlSetLocation(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input.ElementAtOrDefault(0) as LuaString;
            LuaNumber x = args.Input.ElementAtOrDefault(1) as LuaNumber;
            LuaNumber y = args.Input.ElementAtOrDefault(2) as LuaNumber;

            if (controlID == null || !this.controls.ContainsKey(controlID.Value))
                return;

            Control control = controls[controlID.Value];

            ControlWork work = delegate()
            {
                if (x != null && y != null)
                {
                    control.Location = new Point((int)x.Value, (int)y.Value);
                }
                else if (x != null)
                {
                    control.Location = new Point((int)x.Value, control.Location.Y);
                }
                else if (y != null)
                {
                    control.Location = new Point(control.Location.X, (int)y.Value);
                }
            };

            if (control.InvokeRequired)
                control.Invoke(work);
            else
                work();
        }

        private void ControlRemove(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input.ElementAtOrDefault(0) as LuaString;

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
                control.Parent.Invoke(work);
            else
                work();
        }

        private void ControlSetParent(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input.ElementAtOrDefault(0) as LuaString;
            LuaString parentControlID = args.Input.ElementAtOrDefault(1) as LuaString;

            if (controlID == null || parentControlID == null || !this.controls.ContainsKey(controlID.Value) || !this.controls.ContainsKey(parentControlID.Value))
                return;

            Control control = this.controls[controlID.Value];
            Control parentControl = this.controls[parentControlID.Value];

            ControlWork work = delegate()
            {
                parentControl.Controls.Add(control);
            };

            if (parentControl.InvokeRequired)
                parentControl.Invoke(work);
            else
                work();
        }

        private void CreateControl(LuaManagedFunctionArgs args)
        {
            LuaString controlID = args.Input.ElementAtOrDefault(1) as LuaString;

            //return if no unique control ID is specified
            if (controlID == null)
            {
                return;
            }

            ControlType type;
            try
            {
                type = (ControlType)Enum.Parse(typeof(ControlType), args.Input.ElementAtOrDefault(0).ToString(), true);

                ControlWork work = delegate()
                {
                    Control control = ControlFromType(type);
                    control.Name = controlID.Value;

                    control.MouseClick += delegate(object sender, MouseEventArgs e)
                    {
                        /*
                         * Must use a separate thread for this callback. This MouseClick delegate will be run in the GUI thread.
                         * If a non-GUI thread calls another method of this class (though it will always be through through Lua.Execute),
                         * then that thread must invoke a delegate using Control.Invoke() in order to be called in the GUI thread to safely modify a control.
                         * Control.Invoke will hang the non-GUI thread until the GUI thread has had time to process the message queue,
                         * and while it waits it also holds the mutex lock in ThreadSafeLua. So, the mutex lock is locked for any other thread and
                         * the non-GUI thread is waiting on the GUI thread before it can move on and release the mutex (because of Control.Invoke)
                         * and therefore if the MouseClick event is fired during this time, the GUI thread will call lua.Execute, which will
                         * wait for the mutex lock to be released by the non-GUI thread. In that case, the GUI thread is waiting for the non-GUI
                         * thread and vice versa. Then we have a deadlock and the GUI stops responding.
                         */
                        Thread t = new Thread(delegate()
                        {
                            lua.Execute("RSAS.GUI.Trigger('" + controlID + "', 'OnMouseClick')", GUIFramework.frameworkScriptName);
                        });
                        t.IsBackground = true;
                        t.Start();
                    };
                    control.MouseEnter += delegate(object sender, EventArgs e)
                    {
                        Thread t = new Thread(delegate()
                        {
                            lua.Execute("RSAS.GUI.Trigger('" + controlID + "', 'OnMouseEnter')", GUIFramework.frameworkScriptName);
                        });
                        t.IsBackground = true;
                        t.Start();
                    };
                    control.MouseLeave += delegate(object sender, EventArgs e)
                    {
                        Thread t = new Thread(delegate()
                        {
                            lua.Execute("RSAS.GUI.Trigger('" + controlID + "', 'OnMouseLeave')", GUIFramework.frameworkScriptName);
                        });
                        t.IsBackground = true;
                        t.Start();
                    };
                    controls.Add(controlID.Value, control);
                    parent.Controls.Add(control);
                };

                if (parent.InvokeRequired)
                {
                    parent.Invoke(work);
                }
                else
                    work();

                return;
            }
            catch (ArgumentException)
            {
                return;
            }
        }
    }
}
