using GameEngine.Util.Resources;

namespace GameEngine.Util.Nodes;

public class TreeGraph : NodeUI
{

    private TreeGraphItem _root;
    public TreeGraphItem Root
    {
        get { return _root; }
    }

    public TreeGraph()
    {
        _root = new(this) {Name = "root"};
    }

    public TreeGraphItem? GetItem(string path)
    {
        var p = path.Split('/').Where(e => e != "").ToArray();
        return _root?.GetChild(p);
    }
    public TreeGraphItem? AddItem(string path, string name, Texture? icon = null)
    {
        var parent = path != "" ? GetItem(path) : _root;

        if (parent != null)
        {
            var n = new TreeGraphItem(this);
            parent.children.Add(n);
            n.parent = parent;
            n.Icon = icon;
            n.Name = name;

            UpdateList();

            return n;
        }
        return null;
    }

    private void UpdateList()
    {
        List<TreeGraphItem> toUpdate = new();
        toUpdate.Add(_root);

        int listIndex = 0;
        while (toUpdate.Count > 0)
        {
            TreeGraphItem current = toUpdate[0];
            toUpdate.RemoveAt(0);

            current.Update(listIndex);
            listIndex++;

            for (int i = current.children.Count - 1; i >= 0; i--)
                toUpdate.Insert(0,  current.children[i]);
        }
    }


    #region inner classes/strucs

    public class TreeGraphItem
    {
        private TreeGraph graph;

        #region variables

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set {
                _name = value;
                title.Text = _name;
            }
        }
        
        private Texture? _icon = null;
        public Texture? Icon
        {
            get { return _icon; }
            set {
                _icon = value;
                Update();
            }
        }
        
        public string Path
        {
            get {
                if (parent != null)
                    return parent.Path + "/" + Name;
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
        
        private int positionIndex = 0;

        public TreeGraphItem? parent = null;
        public List<TreeGraphItem> children = new();

        /*******/
        private Pannel container = new()
        {
            sizePercent = new(1, 0),
            sizePixels = new(0, 40),
            backgroundColor = new(0,0,0,0f)
        };
        private TextureRect icon = new()
        {
            sizePercent = new(0,0),
            sizePixels = new(20, 20),
            positionPixels = new(10, 10),
            Visible = false
        };
        private Label title = new()
        {
            verticalAligin = Label.Aligin.Center,
            color = new(1f, 1f, 1f)
        };
        /*******/

        #endregion

        public TreeGraphItem(TreeGraph graph)
        {
            this.graph = graph;

            graph.AddAsChild(container);
            container.AddAsChild(icon);
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
    
        public void Update(int pIndex = 0)
        {
            positionIndex = pIndex;
            container.positionPixels.Y = (int) (container.Size.Y * positionIndex);

            int level = Path.Split('/').Length - 1;
            container.positionPixels.X = 40 * level;

            if (_icon != null)
            {
                title.positionPixels.X = 40;
                icon.Show();
            } else
            {
                title.positionPixels.X = 0;
                icon.Hide();
            }

            icon.texture = _icon;
        }
    }

    #endregion

}