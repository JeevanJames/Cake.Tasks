#region --- License & Copyright Notice ---
/*
Cake Tasks
Copyright 2019 Jeevan James

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

namespace Cake.Tasks.Core
{
    public abstract class PipelineTaskEventAttribute : BasePipelineTaskAttribute
    {
        protected PipelineTaskEventAttribute(PipelineTask pipelineTask)
        {
            PipelineTask = pipelineTask;
        }

        public PipelineTask PipelineTask { get; }

        internal TaskEventType EventType { get; set; }
    }

    public sealed class BeforePipelineTaskAttribute : PipelineTaskEventAttribute
    {
        public BeforePipelineTaskAttribute(PipelineTask pipelineTask)
            : base(pipelineTask)
        {
            EventType = TaskEventType.BeforeTask;
        }
    }

    public sealed class AfterPipelineTaskAttribute : PipelineTaskEventAttribute
    {
        public AfterPipelineTaskAttribute(PipelineTask pipelineTask)
            : base(pipelineTask)
        {
            EventType = TaskEventType.AfterTask;
        }
    }

    internal enum TaskEventType
    {
        BeforeTask,
        AfterTask,
    }
}
