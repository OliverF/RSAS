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
        static string frameworkScriptName = "timer.lua";

        Dictionary<string, System.Timers.Timer> timers = new Dictionary<string, System.Timers.Timer>();

        public Timer()
        {
            this.frameworkScriptNames.Add(Timer.frameworkScriptName);

            registerEvents.Add(delegate(ThreadSafeLua lua)
            {
                lua.RegisterGlobalFunction("_RSAS_Timer_Register", delegate(LuaManagedFunctionArgs args)
                {
                    LuaString callback = args.Input.ElementAtOrDefault(0) as LuaString;
                    LuaNumber delay = args.Input.ElementAtOrDefault(1) as LuaNumber;

                    if (callback != null && delay != null)
                    {
                        if (timers.ContainsKey(callback.Value))
                            return;
                        System.Timers.Timer timer = new System.Timers.Timer(delay.Value);
                        timer.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs e)
                        {
                            lua.Execute("RSAS.Timer.TriggerCallback('" + callback.Value + "')", Timer.frameworkScriptName);
                        });
                        timer.Start();

                        timers.Add(callback.Value, timer);
                    }
                });

                lua.RegisterGlobalFunction("_RSAS_Timer_Unregister", delegate(LuaManagedFunctionArgs args)
                {
                    LuaString callback = args.Input.ElementAtOrDefault(0) as LuaString;

                    if (callback != null)
                    {
                        if (!timers.ContainsKey(callback.Value))
                            return;

                        timers[callback.Value].Stop();

                        timers.Remove(callback.Value);
                    }
                });
            });
        }
    }
}
