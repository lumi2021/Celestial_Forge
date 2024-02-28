namespace GameEngine.Debug;

public static class Console
{

    public delegate void OnLogHandler(LogInfo log);
    public static event OnLogHandler? OnLogEvent;

    private static readonly List<LogInfo> _logInfos = [];
    public static LogInfo[] LogInfos => [.. _logInfos];


    public static LogInfo Log(string value) => Log(value, []);
    public static LogInfo Log(object? value) => Log("{0}", [value]);

    public static LogInfo Log(string format, params object?[]? arg)
    {
        string value = string.Format(format, arg ?? []);

        LogInfo nInfo = new(value, DateTime.Now);
        _logInfos.Add(nInfo);

        System.Console.WriteLine(nInfo);

        OnLogEvent?.Invoke(nInfo);

        return nInfo;
    }


}

public readonly struct LogInfo (string message, DateTime timestamp)
{
    public readonly string message = message;
    public readonly DateTime timestamp = timestamp;

    public override string ToString() => message;
}