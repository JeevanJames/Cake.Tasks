using System;

namespace Cake.Tasks.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class TaskAttribute : Attribute
    {
        public string Environment { get; set; }
    }
}
