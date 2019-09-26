using System;

using Cake.Core;
using Cake.Core.Annotations;

namespace Cake.Tasks.Config
{
    public static class Aliases
    {
        [CakeMethodAlias]
        [CakeNamespaceImport("Cake.Tasks.Config")]
        public static void ConfigureTasks(this ICakeContext ctx, Action<TaskConfig> setter)
        {
            if (ctx is null)
                throw new ArgumentNullException(nameof(ctx));
            if (setter is null)
                throw new ArgumentNullException(nameof(setter));

            // Don't run this setter now. Wait for all the config tasks to be called to initialize
            // the configs, and then call this to override with custom values.
            // The setter is actually called by calling TaskConfig.Current.PerformDeferredSetup at
            // various places in the TasksEngine.
            TaskConfig.Current.SetDeferredSetup(setter);
        }

        [CakeMethodAlias]
        [CakeNamespaceImport("Cake.Tasks.Config")]
        public static void ConfigureTask<TConfig>(this ICakeContext ctx, Action<TConfig> setter)
            where TConfig : PluginConfig
        {
            if (setter is null)
                throw new ArgumentNullException(nameof(setter));

            ConfigureTasks(ctx, config =>
            {
                var cfg = config.Load<TConfig>();
                setter(cfg);
            });
        }
    }
}
