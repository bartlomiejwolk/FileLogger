using System;

namespace FileLoggerTool.Enums
{
    [Flags]
    public enum MessageCategories
    {
        Generic = 1 << 0,
        Network = 1 << 1,
        PlayFab = 1 << 2,
        Physics = 1 << 3,
        UI = 1 << 4,
        Unity = 1 << 5,
        // use it from commands
        Command = 1 << 6
    }
}