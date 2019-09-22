namespace Cake.Tasks.Core
{
    public sealed class PostTaskAttribute : PrePostTaskAttribute
    {
        public PostTaskAttribute(CoreTask coreTask, string name)
            : base(coreTask, name)
        {
        }
    }
}
