// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using System.Text;

namespace Cake.Tasks.Core.Config;

public sealed class PipelineSummary
{
    private readonly StringBuilder _builder = new();

    public void AddEntry(string entry)
    {
        _builder.AppendLine(entry)
            .AppendLine();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _builder.ToString();
    }
}
