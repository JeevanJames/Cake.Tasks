namespace Cake.Tasks.Core
{
    public sealed class CoreTaskAttribute : TaskAttribute
    {
        public CoreTaskAttribute(CoreTask coreTask)
        {
            CoreTask = coreTask;
        }

        public CoreTask CoreTask { get; }
    }
}
