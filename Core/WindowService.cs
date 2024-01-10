using GameEngine.Util.Values;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace GameEngine.Core;

public static class WindowService
{

    public static IWindow? mainWindow = null;
    public static List<IWindow> windows = new();

    private static List<IWindow> _windowsToClose = new();

    public static IWindow CreateNewWindow(Action? onload=null)
    {
        return CreateNewWindow(new Vector2<int>(800, 600), "New Window " + windows.Count, onload);
    }
    public static IWindow CreateNewWindow(Vector2<int> size, string title, Action? onload=null)
    {
        
        WindowOptions options = WindowOptions.Default with
        {
            Title = title,
            Size = size.GetAsSilkInt(),
            WindowState = WindowState.Normal,
            Samples = 4
        };

        if (mainWindow != null)
            options.SharedContext = mainWindow.GLContext;

        var nWin = Window.Create(options);

        if (mainWindow == null)
        {
            mainWindow = nWin;
            if (onload!=null) nWin.Load += onload;
            nWin.Initialize();
            Engine.gl = nWin.CreateOpenGL();
        }

        windows.Add(nWin);

        Engine.gl.ClearColor(1f, 1f, 1f, 1f);

        return nWin;

    }

    public static void CloseWindow(IWindow win)
    {
        _windowsToClose.Add(win);
        windows.Remove(win);
        if (win == mainWindow) mainWindow = null;
    }

    public static void CallProcess()
    {
        foreach (var win in _windowsToClose)
        {
            win.Dispose();
        }
    }

}