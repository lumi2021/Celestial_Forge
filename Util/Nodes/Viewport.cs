using GameEngine.Core;
using GameEngine.Util.Attributes;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Values;
using Silk.NET.OpenGL;

namespace GameEngine.Util.Nodes;

[Icon("./Assets/icons/Nodes/Viewport.svg")]
public class Viewport : Node
{

    private uint viewportFramebuffer;
    protected uint viewportTexture;

    [Inspect] public Color backgroundColor = new();

    protected Vector2<uint> _viewportSize;
    [Inspect] public virtual Vector2<uint> ViewportSize
    {
        get { return _viewportSize; }
        set { _viewportSize = value; }
    }

    public bool useContainerSize = false;
    protected Vector2<uint> _containerSize;
    [Inspect] public virtual Vector2<uint> ContainerSize
    {
        get { return useContainerSize ? _containerSize : _viewportSize; }
        set { _containerSize = value; }
    }

    private Vector2<uint> _textureSize = new(10, 10);

    protected bool proceedInput = true;

    protected NodeUI? _focusedUiNode = null;
    public NodeUI? FocusedUiNode
    {
        get { return _focusedUiNode; }
        set
        {
            if (value ==  _focusedUiNode) return;

            _focusedUiNode?.RunFocusChanged(value ==  _focusedUiNode);
            value?.RunFocusChanged(true);
            _focusedUiNode = value;
        }
    }

    private Camera2D _defaultCamera2D = null!;
    protected Camera2D? _currentCamera2D = null;
    public Camera2D Camera2D 
    {
        get {
            if (_currentCamera2D == null)
            {
                if (_defaultCamera2D == null)
                {
                    _defaultCamera2D = new();
                    AddAsChild(_defaultCamera2D);
                }
                return _defaultCamera2D;
            }
            else return _currentCamera2D;
        }
    }

    protected unsafe override void Init_()
    {
        var gl = Engine.gl;

        viewportFramebuffer = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, viewportFramebuffer);

        viewportTexture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, viewportTexture);

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        ResizeTexture(new (30, 30));

        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, viewportTexture, 0);

        FramebufferStatus status = (FramebufferStatus) gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferStatus.Complete)
        {
            throw new Exception($"FrameBuffer initialization error! {status}");
        }

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public override void Free()
    {
        ResourceHeap.Delete(viewportTexture, ResourceHeap.DeleteTarget.Texture);
        viewportTexture = 0;
        try {
            Engine.gl.DeleteFramebuffer(viewportFramebuffer);
        }
        catch {}
    }

    public virtual void Render(Vector2<uint> size, double deltaTime)
    {
        var gl = Engine.gl;
        
        ResizeTexture(size);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, viewportFramebuffer);

        DrawService.SetViewport(size);
        gl.Scissor(0,0, size.X, size.Y);
        gl.ClearColor(backgroundColor);
        gl.Clear(ClearBufferMask.ColorBufferBit);

        List<Node> toIterate = [.. children];
        Dictionary<int, List<Node>> toDraw = [];

        while (toIterate.Count > 0)
        {
            Node current = toIterate.Unqueue();

            if (
                current is Viewport || current.Freeled ||
                current is ICanvasItem ci && !ci.Visible
            ) continue;

            int zindex = (current as ICanvasItem)?.GlobalZIndex ?? 0;

            if (!toDraw.ContainsKey(zindex))
                toDraw.Add(zindex, []);

            toDraw[zindex].Add(current);

            var childrenToAdd = current.GetAllChildren;
            for (int i = childrenToAdd.Length - 1; i >= 0; i--)
                toIterate.Insert(0, childrenToAdd[i]);
        }

        foreach (var i in toDraw)
        foreach (var current in i.Value)
        {
            // configurate scissor
            if (current.parent is IClipChildren @p)
            {
                var clipRect = @p.GetClippingArea();
                var viewRect = new Rect(Camera2D.position.X, Camera2D.position.Y, size.X, size.Y);

                clipRect = clipRect.InvertVerticallyIn(viewRect);
                gl.Scissor(clipRect);
            }
            else gl.Scissor(0,0, size.X, size.Y);

            current.RunDraw(deltaTime);

        }

        DrawService.PopViewport();

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private unsafe void ResizeTexture(Vector2<uint> newSize)
    {
        if (_textureSize != newSize)
        {
            _textureSize = newSize;
            _viewportSize = newSize;
            Engine.gl.BindTexture(TextureTarget.Texture2D, viewportTexture);

            fixed (byte* buffer = new byte[newSize.X * newSize.Y * 4])
            Engine.gl.TexImage2D(
                GLEnum.Texture2D, 0, InternalFormat.Rgba,
                newSize.X, newSize.Y, 0,
                PixelFormat.Rgba, GLEnum.UnsignedByte,
                buffer
            );

            Engine.gl.BindTexture(TextureTarget.Texture2D, 0);

            RequestUpdateAllChildrens();
        }
    }

    public void Use()
    {
        Engine.gl.BindTexture(TextureTarget.Texture2D, viewportTexture);
    }

    public void SupressInputEvent() => proceedInput = false;

    public void SetCurrentCamera(Camera2D? cam) => _currentCamera2D = cam;

}