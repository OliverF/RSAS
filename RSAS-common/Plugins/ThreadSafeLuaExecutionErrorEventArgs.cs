using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSAS.Plugins
{
    public class ThreadSafeLuaExecutionErrorEventArgs:EventArgs
    {
        public string Message { get; set; }
        public int Line { get; set; }
        public string Source { get; set; }

        public ThreadSafeLuaExecutionErrorEventArgs(string message, int line, string source)
        {
            this.Message = message;
            this.Line = line;
            this.Source = source;
        }
    }
}
