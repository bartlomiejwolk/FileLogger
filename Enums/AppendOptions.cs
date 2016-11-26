// Copyright (c) 2015 Bart³omiej Wo³k (bartlomiejwolk@gmail.com)
//  
// This file is part of the FileLogger extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using System;

namespace FileLoggerTool.Enums
{
    /// <summary>
    ///     displayOptions used to decide what info will be added to a single line in the
    ///     log output.
    /// </summary>
    [Flags]
    public enum AppendOptions
    {
        Timestamp = 1 << 1,
        CallerClassName = 1 << 2,
        MethodName = 1 << 3,
        ParentClassName = 1 << 4,
        MessageCategory = 1 << 5
    }
}