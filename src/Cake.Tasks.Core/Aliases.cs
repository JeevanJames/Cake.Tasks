using System;
using Cake.Core;

namespace Cake.Tasks.Core
{
    public static class Aliases
    {
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
