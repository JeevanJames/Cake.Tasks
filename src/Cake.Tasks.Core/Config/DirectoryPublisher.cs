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

using System.Collections.Generic;

namespace Cake.Tasks.Config
{
    public class DirectoryPublisher : Publisher
    {
        public DirectoryPublisher(string name, string directory)
            : base(name)
        {
            Directory = directory;
            SetProperty(DirectoryPublisherKeys.IsDirectory, true);
        }

        public string Directory { get; }

        public override PublishOutputType OutputType => PublishOutputType.Directory;
    }

    public static class DirectoryPublisherExtensions
    {
        public static Publisher AddDirectory(this IList<Publisher> publishers, string name, string directory)
        {
            var publisher = new DirectoryPublisher(name, directory);
            publishers.Add(publisher);
            return publisher;
        }
    }

    public static class DirectoryPublisherKeys
    {
        public const string IsDirectory = "IsDirectory";
    }
}
