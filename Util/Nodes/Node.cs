using GameEngine.Sys;
using GameEngine.Util.Interfaces;

namespace GameEngine.Util.Nodes;

public class Node
{
    /* System variables */
    private bool _freeled = false;
    private bool _isReady = false;
    public readonly uint RID = 0;

    /* Script & load variables */
    private Dictionary<string, object?> _fieldsToLoadWhenReady = new();

    
    public Node? parent;
    public List<Node> children = new();

    public string name = "";


    public Node()
    {
        RID = ResourcesService.CreateNewResouce();
        if (this is ICanvasItem) DrawService.CreateCanvasItem(RID);
        Init_();
    }

    public void RunProcess(double deltaT)
    {
        if (!_isReady)
        {
            Ready();
            OnReady();
            _isReady = true;
        }
        Process(deltaT);
    }
    public void RunDraw(double deltaT)
    {
        Draw(deltaT);
    }


    protected virtual void Init_() {}
    protected virtual void Ready() {}
    protected virtual void Process(double deltaT) {}
    protected virtual void Draw(double deltaT) {}

    private void OnReady()
    {
        foreach (var i in _fieldsToLoadWhenReady)
        {
            var field = GetType().GetField(i.Key);
            if (field != null)
            {
                if (!field.FieldType.IsAssignableTo(typeof(Node)))
                    field.SetValue(this, i.Value);
                else {
                    var node = GetChild((string)i.Value!);
                    field.SetValue(this, node);
                }
                continue;
            }
            var prop = GetType().GetProperty(i.Key);
            if (prop != null)
            {
                if (!prop.PropertyType.IsAssignableTo(typeof(Node)))
                    prop.SetValue(this, i.Value);
                else {
                    var node = GetChild((string)i.Value!);
                    prop.SetValue(this, node);
                }
                continue;
            }
        }
    }

    public void AddAsChild(Node node)
    {
        // Remove the node from the old parent if it as one
        if (node.parent != null)
            node.parent.children.Remove(node);

        node.parent = this;

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

    public bool IsOnRoot()
    {
        if (GetType() == typeof(NodeRoot)) return true;
        if (parent != null) return parent.IsOnRoot();
        return false;
    }

    // TODO configure the correct node and children data dispose
    public virtual void Free(bool fromGC=false)
    {
        if (!_freeled)
        {
            _freeled = true;

            if (!fromGC) GC.SuppressFinalize(this);
        }
    }

    public void AddToOnReady(string field, Object? value)
    {
        _fieldsToLoadWhenReady.Add(field, value);
    }


    ~Node()
    {
        Free(true);
        // Alert of insecure dispose of the class
    }
}
