using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lua4Net;
using Lua4Net.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        static void MapLuaTablePathToJsonObject(JObject jObject, List<LuaValueType> remainingPath, LuaType endValue)
        {
            if (remainingPath.Count == 1)
            {
                //final index, set the value at this index
                string key = remainingPath[0].ToString();

                //find the appropriate type so that it remains the same at deserialization
                if (endValue.GetType() == typeof(LuaString))
                {
                    LuaString ls = endValue as LuaString;
                    jObject.Add(key, new JValue(ls.Value));
                }
                else if (endValue.GetType() == typeof(LuaNumber))
                {
                    LuaNumber ln = endValue as LuaNumber;
                    jObject.Add(key, new JValue(ln.Value));
                }
                else if (endValue.GetType() == typeof(LuaBoolean))
                {
                    LuaBoolean lb = endValue as LuaBoolean;
                    jObject.Add(key, new JValue(lb.Value));
                }
            }
            else
            {
                //if this is not the final index, this must be another sub-table
                //sub-tables can be represented with another JObject
                JObject subObject;

                //check if the sub-table exists from a previous recursion
                if (jObject.Property(remainingPath[0].ToString()) == null)
                {
                    //property does not exist, create new JSON object
                    subObject = new JObject();
                    //add to parent
                    jObject.Add(remainingPath[0].ToString(), subObject);
                }
                else
                {
                    //property exists, get the object representing the sub-table
                    subObject = jObject[remainingPath[0].ToString()] as JObject;
                }

                //remove the segment of the path that has been mapped
                remainingPath.RemoveAt(0);

                //recurse another level
                MapLuaTablePathToJsonObject(subObject, remainingPath, endValue);
            }
        }

        public static string LuaTableToJson(LuaTable table)
        {
            JObject root = new JObject();

            RecurseLuaTableCallback callback = delegate(List<LuaValueType> path, LuaValueType key, LuaType value)
            {
                //build path to value
                path.Add(key);

                MapLuaTablePathToJsonObject(root, path, value);
            };

            RecurseLuaTable(table, callback);

            string json = root.ToString(Formatting.None, new JsonConverter[0]);
            return json;
        }

        static void MapJsonObjectToLuaTable(JObject root, LuaTable table)
        {
            MapJsonObjectToLuaTable(root, table, "");
        }

        static void MapJsonObjectToLuaTable(JObject root, LuaTable table, string path)
        {
            foreach (JProperty property in root.Children())
            {
                string currentPath;
                if (path != "")
                    currentPath = path + LuaTablePath.TablePathSeparator + property.Name;
                else
                    currentPath = property.Name;

                JToken child = property.Children().ElementAt(0);

                if (child.Type == JTokenType.Object)
                {
                    //new sub-table
                    table.CreateSubTable(LuaTablePath.StringToPath(currentPath));
                    MapJsonObjectToLuaTable(child as JObject, table, currentPath);
                }
                else
                {
                    //final value
                    switch (property.Value.Type)
                    {
                        case JTokenType.String:
                            table.SetFieldValue(LuaTablePath.StringToPath(currentPath), new LuaString(property.Value.ToString()));
                            break;
                        case JTokenType.Float:
                            double doubleValue = 0;
                            if (double.TryParse(property.Value.ToString(), out doubleValue))
                                table.SetFieldValue(LuaTablePath.StringToPath(currentPath), new LuaNumber(doubleValue));
                            break;
                        case JTokenType.Boolean:
                            bool boolValue = false;
                            if (bool.TryParse(property.Value.ToString(), out boolValue))
                                table.SetFieldValue(LuaTablePath.StringToPath(currentPath), new LuaBoolean(boolValue));
                            break;
                    }
                }
            }
        }

        public static LuaTable JsonToLuaTable(string json, LuaTable table)
        {
            JObject root = JObject.Parse(json);

            MapJsonObjectToLuaTable(root, table);

            return table;
        }
    }
}
