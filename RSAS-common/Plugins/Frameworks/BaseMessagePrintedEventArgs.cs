using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSAS.Plugins.Frameworks
{
    public class BaseMessagePrintedEventArgs
    {
        public string Message { get; set; }

        public BaseMessagePrintedEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
