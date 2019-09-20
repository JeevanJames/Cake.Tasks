namespace Cake.Tasks.Core
{
    public sealed class PostTaskAttribute : PrePostTaskAttribute
    {
        public PostTaskAttribute(string coreTaskName, string name)
            : base(coreTaskName, name)
        {
        }
    }
}
