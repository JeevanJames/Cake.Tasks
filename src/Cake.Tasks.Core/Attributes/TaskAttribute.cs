using System;

namespace Cake.Tasks.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class TaskAttribute : Attribute
    {
        public TaskAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Specify a valid task name.", nameof(name));
            Name = name;
        }

        public string Name { get; }

        public string Description { get; set; }
    }
}
