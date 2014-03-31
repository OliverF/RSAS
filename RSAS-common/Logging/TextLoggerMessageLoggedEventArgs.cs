using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSAS.Logging
{
    public class TextLoggerMessageLoggedEventArgs
    {
        public string Message { get; set; }

        public TextLoggerMessageLoggedEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
