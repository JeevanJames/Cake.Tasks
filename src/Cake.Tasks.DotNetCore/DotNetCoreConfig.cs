namespace Cake.Tasks.Config
{
    public sealed class DotNetCoreConfig : PluginConfig
    {
        public DotNetCoreConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
        }

        public FuncOrValue<bool> SkipTests
        {
            get => Get<FuncOrValue<bool>>(Keys.SkipTests);
            set => Set(Keys.SkipTests, value);
        }

        public FuncOrListValue<string> BuildProjectFiles
        {
            get => Get<FuncOrListValue<string>>(Keys.BuildProjectFiles);
            set => Set(Keys.BuildProjectFiles, value);
        }

        public FuncOrListValue<string> TestProjectFiles
        {
            get => Get<FuncOrListValue<string>>(Keys.TestProjectFiles);
            set => Set(Keys.TestProjectFiles, value);
        }

        public static class Keys
        {
            public const string BuildProjectFiles = "DotNetCore.BuildProjectFiles";
            public const string TestProjectFiles = "DotNetCore.TestProjectFiles";
            public const string SkipTests = "DotNetCore.SkipTests";
        }
    }
}
