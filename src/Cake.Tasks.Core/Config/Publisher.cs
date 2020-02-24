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
    public abstract class Publisher
    {
        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private bool _outputSet;

        protected Publisher(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Specify a valid and unique name for the publisher.", nameof(name));

            // Name must be a valid directory name
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("Specify a valid directory name for the publisher name.", nameof(name));

            Name = name;
        }

        public string Name { get; }

        public string OutputLocation { get; private set; }

        public PublishOutputType OutputType { get; private set; }

        public void SetOutput(string location, PublishOutputType type = PublishOutputType.Directory)
        {
            if (_outputSet)
                throw new InvalidOperationException("The output is already set for this publisher. Cannot set it again.");

            if (type == PublishOutputType.Directory && !Directory.Exists(location))
                throw new DirectoryNotFoundException($"The specified output directory '{location ?? string.Empty}' does not exist.");
            if (type == PublishOutputType.File && !File.Exists(location))
                throw new FileNotFoundException($"The specified output file does not exist.", location ?? string.Empty);

            OutputLocation = location;
            OutputType = type;

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
