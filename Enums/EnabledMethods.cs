using System;

namespace FileLogger {

    /// <summary>
    /// Options used to decide which Logger methods will be active, ie. will
    /// produce output.
    /// </summary>
    [Flags]
    public enum EnabledMethods {

        LogCall = 1,
        LogString = 2,
        LogResult = 4

    }

}