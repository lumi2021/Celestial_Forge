using System.Runtime.CompilerServices;

namespace GameEngine.Debugging;

public static class Debug
{

    public delegate void OnLogHandler(LogInfo log);
    public delegate void OnClearHandler();

    public static event OnLogHandler? OnLogEvent;
    public static event OnLogHandler? OnErrorEvent;
    public static event OnClearHandler? OnLogClearedEvent;
    public static event OnClearHandler? OnErrorClearedEvent;


    private static readonly List<LogInfo> _logInfos = [];
    public static LogInfo[] LogInfos => [.. _logInfos];

    private static readonly List<LogInfo> _errorInfos = [];
    public static LogInfo[] ErrorInfos => [.. _errorInfos];


    public static LogInfo Log(object? value,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
        )
    {

        string finalValue = value?.ToString() ?? "null";

        LogInfo nInfo = new(finalValue, DateTime.Now.TimeOfDay, sourceFilePath, memberName, sourceLineNumber);
        _logInfos.Add(nInfo);

        Console.WriteLine(nInfo);

        OnLogEvent?.Invoke(nInfo);

        return nInfo;
    }

    public static LogInfo Error(object? value,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
        )
    {

        string finalValue = value?.ToString() ?? "null";

        LogInfo nInfo = new(finalValue, DateTime.Now.TimeOfDay, sourceFilePath, memberName, sourceLineNumber);
        _errorInfos.Add(nInfo);

        var oldFCol = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\nError:");
        Console.ForegroundColor = oldFCol;

        Console.WriteLine(nInfo);

        OnErrorEvent?.Invoke(nInfo);

        return nInfo;
    }

    // FIXME use the exeption trace instead Caller info
    public static LogInfo Exception(Exception ex,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
        )
    {

        string finalValue = ex.Message;

        LogInfo nInfo = new(finalValue, DateTime.Now.TimeOfDay, sourceFilePath, memberName, sourceLineNumber);
        _errorInfos.Add(nInfo);

        var oldCol = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n{ex.GetType().Name}:");
        Console.ForegroundColor = oldCol;

        Console.WriteLine(nInfo);
        Console.WriteLine(ex.StackTrace);

        OnErrorEvent?.Invoke(nInfo);
        return nInfo;
    }


    public static void ClearLog()
    {
        _logInfos.Clear();
        OnLogClearedEvent?.Invoke();
    }
    public static void ClearErrors()
    {
        _errorInfos.Clear();
        OnErrorClearedEvent?.Invoke();
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