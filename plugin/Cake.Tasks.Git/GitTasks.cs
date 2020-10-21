using System;
using System.IO;
using System.Text.RegularExpressions;

using Cake.Core;
using Cake.Core.Diagnostics;
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
                ctx.Log.Information($"[{nameof(ConfigureProjectNameFromGitRepoUrl)}] Working directory '{env.Directories.Working}' is not a Git repository. Cannot calculate project name from it.");
                return;
            }

            string fetchHeadFile = Path.Combine(gitDir, "FETCH_HEAD");
            if (!File.Exists(fetchHeadFile))
            {
                ctx.Log.Information($"[{nameof(ConfigureProjectNameFromGitRepoUrl)}] Cannot retrieve remote URL for current repo.");
                return;
            }

            using StreamReader reader = File.OpenText(fetchHeadFile);
            string firstLine = reader.ReadLine();
            Match match = RemoteUrlPattern.Match(firstLine);
            if (!match.Success)
            {
                ctx.Log.Information($"[{nameof(ConfigureProjectNameFromGitRepoUrl)}] Cannot retrieve remote URL for current repo.");
                return;
            }

            string matchedRemoteUri = match.Groups[1].Value;
            if (!Uri.TryCreate(matchedRemoteUri, UriKind.Absolute, out Uri remoteUri))
            {
                ctx.Log.Information($"[{nameof(ConfigureProjectNameFromGitRepoUrl)}] Cannot retrieve remote URL for current repo.");
                return;
            }

            const string ignoreExtension = ".git";
            string lastSegment = remoteUri.Segments[remoteUri.Segments.Length - 1];
            if (lastSegment.EndsWith(ignoreExtension, StringComparison.OrdinalIgnoreCase))
                lastSegment = lastSegment.Substring(0, lastSegment.Length - ignoreExtension.Length);

            ctx.Log.Information($"[{nameof(ConfigureProjectNameFromGitRepoUrl)}] Setting project name to {lastSegment}.");
            env.Name = lastSegment;
        }

        private static readonly Regex RemoteUrlPattern = new Regex(@"\s+branch '.+' of (.+)$", RegexOptions.Compiled);
    }
}
