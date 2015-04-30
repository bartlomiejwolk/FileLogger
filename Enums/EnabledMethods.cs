using System;

namespace FileLogger {

    /// <summary>
    /// Options used to decide which Logger methods will be active, ie. will
    /// produce output.
    /// </summary>
    [Flags]
    public enum EnabledMethods {

        LogCall = 0x0001,
        LogString = 0x0002,
        LogResult = 0x0004,
        LogIntVariable = 0x0008

    }

}