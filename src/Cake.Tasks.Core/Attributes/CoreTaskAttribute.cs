namespace Cake.Tasks.Core
{
    public sealed class CoreTaskAttribute : BasePipelineTaskAttribute
    {
        public CoreTaskAttribute(CoreTask coreTask)
        {
            CoreTask = coreTask;
        }

        public CoreTask CoreTask { get; }
    }
}
