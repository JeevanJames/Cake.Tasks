namespace Cake.Tasks.Config
{
    public sealed class EnvConfig : PluginConfig
    {
        public EnvConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
        }

        public string Configuration
        {
            get => Get<string>(Keys.Configuration);
            set => Set(Keys.Configuration, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether Cake is running in a CI environment.
        /// </summary>
        public bool IsCi
        {
            get => Get<bool>(Keys.IsCi);
            set => Set(Keys.IsCi, value);
        }

        /// <summary>
        ///     Gets or sets the working directory.
        /// </summary>
        public string WorkingDirectory
        {
            get => Get<string>(Keys.WorkingDirectory);
            set => Set(Keys.WorkingDirectory, value);
        }

        public static class Keys
        {
            public const string Configuration = "Env_Configuration";
            public const string IsCi = "Env_IsCi";
            public const string WorkingDirectory = "Env_WorkingDirectory";
        }
    }
}
