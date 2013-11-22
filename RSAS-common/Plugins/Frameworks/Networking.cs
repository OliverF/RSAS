using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        ObservableCollection<Connection> connections;

        Lua lua;

        public Networking(ObservableCollection<Connection> cons)
        {
            this.connections = cons;

            foreach (Connection con in cons)
            {
                con.MessageReceived += new ConnectionMessageReceivedEventHandler(MessageReceived);
            }

            //hook in to future list changes
            cons.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ConnectionsCollectionChanged);

            this.frameworkScriptNames.Add("networking.lua");

            registerEvents.Add(delegate(Lua lua)
            {
                this.lua = lua;

                lua.RegisterGlobalFunction("_RSAS_Networking_SendTable", delegate(LuaManagedFunctionArgs args)
                {
                    string identifier = args.Input[0].ToString();
                    LuaTable table = args.Input[1] as LuaTable;

                    if (table != null)
                        foreach(Connection con in this.connections)
                            con.SendMessage(new LuaData(table, identifier));
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

        void ConnectionsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (Connection con in e.NewItems)
            {
                con.MessageReceived += new ConnectionMessageReceivedEventHandler(MessageReceived);
            }
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
