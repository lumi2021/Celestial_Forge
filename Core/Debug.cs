using System.Runtime.CompilerServices;

namespace GameEngine.Debugging;

public static class Debug
{

    public delegate void OnLogHandler(LogInfo log);
    public static event OnLogHandler? OnLogEvent;

    private static readonly List<LogInfo> _logInfos = [];
    public static LogInfo[] LogInfos => [.. _logInfos];


    public static LogInfo Log(object? value,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
        )
    {

        string finalValue = value?.ToString() ?? "null";

        LogInfo nInfo = new(finalValue, DateTime.Now.TimeOfDay, sourceFilePath, memberName, sourceLineNumber);
        _logInfos.Add(nInfo);

        System.Console.WriteLine(nInfo);

        OnLogEvent?.Invoke(nInfo);

        return nInfo;
    }


}

public readonly struct LogInfo (string message, TimeSpan timestamp, string sourceFile, string callerName, int lineNumber)
{
    public readonly string message = message;
    public readonly TimeSpan timestamp = timestamp;
    public readonly string sourceFile = sourceFile;
    public readonly string callerName = callerName;
    public readonly int lineNumber = lineNumber;

    public override string ToString() => message;
}