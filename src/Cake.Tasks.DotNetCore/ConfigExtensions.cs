using Cake.Tasks.Core;

namespace Cake.Tasks.DotNetCore
{
    public static class ConfigExtensions
    {
        public static TaskConfig SetProjectFile(this TaskConfig config, string projectFile)
        {
            config.Set(Config.BuildProjectFiles, projectFile);
            config.Set(Config.TestProjectFiles, projectFile);
            return config;
        }
    }
}
