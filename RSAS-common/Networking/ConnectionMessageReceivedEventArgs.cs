using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSAS.Networking
{
    public class ConnectionMessageReceivedEventArgs:EventArgs
    {
        public Message Message { get; set; }
        public ConnectionMessageReceivedEventArgs(Message message)
        {
            this.Message = message;
        }
    }
}
