using GameEngine.Util.Values;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace GameEngine.Core;

public static class WindowService
{

    public static IWindow? mainWindow = null;
    public static List<IWindow> windows = [];

    private static List<IWindow> _windowsToClose = [];

    public static IWindow CreateNewWindow(Action<IWindow>? onload=null)
    {
        return CreateNewWindow(new Vector2<int>(800, 600), "New Window " + windows.Count, onload);
    }
    public static IWindow CreateNewWindow(Vector2<int> size, string title, Action<IWindow>? onload=null)
    {
        
        WindowOptions options = WindowOptions.Default with
        {
            Title = title,
            Size = size.GetAsSilkInt(),
            WindowState = WindowState.Normal,
            ShouldSwapAutomatically = false,
            Samples = 1,
            VSync = false
        };

        if (mainWindow != null)
            options.SharedContext = mainWindow.GLContext;
        else
            options.VSync = true;

        var nWin = Window.Create(options);

        if (onload!=null)
            nWin.Load += () => onload(nWin);

        nWin.Initialize();

        if (mainWindow == null)
        {
            mainWindow = nWin;
            Engine.gl = nWin.CreateOpenGL();
        }

        nWin.ConfigWindow();

        windows.Add(nWin);

        return nWin;

    }

    public static void CloseWindow(IWindow win)
    {
        _windowsToClose.Add(win);
        windows.Remove(win);
        if (win == mainWindow) mainWindow = null;
    }

    private static void ConfigWindow(this IWindow win)
    {
        var gl = Engine.gl;

        // GL configurations //
        gl.ClearColor(1f, 1f, 1f, 1f);
        gl.Enable(EnableCap.ScissorTest);
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    public static void CallProcess()
    {
        foreach (var win in _windowsToClose)
            win.Dispose();
    }

}