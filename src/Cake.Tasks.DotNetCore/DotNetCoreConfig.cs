using Cake.Tasks.Core;

namespace Cake.Tasks.DotNetCore
{
    public sealed class DotNetCoreConfig : PluginConfig
    {
        public DotNetCoreConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
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
        }
    }
}
