using System;
using System.Collections.Generic;
using System.Text;

namespace Cake.Tasks.Core
{
    public sealed class EnvConfig : PluginConfig
    {
        public EnvConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
        }

        public string WorkingDirectory
        {
            get => Get<string>(Keys.WorkingDirectory);
            set => Set(Keys.WorkingDirectory, value);
        }

        public static class Keys
        {
            public const string WorkingDirectory = "Env.WorkingDirectory";
        }
    }
}
