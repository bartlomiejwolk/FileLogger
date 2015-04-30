using System;

namespace FileLogger {

    /// <summary>
    /// AppendOptions used to decide what info will be added to a single line in the
    /// log output.
    /// </summary>
    [Flags]
    public enum AppendOptions {

        Timestamp = 0x0001,
        ClassName = 0x0002,
        MethodName = 0x0004,
        CallerClassName = 0x0008

    }

}