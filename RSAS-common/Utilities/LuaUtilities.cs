using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lua4Net;
using Lua4Net.Types;

namespace RSAS.Utilities
{
    public class LuaUtilities
    {
        public delegate void RecurseLuaTableCallback(List<LuaValueType> path, LuaValueType key, LuaType value);

        public static void RecurseLuaTable(LuaTable root, RecurseLuaTableCallback callback)
        {
            RecurseLuaTable(root, new List<LuaValueType>(), callback);
        }

        static void RecurseLuaTable(LuaTable root, List<LuaValueType> path, RecurseLuaTableCallback callback)
        {
            //fields at the current level
            IDictionary<LuaValueType, LuaType> fields;

            if (path.Count == 0)
                fields = root.FetchTableFields();
            else
                fields = root.FetchTableFields(path);

            foreach (KeyValuePair<LuaValueType, LuaType> v in fields)
            {
                LuaTable tableValue = v.Value as LuaTable;

                if (tableValue == null)
                {
                    callback(path.ToList(), v.Key, v.Value);
                }
                else
                {
                    //copy current path (pointing to parent table)
                    List<LuaValueType> subPath = path.ToList();
                    //add element path (pointing to this table element)
                    subPath.Add(v.Key);
                    //recurse into this table element, starting at parent table path + this element key
                    RecurseLuaTable(root, subPath, callback);
                }

            }
        }

        public static Dictionary<List<LuaValueType>, LuaType> LuaTableToDictionary(LuaTable table)
        {
            Dictionary<List<LuaValueType>, LuaType> values = new Dictionary<List<LuaValueType>, LuaType>();
            RecurseLuaTableCallback lvc = delegate(List<LuaValueType> path, LuaValueType key, LuaType value)
            {
                path.Add(key);
                values.Add(path, value);
            };

            LuaUtilities.RecurseLuaTable(table, lvc);

            return values;
        }

        public static LuaTable DictionaryToLuaTable(Dictionary<List<LuaValueType>, LuaType> dictionary, LuaTable table)
        {

            foreach (KeyValuePair<List<LuaValueType>, LuaType> kv in dictionary)
            {
                //copy key path
                List<LuaValueType> path = kv.Key.ToList();
                //neglect last segment
                path.RemoveAt(path.Count - 1);

                if (path.Count > 0)
                {
                    //build sub tables
                    List<LuaValueType> cumulativePath = new List<LuaValueType>();
                    cumulativePath.Add(path[0]);
                    foreach (LuaValueType key in path)
                    {
                        LuaType value = table.GetField(cumulativePath);

                        LuaNilValue nil = value as LuaNilValue;
                        if (nil != null)
                            table.CreateSubTable(cumulativePath);
                    }
                }

                LuaValueType lvt = kv.Value as LuaValueType;
                table.SetFieldValue(kv.Key, lvt);
            }

            return table;
        }

        public static string GenerateTablePath(IEnumerable<LuaValueType> path)
        {
            string p = "";
            foreach (LuaValueType v in path)
            {
                p += "." + v.ToString();
            }

            return p;
        }
    }
}
