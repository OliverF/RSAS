using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSAS.Networking.Messages
{
    [Serializable]
    public class Generic : Message
    {
        public object Payload{ get; set; }

        public Generic(object payload)
        {
            this.Payload = payload;
        }
    }
}
