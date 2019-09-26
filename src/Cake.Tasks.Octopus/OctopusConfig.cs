namespace Cake.Tasks.Config
{
    public sealed class OctopusConfig : PluginConfig
    {
        public OctopusConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
            Pack = new PackConfig(taskConfig);
            Release = new ReleaseConfig(taskConfig);
        }

        public FuncOrValue<string> Server
        {
            get => Get<FuncOrValue<string>>(Keys.Server);
            set => Set(Keys.Server, value);
        }

        public FuncOrValue<string> ApiKey
        {
            get => Get<FuncOrValue<string>>(Keys.ApiKey);
            set => Set(Keys.ApiKey, value);
        }

        public FuncOrValue<string> PackageId
        {
            get => Get<FuncOrValue<string>>(Keys.PackageId);
            set => Set(Keys.PackageId, value);
        }

        public PackConfig Pack { get; }

        public ReleaseConfig Release { get; }

        public sealed class PackConfig : PluginConfig
        {
            public PackConfig(TaskConfig taskConfig)
                : base(taskConfig)
            {
            }

            public FuncOrValue<string> BasePath
            {
                get => Get<FuncOrValue<string>>(Keys.PackBasePath);
                set => Set(Keys.PackBasePath, value);
            }

            public FuncOrValue<string> OutFolder
            {
                get => Get<FuncOrValue<string>>(Keys.PackOutFolder);
                set => Set(Keys.PackOutFolder, value);
            }

            public FuncOrValue<string> Version
            {
                get => Get<FuncOrValue<string>>(Keys.PackVersion);
                set => Set(Keys.PackVersion, value);
            }
        }

        public sealed class ReleaseConfig : PluginConfig
        {
            public ReleaseConfig(TaskConfig taskConfig)
                : base(taskConfig)
            {
            }

            public FuncOrValue<string> ProjectName
            {
                get => Get<FuncOrValue<string>>(Keys.ReleaseProjectName);
                set => Set(Keys.ReleaseProjectName, value);
            }

            public FuncOrValue<string> DeployTo
            {
                get => Get<FuncOrValue<string>>(Keys.ReleaseDeployTo);
                set => Set(Keys.ReleaseDeployTo, value);
            }
        }

        public static class Keys
        {
            public const string Server = "Octopus_Server";
            public const string ApiKey = "Octopus_ApiKey";
            public const string PackageId = "Octopus_PackageId";

            public const string PackBasePath = "Octopus_Pack_BasePath";
            public const string PackOutFolder = "Octopus_Pack_OutFolder";
            public const string PackVersion = "Octopus_Pack_Version";

            public const string ReleaseProjectName = "Octopus_Release_ProjectName";
            public const string ReleaseDeployTo = "Octopus_Release_DeployTo";
        }
    }
}
