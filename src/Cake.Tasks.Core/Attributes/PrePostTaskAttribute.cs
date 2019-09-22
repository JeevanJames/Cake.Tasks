using System;

namespace Cake.Tasks.Core
{
    public abstract class PrePostTaskAttribute : TaskAttribute
    {
        protected PrePostTaskAttribute(CoreTask coreTask, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("message", nameof(name));
            Name = name;
        }

        public CoreTask CoreTask { get; }

        public string Name { get; }
    }
}
