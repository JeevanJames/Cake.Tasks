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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Cake.Core;
using Cake.Core.Diagnostics;

namespace Cake.Tasks.Module
{
    public sealed partial class TasksEngine : ICakeEngine
    {
        private readonly ICakeEngine _engine;

        private ICakeLog Log { get; }

        private ICakeContext Context { get; }

        public TasksEngine(ICakeDataService dataService, ICakeLog log, ICakeContext context)
        {
            _engine = new CakeEngine(dataService, log);
            Log = log;
            Context = context;
        }

        public IReadOnlyList<ICakeTaskInfo> Tasks => _engine.Tasks;

        public event EventHandler<SetupEventArgs> Setup
        {
            add => _engine.Setup += value;
            remove => _engine.Setup -= value;
        }

        public event EventHandler<TeardownEventArgs> Teardown
        {
            add => _engine.Teardown += value;
            remove => _engine.Teardown -= value;
        }

        public event EventHandler<TaskSetupEventArgs> TaskSetup
        {
            add => _engine.TaskSetup += value;
            remove => _engine.TaskSetup -= value;
        }

        public event EventHandler<TaskTeardownEventArgs> TaskTeardown
        {
            add => _engine.TaskTeardown += value;
            remove => _engine.TaskTeardown -= value;
        }

        public void RegisterSetupAction(Action<ISetupContext> action)
        {
            _engine.RegisterSetupAction(action);
        }

        public void RegisterSetupAction<TData>(Func<ISetupContext, TData> action)
            where TData : class
        {
            _engine.RegisterSetupAction(action);
        }

        public CakeTaskBuilder RegisterTask(string name)
        {
            return _engine.RegisterTask(name);
        }

        public void RegisterTaskSetupAction(Action<ITaskSetupContext> action)
        {
            _engine.RegisterTaskSetupAction(action);
        }

        public void RegisterTaskSetupAction<TData>(Action<ITaskSetupContext, TData> action)
            where TData : class
        {
            _engine.RegisterTaskSetupAction(action);
        }

        public void RegisterTaskTeardownAction(Action<ITaskTeardownContext> action)
        {
            _engine.RegisterTaskTeardownAction(action);
        }

        public void RegisterTaskTeardownAction<TData>(Action<ITaskTeardownContext, TData> action)
            where TData : class
        {
            _engine.RegisterTaskTeardownAction(action);
        }

        public void RegisterTeardownAction(Action<ITeardownContext> action)
        {
            _engine.RegisterTeardownAction(action);
        }

        public void RegisterTeardownAction<TData>(Action<ITeardownContext, TData> action)
            where TData : class
        {
            _engine.RegisterTeardownAction(action);
        }

        public Task<CakeReport> RunTargetAsync(ICakeContext context, IExecutionStrategy strategy, ExecutionSettings settings)
        {
            InitializeCakeTasksSystem();
            return _engine.RunTargetAsync(context, strategy, settings);
        }
    }
}
