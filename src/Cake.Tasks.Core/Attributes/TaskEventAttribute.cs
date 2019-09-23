using System;

namespace Cake.Tasks.Core
{
    public sealed class TaskEventAttribute : TaskAttribute
    {
        public TaskEventAttribute(TaskEventType eventType, CoreTask coreTask)
        {
            EventType = eventType;
            CoreTask = coreTask;
        }

        public TaskEventType EventType { get; }

        public CoreTask CoreTask { get; }
    }

    public enum TaskEventType
    {
        BeforeTask,
        AfterTask,
    }
}
