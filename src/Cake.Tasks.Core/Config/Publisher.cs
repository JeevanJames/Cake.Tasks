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
using System.Collections.Generic;
using System.IO;

namespace Cake.Tasks.Config
{
    /// <summary>
    ///     Represents an item that can be published from the current build.
    /// </summary>
    public abstract class Publisher
    {
        /// <summary>
        ///     Additional properties that can be set on the publisher to control the behavior of
        ///     specific tasks.
        /// </summary>
        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     Indicates whether the output properties - <see cref="OutputLocation"/> and
        ///     <see cref="OutputType"/> have been set using the <see cref="SetOutput"/> method. This
        ///     is used to ensure that the method is called only once.
        /// </summary>
        private bool _outputSet;

        private PublishOutputType _outputType;

        protected Publisher(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Specify a valid and unique name for the publisher.", nameof(name));

            // Name must be a valid directory name. The framework will use it as the directory name
            // to copy the publish output to.
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("Specify a valid directory name for the publisher name.", nameof(name));

            Name = name;
        }

        /// <summary>
        ///     Gets the unique name for this publisher.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the directory where the published output is copied to, or the file name of the
        ///     published output, depending on the value of <see cref="OutputType"/>.
        /// </summary>
        public string OutputLocation { get; private set; }

        /// <summary>
        ///     Gets whether the published output is a directory of multiple files or a single file.
        /// </summary>
        public virtual PublishOutputType OutputType => _outputType;

        public void SetOutput(string location, PublishOutputType type = PublishOutputType.Directory)
        {
            if (_outputSet)
                throw new InvalidOperationException("The output is already set for this publisher. Cannot set it again.");

            if (type == PublishOutputType.Directory && !Directory.Exists(location))
                throw new DirectoryNotFoundException($"The specified output directory '{location ?? string.Empty}' does not exist.");
            if (type == PublishOutputType.File && !File.Exists(location))
                throw new FileNotFoundException($"The specified output file does not exist.", location ?? string.Empty);

            OutputLocation = location;
            _outputType = type;

            _outputSet = true;
        }

        public T GetProperty<T>(string name)
        {
            return _properties.TryGetValue(name, out object obj) ? (T)obj : default;
        }

        public Publisher SetProperty<T>(string name, T value)
        {
            _properties.Add(name, value);
            return this;
        }
    }

    public enum PublishOutputType
    {
        Directory,
        File,
    }
}
