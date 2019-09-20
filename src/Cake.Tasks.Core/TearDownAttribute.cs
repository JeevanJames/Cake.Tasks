using System;

namespace Cake.Tasks.Core
{
    public sealed class TearDownAttribute : TaskAttribute
    {
        public TearDownAttribute(string environment)
        {
            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentException("message", nameof(environment));
            Environment = environment;
        }
    }
}
