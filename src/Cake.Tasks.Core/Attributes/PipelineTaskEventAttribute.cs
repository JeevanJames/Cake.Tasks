// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

namespace Cake.Tasks.Core;

public abstract class PipelineTaskEventAttribute : BasePipelineTaskAttribute
{
    protected PipelineTaskEventAttribute(PipelineTask pipelineTask)
    {
        PipelineTask = pipelineTask;
    }

    public PipelineTask PipelineTask { get; }
}

public sealed class BeforePipelineTaskAttribute : PipelineTaskEventAttribute
{
    public BeforePipelineTaskAttribute(PipelineTask pipelineTask)
        : base(pipelineTask)
    {
    }
}

public sealed class AfterPipelineTaskAttribute : PipelineTaskEventAttribute
{
    public AfterPipelineTaskAttribute(PipelineTask pipelineTask)
        : base(pipelineTask)
    {
    }
}

internal enum TaskEventType
{
    BeforeTask,
    AfterTask,
}
