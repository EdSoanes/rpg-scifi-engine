using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Json
{
    public class JOpException : Exception
    {
        public JOpException()
            : base()
        { }

        public JOpException(string? msg)
            : base(msg)
        { }

        public JOpException(string? msg, Exception? innerException)
            : base(msg, innerException)
        { }

        public JOpException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
