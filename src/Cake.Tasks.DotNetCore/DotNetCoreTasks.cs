using System;
using System.Collections.Generic;
using System.IO;
using Cake.Common.Tools.DotNetCore;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Tasks.Core;
using Cake.Tasks.DotNetCore;

[assembly: TaskPlugin(typeof(DotNetCoreTasks))]

namespace Cake.Tasks.DotNetCore
{
    public static class DotNetCoreTasks
    {
        [CoreTask(CoreTask.Build)]
        public static void Build(this ICakeContext context, TaskConfig config)
        {
            IList<string> buildProjectFiles = config.ResolveAsList<string>(Config.BuildProjectFiles);
            if (buildProjectFiles.Count == 0)
            {
                context.Log.Warning("No solution or project files found to build.");
                return;
            }

            foreach (string buildProjectFile in buildProjectFiles)
                context.DotNetCoreBuild(buildProjectFile);
        }

        [CoreTask(CoreTask.Test)]
        public static void Test(this ICakeContext context, TaskConfig config)
        {
            IList<string> testProjectFiles = config.ResolveAsList<string>(Config.TestProjectFiles);
            if (testProjectFiles.Count == 0)
            {
                context.Log.Warning("No solution or project files found to test.");
                return;
            }

            foreach (string testProjectFile in testProjectFiles)
                context.DotNetCoreBuild(testProjectFile);
        }

        [Config]
        public static void Configuration(this ICakeContext context, TaskConfig config)
        {
            object GetProjectFiles()
            {
                string workingDirectory = config.Resolve("ENV_WorkingDirectory", Directory.GetCurrentDirectory());
                string[] projectFiles = Directory.GetFiles(workingDirectory, "*.sln", SearchOption.TopDirectoryOnly);
                if (projectFiles.Length == 0)
                    projectFiles = Directory.GetFiles(workingDirectory, "*.sln", SearchOption.AllDirectories);
                if (projectFiles.Length == 0)
                    projectFiles = Directory.GetFiles(workingDirectory, "*.csproj", SearchOption.AllDirectories);
                if (projectFiles.Length > 1)
                    return projectFiles;
                if (projectFiles.Length == 1)
                    return projectFiles[0];
                return null;
            }

            config.Register(Config.BuildProjectFiles, (Func<object>)GetProjectFiles);
        }
    }
}
