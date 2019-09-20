using System;

namespace Cake.Tasks.Core
{
    public abstract class PrePostTaskAttribute : TaskAttribute
    {
        protected PrePostTaskAttribute(string coreTaskName, string name)
        {
            if (string.IsNullOrWhiteSpace(coreTaskName))
                throw new ArgumentException("message", nameof(coreTaskName));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("message", nameof(name));
            CoreTaskName = coreTaskName;
            Name = name;
        }

        public string CoreTaskName { get; }

        public string Name { get; }
    }
}
