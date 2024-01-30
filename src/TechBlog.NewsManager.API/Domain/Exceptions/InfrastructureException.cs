using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace TechBlog.NewsManager.API.Domain.Exceptions
{
    public class InfrastructureException : Exception
    {
        public InfrastructureException(string message) : base(message) { }
        protected InfrastructureException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}