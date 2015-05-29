// Copyright (c) 2015 Bartlomiej Wolk (bartlomiejwolk@gmail.com)
//  
// This file is part of the FileLogger extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using System;

namespace FileLogger {

    /// <summary>
    ///     displayOptions used to decide what info will be added to a single line in the
    ///     log output.
    /// </summary>
    [Flags]
    public enum AppendOptions {

        Timestamp = 0x0001,
        CallerClassName = 0x0002,
        MethodName = 0x0004,
        ParentClassName = 0x0008

    }

}