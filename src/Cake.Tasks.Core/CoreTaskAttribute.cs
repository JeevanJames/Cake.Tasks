using System;

namespace Cake.Tasks.Core
{
    public sealed class CoreTaskAttribute : TaskAttribute
    {
        public CoreTaskAttribute(string coreTaskName)
        {
            if (string.IsNullOrWhiteSpace(coreTaskName))
                throw new ArgumentException("message", nameof(coreTaskName));
            CoreTaskName = coreTaskName;
        }

        public string CoreTaskName { get; }
    }
}
