using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSAS.Networking
{
    [Serializable]
    public class Message
    {
        string type;
        object payload;
        bool isValid;

        public string Type
        {
            get { return this.type; }
        }

        public object Payload
        {
            get { return this.payload; }
        }

        public bool IsValid
        {
            get { return this.isValid; }
        }

        public Message(string type, object payload)
        {
            this.type = type;
            Type test = payload.GetType();
            if (payload.GetType().IsSerializable)
            {
                this.isValid = true;
                this.payload = payload;
            }
            else
            {
                this.isValid = false;
            }
        }
    }
}
