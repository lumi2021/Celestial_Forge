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

    /* Script & load variables */
    private Dictionary<string, object?> _fieldsToLoadWhenReady = new();

    public Node? parent;

    public List<Node> children = [];
    protected List<Node> ghostChildren = [];

    public string name = "";

    private Viewport? _parentViewport;
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
            if (Viewport is Window win)
                return win.input;
            else return Viewport!.Input;
        }
    }


    public Node()
    {
        NID = ResourcesService.CreateNewNode(this);
        if (this is ICanvasItem) DrawService.CreateCanvasItem(NID);
        Init_();
    }

    #region vitual methods
    public void RunProcess(double deltaT)
    {
        if (Freeled) throw new ApplicationException("A already freeled node are" +
        "being referenciated and Process are being alled!");

        if (!_isReady)
        {
            OnReady();
            Ready();
            _isReady = true;
        }
        Process(deltaT);
    }
    public void RunDraw(double deltaT)
    {
        if (Freeled) throw new ApplicationException("A already freeled node are" +
        "being referenciated and Draw are being alled!");

        Draw(deltaT);
    }
    public void RunInputEvent(InputEvent e)
    {
        if (Freeled) throw new ApplicationException("A already freeled node are" +
        "being referenciated and InputEvent are being called!");

        OnInputEvent(e);
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

        children.Add(node);
    }
    protected void AddAsGhostChild(Node node)
    {
        // Remove the node from the old parent if it as one
        if (node.parent != null)
            node.parent.children.Remove(node);

        node.parent = this;

        // Verify if theres no other child with the same name
        var count = 0;
        while (true)
        {
            if (ghostChildren.Find(e => e.name == node.name + (count>0?("_" + count):"")) == null)
            {
                if (count>0) node.name += "_" + count; 
                break;
            }
            count++;
        }

        ghostChildren.Add(node);
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

    protected Node? GetGhostChild(string path)
    { return GetGhostChild(path.Split('/')); }
    protected Node? GetGhostChild(string[] path)
    {
        if (path.Length > 1)
        {
            var a = ghostChildren.Find(e => e.name == path[0]);
            return a?.GetChild( path.Skip(1).ToArray() );
        }
        return ghostChildren.Find(e => e.name == path[0]);
    }

    public bool IsOnRoot()
    {
        if (GetType() == typeof(NodeRoot)) return true;
        if (parent != null) return parent.IsOnRoot();
        return false;
    }

    public virtual void Free()
    {
        if (!Freeled)
        {
            Freeled = true;

            ResourcesService.FreeNode(NID);
            foreach (var i in children.ToArray()) i.Free();
            parent?.children.Remove(this);
            parent = null;

            if (!_gcCalled) GC.SuppressFinalize(this);
        }
    }

    public void AddToOnReady(string field, Object? value)
    {
        _fieldsToLoadWhenReady.Add(field, value);
    }

    ~Node()
    {
        _gcCalled = true;
        Free();
        Console.WriteLine("Node {0} of base {1} is being freeled from GC!" +
        "(Did you forgot to call manually Free()?)", name, GetType().Name);
    }
}
