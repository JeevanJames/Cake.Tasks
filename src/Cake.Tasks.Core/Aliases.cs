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

            setter(TaskConfig.Current);
        }
    }
}
