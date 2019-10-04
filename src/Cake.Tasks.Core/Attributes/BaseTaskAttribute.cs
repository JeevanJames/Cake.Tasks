using System;

namespace Cake.Tasks.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class BaseTaskAttribute : Attribute
    {
        /// <summary>
        ///     Gets or sets the name of the environment in which this task can be executed (TFS,
        ///     AppVeyor, etc.).
        ///     <para/>
        ///     If not specified, this task will always be executed.
        /// </summary>
        public string Environment { get; set; }
    }
}
