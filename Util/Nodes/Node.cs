namespace GameEngine.Util.Nodes;

public class Node
{
    private bool _freeled = false;

    public Node? parent;
    public List<Node> children = new();

    public string name = "";

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

    public virtual void Free() {
        _freeled = true;
    }

    ~Node()
    {
        Free();
        if (_freeled)
        {
            // Alert of insecure dispose of the class
        }
    }
}
