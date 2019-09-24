using System;

namespace Cake.Tasks.Core
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

        public string Version
        {
            get => Get<string>(Keys.Version);
            set => Set(Keys.Version, value);
        }

        public string ArtifactsDirectory
        {
            get => Get<string>(Keys.ArtifactsDirectory);
            set => Set(Keys.ArtifactsDirectory, value);
        }

        public string Configuration
        {
            get => Get<string>(Keys.Configuration);
            set => Set(Keys.Configuration, value);
        }

        public static class Keys
        {
            public const string BuildNumber = "CI.BuildNumber";
            public const string Version = "CI.Version";
            public const string ArtifactsDirectory = "CI.ArtifactsDirectory";
            public const string Configuration = "CI.Configuration";
        }
    }
}
