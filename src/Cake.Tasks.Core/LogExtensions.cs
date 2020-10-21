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

using System.Runtime.CompilerServices;

using Cake.Core;
using Cake.Core.Diagnostics;

namespace Cake.Tasks.Core
{
    public static class LogExtensions
    {
        public static void LogInfo(this ICakeContext ctx, string message, [CallerMemberName] string caller = "")
        {
            string fullMessage = $"\u001b[37;1m\u001b[44;1m{caller}\u001b[0m {message}";
            ctx.Log.Information(fullMessage);
        }

        public static void LogWarning(this ICakeContext ctx, string message, [CallerMemberName] string caller = "")
        {
            string fullMessage = $"\u001b[30;1m\u001b[43;1m{caller}\u001b[0m {message}";
            ctx.Log.Information(fullMessage);
        }
    }
}
