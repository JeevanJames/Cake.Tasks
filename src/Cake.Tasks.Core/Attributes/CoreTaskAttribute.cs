namespace Cake.Tasks.Core
{
    public sealed class CoreTaskAttribute : BaseTaskAttribute
    {
        public CoreTaskAttribute(CoreTask coreTask)
        {
            CoreTask = coreTask;
        }

        public CoreTask CoreTask { get; }
    }
}
