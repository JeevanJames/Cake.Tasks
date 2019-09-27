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

        public string BinaryArtifactsDirectory
        {
            get => Get<string>(Keys.BinaryArtifactsDirectory);
            set => Set(Keys.BinaryArtifactsDirectory, value);
        }

        public static class Keys
        {
            public const string BuildNumber = "CI_BuildNumber";
            public const string Version = "CI_Version";
            public const string ArtifactsDirectory = "CI_ArtifactsDirectory";
            public const string BinaryArtifactsDirectory = "CI_BinaryArtifactsDirectory";
        }
    }
}
