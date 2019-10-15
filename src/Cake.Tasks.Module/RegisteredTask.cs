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

using System;
using System.Reflection;

using Cake.Tasks.Core;

namespace Cake.Tasks.Module
{
    /// <summary>
    ///     A task registered from a plugin attribute.
    /// </summary>
    internal sealed class RegisteredTask
    {
        /// <summary>
        ///     Gets or sets the type of the plugin attribute.
        /// </summary>
        internal Type AttributeType { get; set; }

        /// <summary>
        ///     Gets or sets the method that contains the plugin logic.
        /// </summary>
        internal MethodInfo Method { get; set; }

        /// <summary>
        ///     Gets or sets the name of the plugin. This is used to name the Cake task.
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        ///     Gets or sets the name of the CI system under which this plugin can run. A <c>null</c>
        ///     value indicates that the plugin will always run.
        /// </summary>
        internal string CiSystem { get; set; }

        // Optional properties - specific to task type

        internal PipelineTask? CoreTask { get; set; }

        internal TaskEventType? EventType { get; set; }

        internal int Order { get; set; }
    }
}
