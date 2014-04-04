using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Lua4Net;
using Lua4Net.Types;

namespace RSAS.Plugins
{
    public delegate void ThreadSafeLuaErrorEventHandler(object sender, ThreadSafeLuaExecutionErrorEventArgs e);

    public class ThreadSafeLua
    {
        private Lua lua;
        private Mutex mutex = new Mutex();

        public event ThreadSafeLuaErrorEventHandler ExecutionError;

        private string currentSource = "";

        public ThreadSafeLua():this(new Lua())
        {
        }

        public ThreadSafeLua(Lua lua)
        {
            this.lua = lua;
            lua.RegisterGlobalFunction("_RSAS_SetSource", delegate(LuaManagedFunctionArgs args)
            {
                LuaString source = args.Input.ElementAt(0) as LuaString;

                if (source == null)
                    return;

                this.currentSource = source.Value;
            });
        }

        public LuaType[] Execute(string code, string source)
        {
            mutex.WaitOne();
            LuaType[] result = null;
            try
            {
                //set the current script source for plugins and frameworks to access
                lua.Execute("_RSAS_Source = [[" + source + "]]");
                this.currentSource = source;
                result = lua.Execute(code);
            }
            catch (LuaRuntimeErrorException e)
            {
                if (ExecutionError != null)
                    ExecutionError(this, new ThreadSafeLuaExecutionErrorEventArgs(e.Message, e.Line, currentSource));
            }
            catch (LuaSyntaxErrorException e)
            {
                if (ExecutionError != null)
                    ExecutionError(this, new ThreadSafeLuaExecutionErrorEventArgs(e.Message, e.Line, currentSource));
            }

            mutex.ReleaseMutex();

            return result;
        }

        public void RegisterGlobalFunction(string name, LuaManagedFunctionHandler handler)
        {
            lua.RegisterGlobalFunction(name, handler);
        }
    }
}
