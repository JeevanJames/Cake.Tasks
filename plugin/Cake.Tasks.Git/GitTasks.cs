using System;
using System.Linq;

using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.Git;

using LibGit2Sharp;

[assembly: TaskPlugin(typeof(GitTasks))]

namespace Cake.Tasks.Git
{
    public static class GitTasks
    {
        [Config(Order = ConfigTaskOrder.Priority)]
        public static void ConfigureGit(ICakeContext ctx, TaskConfig cfg)
        {
            EnvConfig env = cfg.Load<EnvConfig>();

            using var repo = new Repository(env.Directories.Working);

            Remote remote = repo.Network.Remotes.FirstOrDefault(r => r.Name.Equals("origin", StringComparison.OrdinalIgnoreCase));
            if (remote is null)
            {
                ctx.Log.Information("[Project Name] Cannot find origin remote. Attempting to look for the first available remote.");
                remote = repo.Network.Remotes.FirstOrDefault();
            }

            if (remote is null)
            {
                ctx.Log.Warning("Could not find any remotes in the Git repository.");
                return;
            }

            ctx.Log.Information($"[Project Name] Using remote {remote.Name} at {remote.Url}.");

            if (!Uri.TryCreate(remote.Url, UriKind.Absolute, out Uri remoteUrl))
            {
                ctx.Log.Warning($"[Project Name] Cannot recognize URI of remote {remote.Name} - {remote.Url}.");
                return;
            }

            const string ignoreExtension = ".git";
            string name = remoteUrl.Segments[remoteUrl.Segments.Length - 1];
            if (name.EndsWith(ignoreExtension, StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - ignoreExtension.Length);
            name = name.ToLowerInvariant()
                .Replace('.', '-')
                .Replace(' ', '-')
                .Replace('_', '-');

            ctx.Log.Information($"[Project Name] Setting project name to {name}.");
            env.Name = name;
        }
    }
}
