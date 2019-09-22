namespace Cake.Tasks.Core
{
    public sealed class PreTaskAttribute : PrePostTaskAttribute
    {
        public PreTaskAttribute(CoreTask coreTask, string name)
            : base(coreTask, name)
        {
        }
    }
}
