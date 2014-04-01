using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace RSAS.ClientSide
{
    [Serializable]
    class ServerUnreachableException : Exception
    {
        public ServerUnreachableException() { }

        public ServerUnreachableException(string message) : base(message) { }

        public ServerUnreachableException(string message, Exception innerException) : base(message, innerException) { }

        public ServerUnreachableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
