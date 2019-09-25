using System;
using Cake.Core;
using Cake.Core.Annotations;

namespace Cake.Tasks.Core
{
    public static class Aliases
    {
        [CakeMethodAlias]
        [CakeNamespaceImport("Cake.Tasks.Core")]
        public static void Configuration(this ICakeContext ctx, Action<TaskConfig> setter)
        {
            if (ctx is null)
                throw new ArgumentNullException(nameof(ctx));
            if (setter is null)
                throw new ArgumentNullException(nameof(setter));

            // Don't run this setter now. Wait for all the config tasks to be called to initialize
            // the configs, and then call this to override custom values.
            // The setter is actually called by calling TaskConfig.Current.PerformDeferredSetup at
            // various places in the TasksEngine.
            TaskConfig.Current.SetDeferredSetup(setter);
        }
    }
}
