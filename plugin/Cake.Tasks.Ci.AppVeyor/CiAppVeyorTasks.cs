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

using Cake.Common.Build;
using Cake.Common.Build.AppVeyor;
using Cake.Core;
using Cake.Tasks.Ci.AppVeyor;
using Cake.Tasks.Config;
using Cake.Tasks.Core;

[assembly: TaskPlugin(typeof(CiAppVeyorTasks))]

namespace Cake.Tasks.Ci.AppVeyor
{
    public static class CiAppVeyorTasks
    {
        [Config(CiSystem = "appveyor", Order = ConfigTaskOrder.CiSystem)]
        public static void ConfigureAppVeyorEnvironment(ICakeContext ctx, TaskConfig cfg)
        {
            IAppVeyorProvider appveyor = ctx.AppVeyor();

            if (!appveyor.IsRunningOnAppVeyor)
                throw new TaskConfigException("Not running in AppVeyor");

            EnvConfig env = cfg.Load<EnvConfig>();
            env.IsCi = true;

            env.Version.BuildNumber = appveyor.Environment.Build.Number.ToString();
            env.Version.Build = $"{env.Version.Primary}-build.{env.Version.BuildNumber}";
        }
    }
}
