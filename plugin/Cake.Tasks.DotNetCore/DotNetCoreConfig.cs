﻿namespace Cake.Tasks.Config
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

            public ConfigValue<string> ProjectFile
            {
                get => Get<ConfigValue<string>>(Keys.BuildProjectFile);
                set => Set(Keys.BuildProjectFile, value);
            }
        }

        public sealed class TestConfig : PluginConfig
        {
            public TestConfig(TaskConfig taskConfig)
                : base(taskConfig)
            {
            }

            public ConfigValue<bool> Skip
            {
                get => Get<ConfigValue<bool>>(Keys.TestSkip);
                set => Set(Keys.TestSkip, value);
            }

            public ConfigValue<string> ProjectFile
            {
                get => Get<ConfigValue<string>>(Keys.TestProjectFile);
                set => Set(Keys.TestProjectFile, value);
            }

            public ConfigValue<string> Filter
            {
                get => Get<ConfigValue<string>>(Keys.TestFilter);
                set => Set(Keys.TestFilter, value);
            }
        }

        public sealed class PublishConfig : PluginConfig
        {
            public PublishConfig(TaskConfig taskConfig)
                : base(taskConfig)
            {
            }

            public ConfigValue<string> ProjectFile
            {
                get => Get<ConfigValue<string>>(Keys.PublishProjectFile);
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
