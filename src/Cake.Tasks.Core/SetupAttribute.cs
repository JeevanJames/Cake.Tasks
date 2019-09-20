using System;

namespace Cake.Tasks.Core
{
    public sealed class SetupAttribute : TaskAttribute
    {
        public SetupAttribute(string environment)
        {
            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentException("message", nameof(environment));
            Environment = environment;
        }
    }
}
