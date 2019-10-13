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
using System.Runtime.Serialization;

namespace Cake.Tasks.Core
{
    /// <summary>
    ///     Exception to throw if a task's configuration value is invalid.
    /// </summary>
    [Serializable]
    public sealed class TaskConfigException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TaskConfigException"/> class.
        /// </summary>
        public TaskConfigException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TaskConfigException"/> class with the
        ///     specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TaskConfigException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TaskConfigException"/> class with the
        ///     specified <paramref name="message"/> and <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception, or <c>null</c> if no inner
        ///     exception is specified.
        /// </param>
        public TaskConfigException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TaskConfigException"/> class with serialized data.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="SerializationInfo"/> that holds the serialized object data about the
        ///     exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="StreamingContext"/> that contains contextual information about the
        ///     source or destination.
        /// </param>
        private TaskConfigException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
