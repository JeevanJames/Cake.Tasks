namespace Cake.Tasks.Config
{
    public sealed class DotNetCoreConfig : PluginConfig
    {
        public DotNetCoreConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
            Build = new BuildConfig(taskConfig);
            Test = new TestConfig(taskConfig);
            Publish = new PublishConfig(taskConfig);
        }

        public BuildConfig Build { get; }

        public TestConfig Test { get; }

        public PublishConfig Publish { get; }

        public sealed class BuildConfig : PluginConfig
        {
            public BuildConfig(TaskConfig taskConfig)
                : base(taskConfig)
            {
            }

            public FuncOrValue<string> ProjectFile
            {
                get => Get<FuncOrValue<string>>(Keys.BuildProjectFile);
                set => Set(Keys.BuildProjectFile, value);
            }
        }

        public sealed class TestConfig : PluginConfig
        {
            public TestConfig(TaskConfig taskConfig)
                : base(taskConfig)
            {
            }

            public FuncOrValue<bool> Skip
            {
                get => Get<FuncOrValue<bool>>(Keys.TestSkip);
                set => Set(Keys.TestSkip, value);
            }

            public FuncOrValue<string> ProjectFile
            {
                get => Get<FuncOrValue<string>>(Keys.TestProjectFile);
                set => Set(Keys.TestProjectFile, value);
            }

            public FuncOrValue<string> Filter
            {
                get => Get<FuncOrValue<string>>(Keys.TestFilter);
                set => Set(Keys.TestFilter, value);
            }
        }

        public sealed class PublishConfig : PluginConfig
        {
            public PublishConfig(TaskConfig taskConfig)
                : base(taskConfig)
            {
            }

            public FuncOrValue<string> ProjectFile
            {
                get => Get<FuncOrValue<string>>(Keys.PublishProjectFile);
                set => Set(Keys.PublishProjectFile, value);
            }
        }

        public static class Keys
        {
            public const string BuildProjectFile = "DotNetCore_Build_ProjectFile";

            public const string TestSkip = "DotNetCore_Test_Skip";
            public const string TestProjectFile = "DotNetCore_Test_ProjectFile";
            public const string TestFilter = "DotNetCore_Test_Filter";

            public const string PublishProjectFile = "DotNetCore_Publish_ProjectFile";
        }
    }
}
