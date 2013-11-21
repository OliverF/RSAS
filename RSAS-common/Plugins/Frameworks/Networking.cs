using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lua4Net;
using Lua4Net.Types;
using RSAS.Networking;
using RSAS.Networking.Messages;
using RSAS.Utilities;

namespace RSAS.Plugins.Frameworks
{
    public class Networking : PluginFramework
    {

        Dictionary<string, LuaData> luaDataBuffer = new Dictionary<string, LuaData>();

        Connection connection;

        Lua lua;

        public Networking(Connection con)
        {
            this.connection = con;
            this.connection.MessageReceived += new ConnectionMessageReceivedEventHandler(MessageReceived);

            this.frameworkScriptNames.Add("networking.lua");

            registerEvents.Add(delegate(Lua lua)
            {
                this.lua = lua;

                lua.RegisterGlobalFunction("_RSAS_Networking_SendTable", delegate(LuaManagedFunctionArgs args)
                {
                    string identifier = args.Input[0].ToString();
                    LuaTable table = args.Input[1] as LuaTable;

                    if (table != null)
                        connection.SendMessage(new LuaData(table, identifier));
                });

                lua.RegisterGlobalFunction("_RSAS_Networking_GetTable", delegate(LuaManagedFunctionArgs args)
                {
                    string identifier = args.Input[0].ToString();
                    LuaTable table = args.Input[1] as LuaTable;

                    if (table != null && luaDataBuffer.ContainsKey(identifier))
                    {
                        luaDataBuffer[identifier].GetLuaTable(table);
                    }
                });
            });
        }

        void MessageReceived(object sender, ConnectionMessageReceivedEventArgs e)
        {
            if (e.Message.GetType() == typeof(LuaData))
            {
                LuaData data = e.Message as LuaData;

                //load data into buffer
                if (this.luaDataBuffer.ContainsKey(data.Identifier))
                    this.luaDataBuffer[data.Identifier] = data;
                else
                    this.luaDataBuffer.Add(data.Identifier, data);

                //let plugin know data has arrived
                this.TriggerCallbacks(data.Identifier);
            }
        }

        void TriggerCallbacks(string identifier)
        {
            lua.Execute("RSAS.Networking.TriggerCallback('" + identifier + "')");
        }
    }
}
