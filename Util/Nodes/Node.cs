using GameEngine.Sys;
using GameEngine.Util.Interfaces;

namespace GameEngine.Util.Nodes;

public class Node
{
    private bool _freeled = false;

    public readonly uint RID = 0;

    public Node? parent;
    public List<Node> children = new();

    public string name = "";

    public Node()
    {
        RID = ResourcesService.CreateNewResouce();
        if (this is ICanvasItem) DrawService.CreateCanvasItem(RID);
    }

    public void RunInit()
    {
        Init_();
        Ready();
    }
    public void RunDraw(double deltaT)
    {
        Draw(deltaT);
    }

    protected virtual void Init_() {}
    protected virtual void Ready() {}
    protected virtual void Process(double deltaT) {}
    protected virtual void Draw(double deltaT) {}

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
    public Node? GetChild(string name)
    {
        return children.Find(e => e.name == name);
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

    ~Node()
    {
        Free(true);
        // Alert of insecure dispose of the class
    }
}
