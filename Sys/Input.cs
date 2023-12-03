using System.Numerics;
using GameEngine.Util.Values;
using Silk.NET.Input;

namespace GameEngine.Sys;

public static class Input
{

    private static readonly IInputContext _input;

    private static readonly List<Key> keysPressed = new();
    private static readonly List<Key> keysDowned = new();
    private static readonly List<Key> keysReleased = new();

    private static readonly List<MouseButton> mousePressed = new();
    private static readonly List<MouseButton> mouseDowned = new();
    private static readonly List<MouseButton> mouseReleased = new();

    private static Vector2<float> lastMousePosition;
    public static Vector2<float> mouseDelta;
    private static bool mouseMoved = false;

    static Input()
    {
        _input = Engine.window.CreateInput();
        for (int i = 0; i < _input.Keyboards.Count; i++)
        {
            _input.Keyboards[i].KeyDown += KeyDown;
            _input.Keyboards[i].KeyUp += KeyUp;
        }
        for (int i = 0; i < _input.Mice.Count; i++)
        {
            _input.Mice[i].MouseDown += MouseDown;
            _input.Mice[i].MouseUp += MouseUp;
            _input.Mice[i].MouseMove += MouseMove;
        }
    }

    // KEYBOARD
    public static bool IsActionPressed(Key key)
    {
        return keysPressed.Contains(key);
    }
    public static bool IsActionJustPressed(Key key)
    {
        return keysDowned.Contains(key);
    }
    public static bool IsActionJustReleased(Key key)
    {
        return keysReleased.Contains(key);
    }

    // MOUSE
    public static bool IsActionPressed(MouseButton btn)
    {
        return mousePressed.Contains(btn);
    }
    public static bool IsActionJustPressed(MouseButton btn)
    {
        return mouseDowned.Contains(btn);
    }
    public static bool IsActionJustReleased(MouseButton btn)
    {
        return mouseReleased.Contains(btn);
    }
    
    
    public static Vector2<float> GetMousePosition()
    {
        return lastMousePosition;
    }

    public static void SetCursorMode(CursorMode mode)
    {
        for (int i = 0; i < _input.Mice.Count; i++)
        {
            _input.Mice[i].Cursor.CursorMode = mode;
        }
    }
    public static unsafe void SetCursorShape(Silk.NET.GLFW.CursorShape shape)
    {
        var cursor = Silk.NET.GLFW.GlfwProvider.GLFW.Value.CreateStandardCursor(shape);
        Silk.NET.GLFW.GlfwProvider.GLFW.Value.SetCursor((Silk.NET.GLFW.WindowHandle*)Engine.window.Handle, cursor);
    }

    public static void CallProcess()
    {
        mouseDelta = new();
        keysDowned.Clear();
        keysReleased.Clear();
        mouseDowned.Clear();
        mouseReleased.Clear();
    }

    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        keysPressed.Add(key);
        keysDowned.Add(key);
    }
    private static void KeyUp(IKeyboard keyboard, Key key, int keyCode)
    {
        keysPressed.Remove(key);
        keysReleased.Add(key);
    }

    private static void MouseDown(IMouse mouse, MouseButton button)
    {
        mousePressed.Add(button);
        mouseDowned.Add(button);
    }
    private static void MouseUp(IMouse mouse, MouseButton button)
    {
        mousePressed.Remove(button);
        mouseReleased.Add(button);
    }

    private static void MouseMove(IMouse mouse, Vector2 numericsPos)
    {
        if (!mouseMoved)
        {
            lastMousePosition = new(numericsPos);
            mouseMoved = true;
        }

        Vector2<float> position = new(numericsPos);

        mouseDelta = position - lastMousePosition;

        lastMousePosition = position;
    }

}