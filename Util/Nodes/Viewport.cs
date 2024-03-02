using GameEngine.Core;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Values;
using Silk.NET.OpenGL;

namespace GameEngine.Util.Nodes;

public class Viewport : Node
{

    private uint viewportFramebuffer;
    protected uint viewportTexture;

    public Color backgroundColor = new();

    protected Vector2<uint> _size;
    public virtual Vector2<uint> Size
    {
        get { return _size; }
        set { _size = value; }
    }

    public bool useContainerSize = false;
    protected Vector2<uint> _containerSize;
    public virtual Vector2<uint> ContainerSize
    {
        get { return useContainerSize ? _containerSize : _size; }
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
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        List<Node> toDraw = [.. children];

        while (toDraw.Count > 0)
        {
            Node current = toDraw[0];
            toDraw.RemoveAt(0);

            if (current is Viewport || current.Freeled) continue;

            if (current is ICanvasItem)
            {
                // configurate scissor
                if (current.parent is IClipChildren)
                {
                    var clipRect = (current.parent as IClipChildren)!.GetClippingArea();
                    clipRect = clipRect.InvertVerticallyIn(
                        new(Camera2D.position.X, Camera2D.position.Y, size.X, size.Y) );
                    gl.Scissor(clipRect);
                }

                // checks if it's visible and draw
                if ((current as ICanvasItem)!.Visible)
                    current.RunDraw(deltaTime);
                
                else continue; // Don't draw childrens
            }

            for (int i = current.children.Count - 1; i >= 0; i--)
                toDraw.Insert(0,  current.children[i]);
        }

        DrawService.PopViewport();

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private unsafe void ResizeTexture(Vector2<uint> newSize)
    {
        if (_textureSize != newSize)
        {
            _textureSize = newSize;
            Size = newSize;
            Engine.gl.BindTexture(TextureTarget.Texture2D, viewportTexture);

            fixed (byte* buffer = new byte[newSize.X * newSize.Y * 4])
            Engine.gl.TexImage2D(
                GLEnum.Texture2D, 0, InternalFormat.Rgba,
                newSize.X, newSize.Y, 0,
                PixelFormat.Rgba, GLEnum.UnsignedByte,
                buffer
            );

            Engine.gl.BindTexture(TextureTarget.Texture2D, 0);
        }
    }

    public void Use()
    {
        Engine.gl.BindTexture(TextureTarget.Texture2D, viewportTexture);
    }

    public void SupressInputEvent()
    {
        proceedInput = false;
    }

    public void SetCurrentCamera(Camera2D? cam) => _currentCamera2D = cam;

}