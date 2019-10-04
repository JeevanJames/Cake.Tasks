using System;

namespace Cake.Tasks.Config
{
    public sealed class CiConfig : PluginConfig
    {
        public CiConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
        }

        public int BuildNumber
        {
            get => Get<int>(Keys.BuildNumber);
            set => Set(Keys.BuildNumber, value);
        }

        public FuncOrValue<string> Version
        {
            get => Get<string>(Keys.Version);
            set => Set(Keys.Version, value);
        }

        public string ArtifactsDirectory
        {
            get => Get<string>(Keys.ArtifactsDirectory);
            set => Set(Keys.ArtifactsDirectory, value);
        }

        public string BuildOutputDirectory
        {
            get => Get<string>(Keys.BuildOutputDirectory);
            set => Set(Keys.BuildOutputDirectory, value);
        }

        public string TestOutputDirectory
        {
            get => Get<string>(Keys.TestOutputDirectory);
            set => Set(Keys.TestOutputDirectory, value);
        }

        public static class Keys
        {
            public const string BuildNumber = "CI_BuildNumber";
            public const string Version = "CI_Version";
            public const string ArtifactsDirectory = "CI_ArtifactsDirectory";
            public const string BuildOutputDirectory = "CI_BuildOutputDirectory";
            public const string TestOutputDirectory = "CI_TestOutputDirectory";
        }
    }
}
