namespace Cake.Tasks.Core
{
    /// <summary>
    ///     Marks a method as a configuration task.
    /// </summary>
    public sealed class ConfigAttribute : BasePipelineTaskAttribute
    {
        public ConfigAttribute(string environment = null)
        {
            CiSystem = environment;
        }

        public int Order { get; set; }
    }
}
