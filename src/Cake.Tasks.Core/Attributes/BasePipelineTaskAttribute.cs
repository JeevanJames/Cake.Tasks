using System;

namespace Cake.Tasks.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class BasePipelineTaskAttribute : Attribute
    {
        /// <summary>
        ///     Gets or sets the name of the CI system in which this task can be executed (TFS,
        ///     AppVeyor, etc.).
        ///     <para/>
        ///     If not specified, this task will always be executed.
        /// </summary>
        public string CiSystem { get; set; }
    }
}
