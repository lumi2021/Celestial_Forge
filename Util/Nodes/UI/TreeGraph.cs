namespace GameEngine.Util.Nodes;

public class TreeGraph : NodeUI
{

    private TreeGraphItem root;

    public TreeGraph()
    {
        root = new(this) {Name = "root"};
    }

    public TreeGraphItem? GetItem(string path)
    {
        return root?.GetChild(path.Split('/'));
    }
    public TreeGraphItem? AddItem(string path, string name)
    {
        var parent = path != "" ? GetItem(path) : root;

        if (parent != null)
        {
            var n = new TreeGraphItem(this);
            parent.children.Add(n);
            n.parent = parent;
            n.Name = name;

            n.Update();

            return n;
        }
        return null;
    }

    #region inner classes/strucs

    public class TreeGraphItem
    {
        private TreeGraph graph;

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set {
                _name = value;
                title.Text = _name;
            }
        }
        public string Path
        {
            get {
                if (parent != null)
                    return parent.Path + "\\" + Name;
                else
                    return Name;
            }
        }
        public int Index
        {
            get {
            if (parent != null)
                return parent.children.IndexOf(this);
            else return 0;
            }
        }
        
        public int PositionIndex
        {
            get {
            if (parent != null)
                return parent.Index + Index + 1;
            else return 0;
            }
        }

        public TreeGraphItem? parent = null;
        public List<TreeGraphItem> children = new();

        /*******/
        private Pannel container = new()
        {
            sizePercent = new(1, 0),
            sizePixels = new(0, 40),
            //backgroundColor = new(0,0,0,0f)
        };
        private Label title = new()
        {
            verticalAligin = Label.Aligin.Center
        };
        /*******/

        public TreeGraphItem(TreeGraph graph)
        {
            this.graph = graph;

            graph.AddAsChild(container);
            container.AddAsChild(title);
        }

        public TreeGraphItem? GetChild(string[] path)
        {
            var c = children.Find(e => e.Name == path[0]);
            if (path.Length > 1)
                return c?.GetChild(path.Skip(1).ToArray());
            else
                return c;
        }
        public void RemoveItem()
        {
            foreach (var i in children)
            {
                i.RemoveItem();
            }
            parent?.children.Remove(this);
        }
    
        public void Update()
        {
            container.positionPixels.Y = (int) (container.Size.Y * PositionIndex);
        }
    }

    #endregion
}