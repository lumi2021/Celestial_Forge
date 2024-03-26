using GameEngine.Core;
using GameEngine.Util.Attributes;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources;
using static GameEngine.Util.Nodes.Window;

namespace GameEngine.Util.Nodes;

[Icon("./Assets/icons/Nodes/Node.svg")]
public class Node
{

    /* System variables */
    public bool Freeled { get; private set; }
    private bool _gcCalled = false;
    private bool _isReady = false;
    public readonly uint NID = 0;

    /* Process variables */
    public bool readyEnabled = true;
    public bool processEnabled = true;
    public bool inputEnabled = true;

    /* Tree-referent variables */
    private bool _isOnTree = false;
    private Viewport? _parentViewport = null;
    private InputHandler? _parentInputHandler = null;

    /* Script & loading variables */
    private Dictionary<string, object?> _fieldsToLoadWhenReady = [];

    public Node? parent;
    private bool _isGhostChildren = false;

    public List<Node> children = [];
    protected List<WeakReference<Node>> ghostChildren = [];
    
    public Node[] GetAllChildren
        => [.. ghostChildren.Select(e => {e.TryGetTarget(out var n); return n;}), .. children];

    public string name = "";

    protected Viewport? Viewport
    {
        get {
            if (_parentViewport == null)
                if (parent is Viewport view)
                    return view;

                else if (parent != null)
                    _parentViewport = parent.Viewport;
            
            return _parentViewport;
        }
    }
    protected InputHandler Input
    {
        get {
            if (_parentInputHandler == null)
            {
                if (Viewport is Window win)
                    _parentInputHandler = win.input;

                else if (Viewport != null)
                    _parentInputHandler = Viewport!.Input;
                
                _parentInputHandler ??= new();
            }
            return _parentInputHandler;
        }
    }

    /* SIGNALS */
    public readonly Signal OnTreeEntered = new();
    public readonly Signal OnTreeExited = new();
    public readonly Signal OnTreeMoved = new();
    public readonly Signal OnChildChanged = new();
    public readonly Signal OnParentChanged = new();

    public Node()
    {
        NID = ResourcesService.CreateNewNode(this);
        if (this is ICanvasItem) DrawService.CreateCanvasItem(NID);
        Init_();
    }

    #region vitual methods
    public bool RunProcess(double deltaT)
    {
        if (Freeled) throw new ApplicationException("A already freeled node are" +
        "being referenciated and Process are being alled!");

        if (!_isReady && readyEnabled)
        {
            OnReady();
            Ready();
            _isReady = true;
        }
        if (processEnabled) Process(deltaT);

        return processEnabled;
    }
    public void RunDraw(double deltaT)
    {
        if (Freeled) throw new ApplicationException("A already freeled node are" +
        "being referenciated and Draw are being alled!");

        Draw(deltaT);
    }
    public bool RunInputEvent(InputEvent e)
    {
        if (Freeled) throw new ApplicationException("A already freeled node are" +
        "being referenciated and InputEvent are being called!");

        if (inputEnabled) OnInputEvent(e);

        return inputEnabled;
    }

    protected virtual void Init_() {}
    protected virtual void Ready() {}
    protected virtual void Process(double deltaT) {}
    protected virtual void Draw(double deltaT) {}
    protected virtual void OnInputEvent(InputEvent e) {}
    #endregion

    private void OnReady()
    {
        foreach (var i in _fieldsToLoadWhenReady)
        {
            var field = GetType().GetField(i.Key);
            if (field != null)
            {
                if (!field.FieldType.IsAssignableTo(typeof(Node)))
                    field.SetValue(this, i.Value);
                else
                {
                    var node = GetChild((string)i.Value!);
                    field.SetValue(this, node);

                    if (node == null)
                    Console.WriteLine("Node \"{0}\" can't find node in path \"{1}\"", this.name, i.Value);
                }
                continue;
            }
            var prop = GetType().GetProperty(i.Key);
            if (prop != null)
            {
                if (!prop.PropertyType.IsAssignableTo(typeof(Node)))
                    prop.SetValue(this, i.Value);
                else
                {
                    var node = GetChild((string)i.Value!);
                    prop.SetValue(this, node);

                    if (node == null)
                    Console.WriteLine("Node \"{0}\" can't find node in path \"{1}\"", this.name, i.Value);
                }
                continue;
            }
        }
    }

    public void AddAsChild(Node node)
    {
        // Remove the node from the old parent if it as one
        node.parent?.children.Remove(node);

        node.parent = this;
        node._isGhostChildren = false;

        //verify if name isn't empty
        if (node.name == "")
        node.name = node.GetType().Name + "_" + node.GetHashCode();

        // Verify if theres no other child with the ssame name
        var count = 0;
        while (true)
        {
            if (children.Find(e => e.name == node.name + (count>0?("_" + count):"")) == null)
            {
                if (count>0) node.name += "_" + count; 
                break;
            }
            count++;
        }

        node.AddedAsChild(this);

        node.NotifyChangeAllChildrens();
        NotifyChangeAllParents();

        children.Add(node);
    }
    protected void AddAsGhostChild(Node node)
    {
        // Remove the node from the old parent if it as one
        if (!node._isGhostChildren)
            node.parent?.children.Remove(node);
        
        else if (parent != null)
        {
            var idx = parent.ghostChildren.FindIndex(e =>
            {e.TryGetTarget(out var n); return n == this;});
            node.parent?.ghostChildren.RemoveAt(idx);
        }

        node.parent = this;
        node._isGhostChildren = true;

        ghostChildren.Add(new(node));
    }
   
    public Node? GetChild(string path)
    { return GetChild(path.Split('/')); }
    public Node? GetChild(string[] path)
    {
        if (path.Length > 1)
        {
            if (path[0] == "..")
                return parent?.GetChild( path.Skip(1).ToArray() );
            else
            {
                var a = children.Find(e => e.name == path[0]);
                return a?.GetChild( path.Skip(1).ToArray() );
            }
        }
        return children.Find(e => e.name == path[0]);
    }

    public bool IsOnRoot() => _isOnTree;

    public virtual void Free()
    {
        if (!Freeled)
        {
            Freeled = true;

            OnTreeExited.Emit(this);
            _isOnTree = false;

            ResourcesService.FreeNode(NID);
            foreach (var i in GetAllChildren.ToArray())
                i.Free();

            if (parent != null)
            {
                if (!_isGhostChildren)
                    parent.children.Remove(this);

                else
                {
                    var idx = parent.ghostChildren.FindIndex(e =>
                    {e.TryGetTarget(out var n); return n == this;});
                    parent?.ghostChildren.RemoveAt(idx);
                }
            }

            parent = null;

            if (!_gcCalled)
                GC.SuppressFinalize(this);
        }
    }
    public void FreeChildren()
    {
        while (children.Count > 0)
        {

            children[0].Free();
        }
    }
    public void FreeGhostChildren()
    {
        while (ghostChildren.Count > 0)
        {
            if (ghostChildren[0].TryGetTarget(out var n))
                n.Free();
        }
    }

    public void AddToOnReady(string field, Object? value)
    {
        _fieldsToLoadWhenReady.Add(field, value);
    }

    #region tree and tree-chain related methods

    public void RequestUpdateAllChildrens() => NotifyChangeAllChildrens();
    public void RequestUpdateAllParents() => NotifyChangeAllParents();

    protected virtual void OnTreeParentChanged()
    {
        _parentViewport = null;
        _parentInputHandler = null;
    }

    private void NotifyChangeAllParents()
    {

        OnChildChanged.Emit();
        parent?.NotifyChangeAllChildrens();

    }
    private void NotifyChangeAllChildrens()
    {

        OnParentChanged.Emit();

        foreach (var c in GetAllChildren)
        {
            c.OnTreeParentChanged();
            c.NotifyChangeAllChildrens();
        }
        
    }

    private void AddedAsChild(Node newParent)
    {
        _isOnTree = newParent is NodeRoot || newParent.IsOnRoot();
    }

    #endregion

    ~Node()
    {
        if (Freeled) return;
        _gcCalled = true;
        Free();
        Console.WriteLine("Node {0} of base {1} is being freeled from GC!" +
        "(Did you forgot to call manually Free()?)", name, GetType().Name);
    }

}
