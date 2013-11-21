using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lua4Net;
using Lua4Net.Types;
using RSAS.Utilities;

namespace RSAS.Networking.Messages
{
    [Serializable]
    public class LuaData : Message
    {
        Dictionary<string, LuaType> serializedTable;

        public string Identifier { get; set; }

        public LuaData(LuaTable table, string identifier)
        {
            this.serializedTable = LuaUtilities.LuaTableToDictionary(table);
            this.Identifier = identifier;
        }

        public void GetLuaTable(LuaTable lt)
        {
            LuaUtilities.DictionaryToLuaTable(this.serializedTable, lt);
        }
    }
}
