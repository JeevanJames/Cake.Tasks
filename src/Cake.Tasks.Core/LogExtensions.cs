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
            ctx.Log.Information($"{InfoPrefix}{caller}{Reset} {Info}{message}{Reset}");
        }

        public static void LogWarning(this ICakeContext ctx, string message, [CallerMemberName] string caller = "")
        {
            ctx.Log.Warning($"{WarningPrefix}{caller}{Reset} {Warning}{message}{Reset}");
        }

        public static void LogVerbose(this ICakeContext ctx, string message, [CallerMemberName] string caller = "")
        {
            ctx.Log.Verbose($"{VerbosePrefix}{caller}{Reset} {Verbose}{message}{Reset}");
        }

        private const string Escape = "\u001b[";

        private const string Reset = Escape + "0m";

        private const string InfoPrefixBg = Escape + "44;1m"; // Bright blue bg
        private const string InfoPrefixFg = Escape + "37;1m"; // Bright white fg
        private const string InfoPrefix = InfoPrefixFg + InfoPrefixBg;
        private const string InfoFg = Escape + "34;1m"; // Bright blue fg
        private const string Info = InfoFg;

        private const string WarningPrefixBg = Escape + "43;1m"; // Bright yellow bg
        private const string WarningPrefixFg = Escape + "30;1m"; // Bright black fg
        private const string WarningPrefix = WarningPrefixFg + WarningPrefixBg;
        private const string WarningFg = Escape + "33;1m"; // Bright yellow fg
        private const string Warning = WarningFg;

        private const string VerbosePrefixBg = Escape + "46;1m"; // Bright cyan bg
        private const string VerbosePrefixFg = Escape + "37;1m"; // Bright white fg
        private const string VerbosePrefix = VerbosePrefixFg + VerbosePrefixBg;
        private const string VerboseFg = Escape + "36;1m"; // Bright cyan fg
        private const string Verbose = VerboseFg;
    }
}
