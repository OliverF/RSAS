using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Lua4Net;
using Lua4Net.Types;
using RSAS.Utilities;
using RSAS.Networking;

namespace RSAS.Plugins.Frameworks
{
    public delegate void BaseMessagePrintedEventHandler(object sender, BaseMessagePrintedEventArgs e);

    public class Base : PluginFramework
    {
        static string frameworkScriptName = "base.lua";

        public event BaseMessagePrintedEventHandler MessagePrinted;

        public Base()
        {
            this.frameworkScriptNames.Add(Base.frameworkScriptName);

            registerEvents.Add(delegate(ThreadSafeLua lua)
            {
                lua.RegisterGlobalFunction("_RSAS_Print", delegate(LuaManagedFunctionArgs args)
                {
                    LuaType a = args.Input.ElementAtOrDefault(0);

                    LuaTable tbl = a as LuaTable;

                    if (tbl == null)
                        PrintMessage(a.ToString());
                    else
                    {
                        string message = "";
                        LuaUtilities.RecurseLuaTable(tbl, delegate(List<LuaValueType> path, LuaValueType key, LuaType value)
                        {
                            message += LuaUtilities.GenerateTablePath(path) + LuaTablePath.TablePathSeparator + key + ": " + value + Environment.NewLine;
                        });
                        PrintMessage(message);
                    }
                });

                lua.RegisterGlobalFunction("_RSAS_Execute", delegate(LuaManagedFunctionArgs args)
                {
                    LuaString cmd = args.Input.ElementAtOrDefault(0) as LuaString;
                    LuaString cmdArgs = args.Input.ElementAtOrDefault(1) as LuaString;

                    if (cmd == null)
                        return;

                    ProcessStartInfo processInfo = new ProcessStartInfo(cmd.Value);
                    processInfo.RedirectStandardOutput = true;
                    processInfo.UseShellExecute = false;

                    if (cmdArgs != null)
                        processInfo.Arguments = cmdArgs.Value;

                    try
                    {
                        Process p = Process.Start(processInfo);
                        args.Output.Add(new LuaString(p.StandardOutput.ReadToEnd()));
                        p.Close();
                    }
                    catch
                    {
                        return;
                    }
                });
            });
        }

        void PrintMessage(string message)
        {
            if (MessagePrinted != null)
                MessagePrinted(this, new BaseMessagePrintedEventArgs(message));
        }
    }
}
