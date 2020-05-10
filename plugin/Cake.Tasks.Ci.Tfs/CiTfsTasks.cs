#region --- License & Copyright Notice ---
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
using Cake.Common.Build.TFBuild;
using Cake.Common.Build.TFBuild.Data;
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
        public static void PublishTestResults(ICakeContext ctx, TaskConfig cfg)
        {
            ITFBuildProvider tfs = ctx.TFBuild();

            FilePathCollection trxFiles = ctx.GetFiles("./**/*.trx");
            for (int i = 0; i < trxFiles.Count; i++)
            {
                string sourceFile = trxFiles.ElementAt(i).FullPath;
                string directory = IO.Path.GetDirectoryName(sourceFile);
                string renamedFile = IO.Path.Combine(directory, $"TestResults{i}.trx");
                IO.File.Move(sourceFile, renamedFile);
            }

            FilePathCollection testResultsFiles = ctx.GetFiles("./**/*.trx");
            foreach (FilePath filePath in testResultsFiles)
                ctx.Log.Information(filePath.ToString());

            tfs.Commands.PublishTestResults(new TFBuildPublishTestResultsData
            {
                TestRunner = TFTestRunnerType.VSTest,
                TestResultsFiles = testResultsFiles.ToList(),
                MergeTestResults = true,
            });
        }

        [Config(CiSystem = "tfs", Order = ConfigTaskOrder.CiSystem)]
        public static void ConfigureTfsEnvironment(ICakeContext ctx, TaskConfig cfg)
        {
            ITFBuildProvider tfs = ctx.TFBuild();
            if (!tfs.IsRunningOnAzurePipelines)
                throw new TaskConfigException("Not running in TFS");

            var env = cfg.Load<EnvConfig>();
            env.IsCi = true;

            // If the build number is configured as an integer, use it. Otherwise use the build ID.
            // Basically, we need something unique.
            env.Version.BuildNumber = int.TryParse(tfs.Environment.Build.Number, out int buildNum)
                ? buildNum : tfs.Environment.Build.Id;
        }
    }
}
