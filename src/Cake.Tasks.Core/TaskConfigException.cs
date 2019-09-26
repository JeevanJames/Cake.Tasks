using System;
using System.Runtime.Serialization;

namespace Cake.Tasks.Core
{
    [Serializable]
    public sealed class TaskConfigException : Exception
    {
        public TaskConfigException()
        {
        }

        public TaskConfigException(string message)
            : base(message)
        {
        }

        public TaskConfigException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private TaskConfigException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
