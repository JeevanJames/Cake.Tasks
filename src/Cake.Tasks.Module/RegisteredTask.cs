using System;
using System.Reflection;

using Cake.Tasks.Core;

namespace Cake.Tasks.Module
{
    internal sealed class RegisteredTask
    {
        internal Type AttributeType { get; set; }

        internal MethodInfo Method { get; set; }

        internal string Name { get; set; }

        internal string Environment { get; set; }

        // Optional properties - specific to task type

        internal CoreTask? CoreTask { get; set; }

        internal TaskEventType? EventType { get; set; }

        internal int Order { get; set; }
    }
}
