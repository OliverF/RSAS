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

        public ThreadSafeLua()
        {
            this.lua = new Lua();
        }

        public ThreadSafeLua(Lua lua)
        {
            this.lua = lua;
        }

        public LuaType[] Execute(string code, string source)
        {
            mutex.WaitOne();
            LuaType[] result = null;
            try
            {
                result = lua.Execute(code);
            }
            catch (LuaRuntimeErrorException e)
            {
                if (ExecutionError != null)
                    ExecutionError(this, new ThreadSafeLuaExecutionErrorEventArgs(e.Message, e.Line, source));
            }
            catch (LuaSyntaxErrorException e)
            {
                if (ExecutionError != null)
                    ExecutionError(this, new ThreadSafeLuaExecutionErrorEventArgs(e.Message, e.Line, source));
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
