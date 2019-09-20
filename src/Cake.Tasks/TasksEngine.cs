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

        public ICakeLog Log { get; }

        public ICakeContext Context { get; }

        public TasksEngine(ICakeDataService dataService, ICakeLog log, ICakeContext context)
        {
            _engine = new CakeEngine(dataService, log);
            Log = log;
            Context = context;

            RegisterPlugins();
        }

        public IReadOnlyList<ICakeTaskInfo> Tasks => _engine.Tasks;

        public event EventHandler<SetupEventArgs> Setup
        {
            add { _engine.Setup += value; }
            remove { _engine.Setup -= value; }
        }

        public event EventHandler<TeardownEventArgs> Teardown
        {
            add { _engine.Teardown += value; }
            remove { _engine.Teardown -= value; }
        }

        public event EventHandler<TaskSetupEventArgs> TaskSetup
        {
            add { _engine.TaskSetup += value; }
            remove { _engine.TaskSetup -= value; }
        }

        public event EventHandler<TaskTeardownEventArgs> TaskTeardown
        {
            add { _engine.TaskTeardown += value; }
            remove { _engine.TaskTeardown -= value; }
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
            return _engine.RunTargetAsync(context, strategy, settings);
        }
    }
}
