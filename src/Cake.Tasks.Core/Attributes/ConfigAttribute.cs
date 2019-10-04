namespace Cake.Tasks.Core
{
    /// <summary>
    ///     Marks a method as a configuration task.
    /// </summary>
    public sealed class ConfigAttribute : BaseTaskAttribute
    {
        public ConfigAttribute(string environment = null)
        {
            Environment = environment;
        }

        public int Order { get; set; }
    }
}
