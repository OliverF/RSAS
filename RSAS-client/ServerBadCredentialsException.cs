using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace RSAS.ClientSide
{
    [Serializable]
    class ServerBadCredentialsException : Exception
    {
        public ServerBadCredentialsException() { }

        public ServerBadCredentialsException(string message) : base(message) { }

        public ServerBadCredentialsException(string message, Exception innerException) : base(message, innerException) { }

        public ServerBadCredentialsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
