using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lua4Net;
using Lua4Net.Types;
using RSAS.Utilities;
using RSAS.Networking;

namespace RSAS.Plugins.Frameworks
{
    public class Base : PluginFramework
    {
        static string frameworkScriptName = "base.lua";

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
                        Console.WriteLine(a);
                    else
                        LuaUtilities.RecurseLuaTable(tbl, delegate(List<LuaValueType> path, LuaValueType key, LuaType value)
                        {
                            Console.WriteLine(LuaUtilities.GenerateTablePath(path) + LuaTablePath.TablePathSeparator + key + ": " + value);
                        });
                });
            });
        }
    }
}
