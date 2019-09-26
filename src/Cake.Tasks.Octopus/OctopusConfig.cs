namespace Cake.Tasks.Config
{
    public sealed class OctopusConfig : PluginConfig
    {
        public OctopusConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
            Pack = new PackConfig(taskConfig);
        }

        public FuncOrValue<string> PackageId
        {
            get => Get<FuncOrValue<string>>(Keys.PackageId);
            set => Set(Keys.PackageId, value);
        }

        public PackConfig Pack { get; }

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

        public static class Keys
        {
            public const string PackageId = "Octopus_PackageId";

            public const string PackBasePath = "Octopus_Pack_BasePath";
            public const string PackOutFolder = "Octopus_Pack_OutFolder";
            public const string PackVersion = "Octopus_Pack_Version";
        }
    }
}
