using System;
using System.IO;
using System.Text.RegularExpressions;

using Cake.Core;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.Git;

[assembly: TaskPlugin(typeof(GitTasks))]

namespace Cake.Tasks.Git
{
    public static class GitTasks
    {
        [Config(Order = ConfigTaskOrder.Priority)]
        public static void ConfigureProjectNameFromGitRepoUrl(ICakeContext ctx, TaskConfig cfg)
        {
            EnvConfig env = cfg.Load<EnvConfig>();

            string gitDir = Path.Combine(env.Directories.Working, ".git");
            if (!Directory.Exists(gitDir))
            {
                ctx.LogInfo($"Working directory '{env.Directories.Working}' is not a Git repository. Cannot calculate project name from it.");
                return;
            }

            ctx.LogInfo($"Git directory found '{gitDir}'");

            string fetchHeadFile = Path.Combine(gitDir, "FETCH_HEAD");
            if (!File.Exists(fetchHeadFile))
            {
                ctx.LogInfo("Cannot retrieve remote URL for current repo.");
                return;
            }

            ctx.LogInfo($"FETCH_HEAD file found '{fetchHeadFile}'");

            using StreamReader reader = File.OpenText(fetchHeadFile);
            string firstLine = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(firstLine))
            {
                ctx.LogInfo("No content in FETCH_HEAD file");
                return;
            }

            ctx.LogInfo($"First line of FETCH_HEAD file: '{firstLine}'");

            Match match = RemoteUrlPattern.Match(firstLine);
            if (!match.Success)
            {
                ctx.LogInfo("Cannot retrieve remote URL for current repo.");
                return;
            }

            string matchedRemoteUri = match.Groups[1].Value;
            ctx.LogInfo($"URL portion: '{matchedRemoteUri}'");
            if (!Uri.TryCreate(matchedRemoteUri, UriKind.Absolute, out Uri remoteUri))
            {
                ctx.LogInfo($"The URL portion '{matchedRemoteUri}' is not a valid URL.");
                return;
            }

            const string ignoreExtension = ".git";
            string lastSegment = remoteUri.Segments[remoteUri.Segments.Length - 1];
            if (lastSegment.EndsWith(ignoreExtension, StringComparison.OrdinalIgnoreCase))
                lastSegment = lastSegment.Substring(0, lastSegment.Length - ignoreExtension.Length);

            ctx.LogInfo($"Setting project name to {lastSegment}.");
            env.Name = lastSegment;
        }

        private static readonly Regex RemoteUrlPattern = new(@"\s+branch '.+' of (.+)$", RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));
    }
}
