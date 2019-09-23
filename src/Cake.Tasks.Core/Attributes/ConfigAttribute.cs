namespace Cake.Tasks.Core
{
    public sealed class ConfigAttribute : TaskAttribute
    {
        public ConfigAttribute(string environment = null)
        {
            Environment = environment;
        }

        public int Order { get; set; }
    }
}
