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
        string jsonLuaTable;

        public string Identifier { get; set; }

        public LuaData(LuaTable table, string identifier)
        {
            this.jsonLuaTable = LuaUtilities.LuaTableToJson(table);
            this.Identifier = identifier;
        }

        public void GetLuaTable(LuaTable lt)
        {
            LuaUtilities.JsonToLuaTable(this.jsonLuaTable, lt);
        }
    }
}
