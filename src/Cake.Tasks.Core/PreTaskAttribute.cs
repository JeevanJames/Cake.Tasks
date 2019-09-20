namespace Cake.Tasks.Core
{
    public sealed class PreTaskAttribute : PrePostTaskAttribute
    {
        public PreTaskAttribute(string coreTaskName, string name)
            : base(coreTaskName, name)
        {
        }
    }
}
