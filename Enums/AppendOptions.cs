using System;

namespace FileLogger {

    /// <summary>
    /// Options used to decide what info will be added to a single line in the
    /// log output.
    /// </summary>
    [Flags]
    public enum AppendOptions {

        Timestamp = 1,
        ClassName = 2,
        CallerClassName = 4

    }

}