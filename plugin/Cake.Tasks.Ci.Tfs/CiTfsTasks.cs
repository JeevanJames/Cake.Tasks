// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;

using Cake.Common.Build;
using Cake.Common.Build.AzurePipelines;
using Cake.Common.Build.AzurePipelines.Data;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Tasks.Ci.Tfs;
using Cake.Tasks.Config;
using Cake.Tasks.Core;

using IO = System.IO;

[assembly: TaskPlugin(typeof(CiTfsTasks))]

namespace Cake.Tasks.Ci.Tfs;

public static class CiTfsTasks
{
    [TeardownTask(CiSystem = "tfs", ContinueOnError = true)]
    public static void PublishTestResults(ICakeContext ctx, TaskConfig cfg)
    {
        IAzurePipelinesProvider azurePipelines = ctx.AzurePipelines();
        if (!azurePipelines.IsRunningOnAzurePipelines && !azurePipelines.IsRunningOnAzurePipelinesHosted)
            return;

        EnvConfig env = cfg.Load<EnvConfig>();
        string[] ioTrxFiles = IO.Directory.GetFiles(env.Directories.Working, "*.trx", IO.SearchOption.AllDirectories);
        ctx.LogInfo($"TRX test result files found (via IO): {ioTrxFiles.Length}");

        FilePathCollection trxFiles = ctx.GetFiles("./**/*.trx");
        ctx.LogInfo($"TRX test result files found: {trxFiles.Count}");
        if (trxFiles.Count == 0)
            return;

        int index = 0;
        foreach (FilePath trxFile in trxFiles)
        {
            string sourceFile = trxFile.FullPath;
            string directory = IO.Path.GetDirectoryName(sourceFile);
            string renamedFile = IO.Path.Combine(directory, $"TestResults{index}.trx");
            IO.File.Move(sourceFile, renamedFile);
            index++;
        }

        FilePathCollection testResultsFiles = ctx.GetFiles("./**/*.trx");
        foreach (FilePath filePath in testResultsFiles)
            ctx.Log.Information(filePath.ToString());

        azurePipelines.Commands.PublishTestResults(new AzurePipelinesPublishTestResultsData
        {
            TestRunner = AzurePipelinesTestRunnerType.VSTest,
            TestResultsFiles = testResultsFiles.ToList(),
            MergeTestResults = true,
        });
    }

    [TeardownTask(CiSystem = "tfs", ContinueOnError = true)]
    public static void PublishArtifacts(ICakeContext ctx, TaskConfig cfg)
    {
        IAzurePipelinesProvider azurePipelines = ctx.AzurePipelines();
        if (!azurePipelines.IsRunningOnAzurePipelines && !azurePipelines.IsRunningOnAzurePipelinesHosted)
            return;

        EnvConfig env = cfg.Load<EnvConfig>();
        if (!IO.Directory.Exists(env.Directories.Artifacts))
            return;

        IEnumerable<string> artifactDirs = IO.Directory
            .EnumerateDirectories(env.Directories.Artifacts, "*", IO.SearchOption.TopDirectoryOnly);
        foreach (string artifactDir in artifactDirs)
        {
            string artifactName = IO.Path.GetFileName(artifactDir);
            ctx.LogInfo($"Uploading artifact directory {artifactDir} as {artifactName}");
            azurePipelines.Commands.UploadArtifactDirectory(artifactDir, artifactName);
        }

        IEnumerable<string> artifactFiles = IO.Directory
            .EnumerateFiles(env.Directories.Artifacts, "*", IO.SearchOption.TopDirectoryOnly);
        foreach (string artifactFile in artifactFiles)
        {
            string artifactName = IO.Path.GetFileName(artifactFile);
            ctx.LogInfo($"Uploading artifact file {artifactFile} as {artifactName}");
            azurePipelines.Commands.UploadArtifact("__artifacts", artifactFile, artifactName);
        }
    }

    [Config(CiSystem = "tfs", Order = ConfigTaskOrder.CiSystem)]
    public static void ConfigureTfsEnvironment(ICakeContext ctx, TaskConfig cfg)
    {
        IAzurePipelinesProvider azurePipelines = ctx.AzurePipelines();
        ctx.LogInfo(azurePipelines.Dump());
        ctx.LogInfo(azurePipelines.Environment.Repository.Dump());

        EnvConfig env = cfg.Load<EnvConfig>();
        env.IsCi = true;
        env.Version.BuildNumber = azurePipelines.Environment.Build.Number;
    }
}
