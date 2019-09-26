using Cake.Common.Tools.DotNetCore.Build;
using Cake.Common.Tools.DotNetCore.Test;

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

            public FuncOrListValue<string> ProjectFiles
            {
                get => Get<FuncOrListValue<string>>(Keys.BuildProjectFiles);
                set => Set(Keys.BuildProjectFiles, value);
            }

            public FuncOrValue<bool> NoRestore
            {
                get => Get<FuncOrValue<bool>>(Keys.BuildNoRestore);
                set => Set(Keys.BuildNoRestore, value);
            }

            public FuncOrValue<DotNetCoreBuildSettings> Settings
            {
                get => Get<FuncOrValue<DotNetCoreBuildSettings>>(Keys.BuildSettings);
                set => Set(Keys.BuildSettings, value);
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

            public FuncOrListValue<string> ProjectFiles
            {
                get => Get<FuncOrListValue<string>>(Keys.TestProjectFiles);
                set => Set(Keys.TestProjectFiles, value);
            }

            public FuncOrValue<bool> NoRestore
            {
                get => Get<FuncOrValue<bool>>(Keys.TestNoRestore);
                set => Set(Keys.TestNoRestore, value);
            }

            public FuncOrValue<bool> NoBuild
            {
                get => Get<FuncOrValue<bool>>(Keys.TestNoBuild);
                set => Set(Keys.TestNoBuild, value);
            }

            public FuncOrValue<DotNetCoreTestSettings> Settings
            {
                get => Get<FuncOrValue<DotNetCoreTestSettings>>(Keys.TestSettings);
                set => Set(Keys.TestSettings, value);
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
            public const string BuildProjectFiles = "DotNetCore_Build_ProjectFiles";
            public const string BuildNoRestore = "DotNetCore_Build_NoRestore";
            public const string BuildSettings = "DotNetCore_Build_Settings";

            public const string TestSkip = "DotNetCore_Test_Skip";
            public const string TestProjectFiles = "DotNetCore_Test_ProjectFiles";
            public const string TestNoRestore = "DotNetCore_Test_NoRestore";
            public const string TestNoBuild = "DotNetCore_Test_NoBuild";
            public const string TestSettings = "DotNetCore_Test_Settings";

            public const string PublishProjectFile = "DotNetCore_Deploy_ProjectFile";
        }
    }
}
