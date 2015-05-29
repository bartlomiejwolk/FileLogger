// Copyright (c) 2015 Bartlomiej Wolk (bartlomiejwolk@gmail.com)
//  
// This file is part of the FileLogger extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using System;

namespace FileLogger {

    /// <summary>
    ///     Options used to decide which Logger methods will be active, ie. will
    ///     produce output.
    /// </summary>
    [Flags]
    public enum EnabledMethods {

        LogCall = 1,
        LogString = 2,
        LogResult = 4

    }

}