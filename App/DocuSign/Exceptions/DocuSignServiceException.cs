using System;
using System.Runtime.Serialization;

namespace DocuSign.Exceptions
{
    [Serializable]
    public class DocuSignServiceException : Exception
    {
        public DocuSignServiceException() : base() { }

        public DocuSignServiceException(string message) : base(message) { }

        public DocuSignServiceException(string message, Exception innerException) : base(message, innerException) { }

        protected DocuSignServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
