using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace RSAS.ServerSide
{
    [Serializable]
    class UserDirectoryNotEmptyException : Exception
    {
        public UserDirectoryNotEmptyException() { }

        public UserDirectoryNotEmptyException(string message) : base(message) { }

        public UserDirectoryNotEmptyException(string message, Exception innerException) : base(message, innerException) { }

        public UserDirectoryNotEmptyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
