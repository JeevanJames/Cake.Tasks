using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Tasks.Module;

[assembly: CakeModule(typeof(TasksModule))]

namespace Cake.Tasks.Module
{
    public sealed class TasksModule : ICakeModule
    {
        public void Register(ICakeContainerRegistrar registrar)
        {
            registrar.RegisterType<TasksEngine>().As<ICakeEngine>().Singleton();
        }
    }
}
