using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Lua4Net;
using Lua4Net.Types;

namespace RSAS.Plugins.Frameworks
{
    public class Timer : PluginFramework
    {
        public Timer()
        {
            this.frameworkScriptNames.Add("timer.lua");

            registerEvents.Add(delegate(Lua lua)
            {
                lua.RegisterGlobalFunction("_RSAS_Timer_Register", delegate(LuaManagedFunctionArgs args)
                {
                    string callback = args.Input[0].ToString();
                    LuaNumber delay = args.Input[1] as LuaNumber;

                    if (delay != null)
                    {
                        System.Timers.Timer timer = new System.Timers.Timer(delay.Value);
                        timer.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs e)
                        {
                            lua.Execute("RSAS.Timer.TriggerCallback('" + callback + "')");
                        });
                        timer.Start();
                    }
                });
            });
        }
    }
}
