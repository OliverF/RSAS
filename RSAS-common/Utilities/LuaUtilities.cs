using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lua4Net;
using Lua4Net.Types;

namespace RSAS.Utilities
{
    class LuaUtilities
    {
        public delegate void RecurseLuaTableCallback(string path, LuaValueType key, LuaType value);

        public static void RecurseLuaTable(LuaTable root, RecurseLuaTableCallback callback)
        {
            RecurseLuaTable(root, "", 0, callback);
        }

        static void RecurseLuaTable(LuaTable root, string sub, int level, RecurseLuaTableCallback callback)
        {
            IDictionary<LuaValueType, LuaType> fields;

            if (sub == "")
                fields = root.FetchTableFields();
            else
                fields = root.FetchTableFields(LuaTablePath.StringToPath(sub));

            foreach (KeyValuePair<LuaValueType, LuaType> v in fields)
            {
                LuaTable tableValue = v.Value as LuaTable;

                if (tableValue == null)
                {
                    callback(sub, v.Key, v.Value);
                }
                else
                {
                    string path = "";
                    if (sub == "")
                        path = v.Key.ToString();
                    else
                        path = sub + "." + v.Key.ToString();
                    int nlevel = level + 1;
                    RecurseLuaTable(root, path, nlevel, callback);
                }

            }
        }

        public static Dictionary<string, LuaType> LuaTableToDictionary(LuaTable table)
        {
            Dictionary<string, LuaType> values = new Dictionary<string, LuaType>();
            RecurseLuaTableCallback lvc = delegate(string path, LuaValueType key, LuaType value)
            {
                string totalPath = "";
                if (path == "")
                    totalPath = key.ToString();
                else
                    totalPath = path + "." + key.ToString();
                values.Add(totalPath, value);
            };

            LuaUtilities.RecurseLuaTable(table, lvc);

            return values;
        }

        public static LuaTable DictionaryToLuaTable(Dictionary<string, LuaType> dictionary, LuaTable lt)
        {

            foreach (KeyValuePair<string, LuaType> kv in dictionary)
            {
                //find path to this key
                int dotCount = kv.Key.Count(f => f == LuaTablePath.TablePathSeparator);
                int lastDotPos = kv.Key.LastIndexOf(LuaTablePath.TablePathSeparator);
                string path = kv.Key;
                if (lastDotPos > 0 && dotCount > 0)
                    path = kv.Key.Remove(lastDotPos);

                LuaType value = lt.GetField(LuaTablePath.StringToPath(path));

                LuaNilValue nil = value as LuaNilValue;

                if (nil != null && dotCount > 0)
                    lt.CreateSubTable(LuaTablePath.StringToPath(path));

                LuaValueType lvt = kv.Value as LuaValueType;
                lt.SetFieldValue(LuaTablePath.StringToPath(kv.Key), lvt);
            }

            return lt;
        }
    }
}
