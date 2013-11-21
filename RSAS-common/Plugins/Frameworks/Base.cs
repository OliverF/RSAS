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
        public Base()
        {
            this.frameworkScriptNames.Add("base.lua");

            registerEvents.Add(delegate(Lua lua)
            {
                lua.RegisterGlobalFunction("_RSAS_Print", delegate(LuaManagedFunctionArgs args)
                {
                    LuaType a = args.Input[0];

                    LuaTable tbl = a as LuaTable;

                    if (tbl == null)
                        Console.WriteLine(a);
                    else
                        LuaUtilities.RecurseLuaTable(tbl, delegate(string path, LuaValueType key, LuaType value)
                        {
                            Console.WriteLine(path + LuaTablePath.TablePathSeparator + key + ": " + value);
                        });
                });
            });
        }
    }
}
