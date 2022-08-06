// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Cake.Tasks.Config;

public class DirectoryPublisher : Publisher
{
    public DirectoryPublisher(string directory)
    {
        Directory = directory;
    }

    public string Directory { get; }

    public override PublishOutputType OutputType => PublishOutputType.Directory;

    public virtual void ValidateDirectory()
    {
        // No validation, by default
    }
}

public static class DirectoryPublisherExtensions
{
    public static Publisher AddDirectory(this IList<Publisher> publishers, string directory)
    {
        var publisher = new DirectoryPublisher(directory);
        publishers.Add(publisher);
        return publisher;
    }
}
