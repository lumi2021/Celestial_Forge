using GameEngine.Util.Values;
using Silk.NET.GLFW;

namespace GameEngine.Util.Resources;

public readonly struct InputEvent(TimeSpan timestamp, IInputEvent eventObject)
{

    public readonly TimeSpan timestamp = timestamp;
    public readonly Type eventType = eventObject.GetType();
    public readonly IInputEvent eventData = eventObject;

    public bool Is(Type inputEventType) => eventData.Is(inputEventType);
    public bool Is<T>() where T : struct, IInputEvent => eventData.Is<T>();
    public bool Is<T>(out T inputEvent) where T : struct, IInputEvent
    {
        if (eventData.Is<T>())
        {
            if (eventType == typeof(T))
                inputEvent = (T)eventData;
            else
                inputEvent = default!;

            return true;
        }
        else
        {
            inputEvent = default!;
            return false;
        }
    }

    public override string ToString()
    {
        return $"{eventType.Name} (timestamp: {timestamp}, " + eventData.DataToString() + ")";
    }

}

/* input events */
public readonly struct KeyboardInputEvent(bool repeating, Keys key, InputAction action) : IInputEvent
{
    private readonly Type[] inheritance = [ typeof(InputEvent) ];
    public bool Is<T>() where T : IInputEvent => inheritance.Contains(typeof(T))||GetType()==typeof(T);
    public bool Is(Type inputEventType) => inheritance.Contains(inputEventType);
    
    public readonly bool repeating = repeating;
    public readonly Keys key = key;
    public readonly InputAction action = action;

    public readonly string DataToString()
    {
        return $"repeating: {repeating}, key: {key}, action: {action}";
    }

    public override readonly string ToString()
    {
        return $"{GetType().Name} (" + DataToString() + ")";
    }
}

public readonly struct MouseInputEvent() : IInputEvent
{
    private readonly Type[] inheritance = [ typeof(InputEvent), typeof(MouseInputEvent) ];

    public bool Is<T>() where T : IInputEvent => inheritance.Contains(typeof(T))||GetType()==typeof(T);
    public bool Is(Type inputEventType) => inheritance.Contains(inputEventType);

    public readonly string DataToString()
    {
        return "";
    }
    public override readonly string ToString()
    {
        return $"{GetType().Name} ()";
    }

}
public readonly struct MouseBtnInputEvent(MouseButton button, InputAction action, Vector2<int> position) : IInputEvent
{
    private readonly Type[] inheritance = [ typeof(InputEvent), typeof(MouseInputEvent) ];
    public bool Is<T>() where T : IInputEvent => inheritance.Contains(typeof(T))||GetType()==typeof(T);
    public bool Is(Type inputEventType) => inheritance.Contains(inputEventType);

    public readonly MouseButton button = button;
    public readonly InputAction action = action;
    public readonly Vector2<int> position = position;

    public readonly string DataToString()
    {
        return $"button: {button}, action: {action}, position: {position}";
    }
    public override readonly string ToString()
    {
        return $"{GetType().Name} (" + DataToString() + ")";
    }

}
public readonly struct MouseMoveInputEvent(Vector2<int> position, Vector2<int> lastPosition ,Vector2<int> positionDelta) : IInputEvent
{
    private readonly Type[] inheritance = [ typeof(InputEvent), typeof(MouseInputEvent) ];
    public bool Is<T>() where T : IInputEvent => inheritance.Contains(typeof(T))||GetType()==typeof(T);
    public bool Is(Type inputEventType) => inheritance.Contains(inputEventType);

    public readonly Vector2<int> position = position;
    public readonly Vector2<int> lastPosition = lastPosition;
    public readonly Vector2<int> positionDelta = positionDelta;

    public readonly string DataToString()
    {
        return $"position: {position}, last position: {lastPosition}, position delta: {positionDelta}";
    }
    public override readonly string ToString()
    {
        return $"{GetType().Name} (" + DataToString() + ")";
    }

}
public readonly struct MouseScrollInputEvent(Vector2<double> offset) : IInputEvent
{
    private readonly Type[] inheritance = [ typeof(InputEvent), typeof(MouseInputEvent) ];
    public bool Is<T>() where T : IInputEvent => inheritance.Contains(typeof(T))||GetType()==typeof(T);
    public bool Is(Type inputEventType) => inheritance.Contains(inputEventType);

    public readonly Vector2<double> offset = offset;

    public readonly string DataToString()
    {
        return $"offset: {offset}";
    }
    public override readonly string ToString()
    {
        return $"{GetType().Name} (" + DataToString() + ")";
    }

}

public interface IInputEvent
{
    public bool Is<T>() where T : IInputEvent;
    public bool Is(Type inputEventType);
    public string DataToString();
}