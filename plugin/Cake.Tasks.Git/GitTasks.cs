// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Text.RegularExpressions;

using Cake.Core;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.Git;

[assembly: TaskPlugin(typeof(GitTasks))]

namespace Cake.Tasks.Git;

public static class GitTasks
{
    [Config(Order = 0)]
    public static void ConfigureProjectNameFromGitRepoUrl(ICakeContext ctx, TaskConfig cfg)
    {
        string errorMessage = TryGetRemoteUrl(ctx, cfg, out Uri remoteUri);
        if (errorMessage is not null)
        {
            ctx.LogInfo(errorMessage);
            return;
        }

        const string ignoreExtension = ".git";
        string lastSegment = remoteUri.Segments[remoteUri.Segments.Length - 1];
        if (lastSegment.EndsWith(ignoreExtension, StringComparison.OrdinalIgnoreCase))
            lastSegment = lastSegment.Substring(0, lastSegment.Length - ignoreExtension.Length);

        ctx.LogInfo($"Setting project name to {lastSegment}.");

        EnvConfig env = cfg.Load<EnvConfig>();
        env.Name = lastSegment;
        env.Repository.Name = lastSegment;
        env.Repository.Url = remoteUri.ToString();
        env.Repository.Type = "git";

        //TODO: Get the branch and commit details here
    }

    private static string TryGetRemoteUrl(ICakeContext ctx, TaskConfig cfg, out Uri remoteUri)
    {
        remoteUri = null;

        EnvConfig env = cfg.Load<EnvConfig>();

        string gitDir = Path.Combine(env.Directories.Working, ".git");
        if (!Directory.Exists(gitDir))
            return $"Working directory '{env.Directories.Working}' is not a Git directory. Could not find the .git directory.";

        string configFile = Path.Combine(gitDir, "config");
        if (!File.Exists(configFile))
            return $"Could not find config file under '{gitDir}'.";

        string[] configLines = File.ReadAllLines(configFile);
        for (int i = 0; i < configLines.Length; i++)
        {
            string configLine = configLines[i];

            Match remoteMatch = RemotePattern.Match(configLine);
            if (!remoteMatch.Success)
                continue;

            string remoteName = remoteMatch.Groups[1].Value;
            ctx.LogInfo($"Found remote {remoteName}");

            Match urlMatch = UrlPattern.Match(configLines[i + 1]);
            if (!urlMatch.Success)
                return $"Could not find the URL for the remote {remoteName}";

            if (!Uri.TryCreate(urlMatch.Groups[1].Value, UriKind.Absolute, out remoteUri))
                return $"The URL for remote {remoteName} - {urlMatch.Groups[1].Value} - is not a valid URI.";

            return null;
        }

        return "Could not find any remotes.";
    }

    private static readonly Regex RemotePattern = new(@"^\[remote ""(\w+)""\]$", RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));

    private static readonly Regex UrlPattern = new(@"^\s*url\s*=\s*(.+)$", RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));
}
