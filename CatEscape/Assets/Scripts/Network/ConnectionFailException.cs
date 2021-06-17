using System;
using System.Runtime.Serialization;

namespace CatEscape.Network
{
    [Serializable]
    public class ConnectionFailException : Exception
    {
        public ReplyPacket.Reasons Reason { get; }

        public ConnectionFailException(ReplyPacket.Reasons reason)
        {
            Reason = reason;
        }

        protected ConnectionFailException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
