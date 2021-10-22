﻿#region --- License & Copyright Notice ---
/*
Cake Tasks
Copyright 2019 Jeevan James

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

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

namespace Cake.Tasks.Ci.Tfs
{
    public static class CiTfsTasks
    {
        [AfterPipelineTask(PipelineTask.Test, CiSystem = "tfs", ContinueOnError = true)]
        public static void PublishTestResults(ICakeContext ctx)
        {
            IAzurePipelinesProvider azurePipelines = ctx.AzurePipelines();
            if (!azurePipelines.IsRunningOnAzurePipelines || !azurePipelines.IsRunningOnAzurePipelinesHosted)
                return;

            FilePathCollection trxFiles = ctx.GetFiles("./**/*.trx");
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

        [Config(CiSystem = "tfs", Order = ConfigTaskOrder.CiSystem)]
        public static void ConfigureTfsEnvironment(ICakeContext ctx, TaskConfig cfg)
        {
            IAzurePipelinesProvider azurePipelines = ctx.AzurePipelines();
            ctx.LogInfo(azurePipelines.Dump());

            //if (!azurePipelines.IsRunningOnAzurePipelines || !azurePipelines.IsRunningOnAzurePipelinesHosted)
            //    throw new TaskConfigException("Not running in Azure Pipelines");

            EnvConfig env = cfg.Load<EnvConfig>();
            env.IsCi = true;

            // If the build number is configured as an integer, use it. Otherwise use the build ID.
            // Basically, we need something unique.
            env.Version.BuildNumber = azurePipelines.Environment.Build.Number;
        }
    }
}
