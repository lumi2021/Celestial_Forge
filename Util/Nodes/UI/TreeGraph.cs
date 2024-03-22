using GameEngine.Util.Resources;

namespace GameEngine.Util.Nodes;

public class TreeGraph : NodeUI
{

    private ButtonGroup _buttonGroup = new();

    private TreeGraphItem _root;
    public TreeGraphItem Root => _root;

    public bool _hideRoot = false;
    public bool HideRoot {
        get => _hideRoot;
        set
        {
            _hideRoot = value;
            UpdateList();
        }
    }

    public TreeGraph()
    {
        _root = new(this) {Name = "root"};
    }

    public TreeGraphItem? GetItem(string path)
    {
        var p = path.Split('/').Where(e => e != "").ToArray();
        if (p[0] == "root")
            return _root?.GetChild(p[1 ..]);
        else
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
    public void DeleteItem(string path)
    {
        GetItem(path)?.Delete();
    }

    public void UpdateList()
    {
        List<TreeGraphItem> toUpdate = !_hideRoot ? [_root] : [.. _root.children];

        if (_hideRoot) _root.SelfVisible = false;

        int listIndex = 0;
        while (toUpdate.Count > 0)
        {
            TreeGraphItem current = toUpdate[0];
            toUpdate.RemoveAt(0);

            current.Update(listIndex);
            listIndex++;

            if (!current.Collapsed)
                for (int i = current.children.Count - 1; i >= 0; i--)
                    toUpdate.Insert(0,  current.children[i]);
        }

        RequestUpdateAllChildrens();
    }

    public void ClearGraph()
    {
        _root.Delete();
        _root = new(this) {Name = "root"};
        GC.Collect();
    }


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

        private Texture? _icon = null;
        public Texture? Icon
        {
            get { return _icon; }
            set {
                _icon = value;
                Update();
            }
        }
        
        public string Path => string.Join("/", ArrayPath);
        public string[] ArrayPath
        {
            get
            {
                if (parent != null)
                    return [.. parent.ArrayPath, Name];
                else
                    return [Name];   
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
        
        public int Level
        {
            get
            {
                if (parent != null)
                    return parent.Level + (SelfVisible ? 1 : 0);
            
                return SelfVisible ? 0 : -1;   
            }
        }

        private bool _collapsed = false;
        public bool Collapsed
        {
            get { return _collapsed; }
            set {
                _collapsed = value;
                graph.UpdateList();
                ChildrenVisibility(!value);
            }
        }

        private int positionIndex = 0;

        public TreeGraphItem? parent = null;
        public List<TreeGraphItem> children = [];

        public bool Visible
        {
            get => container.Visible;
            set {
                container.Visible = value;
                ChildrenVisibility(value);
            }
        }
        public bool SelfVisible
        {
            get => container.Visible;
            set { container.Visible = value; }
        }

        /* * * * * * */
        private readonly Button container = new()
        {
            sizePercent = new(1, 0),
            sizePixels = new(0, 32),
            mouseFilter = MouseFilter.Block,
            Togleable = true,

            DefaultBackgroundColor = new(255, 255, 255, 0),
            HoverBackgroundColor = new(255, 255, 255, 0.5f),
            HoverCornerRadius = new(10, 10, 10, 10),
            ActiveBackgroundColor = new(255, 255, 255, 0.25f),

            SelectedBackgroundColor = new(255, 255, 255, 0.25f),
            SelectedCornerRadius = new(10, 10, 10, 10),
            SelectedStrokeColor = new(217, 217, 217),
            SelectedStrokeSize = 2,

            ClipChildren = true
        };
        private readonly TextureRect icon = new()
        {
            sizePercent = new(0,0),
            sizePixels = new(16, 16),
            positionPixels = new(8, 8),
            Visible = false,
            mouseFilter = MouseFilter.Ignore
        };
        private readonly TextField title = new()
        {
            verticalAligin = TextField.Aligin.Center,
            Color = new(1f, 1f, 1f),
            mouseFilter = MouseFilter.Ignore
        };
        /* * * * * * */

        private readonly Dictionary<string, object> _data = [];

        public readonly Signal OnClick = new();

        public TreeGraphItem(TreeGraph graph)
        {
            this.graph = graph;

            graph.AddAsChild(container);
            container.AddAsChild(icon);
            container.AddAsChild(title);

            container.ButtonGroup = graph._buttonGroup;

            container.OnClick.Connect((a,b) => OnClick.Emit(this));
        }

        public TreeGraphItem? GetChild(string[] path)
        {
            var c = children.Find(e => e.Name == path[0]);
            if (path.Length > 1)
                return c?.GetChild(path.Skip(1).ToArray());
            else
                return c;
        }
    
        public void Update(int pIndex = 0)
        {
            positionIndex = pIndex;
            container.positionPixels.Y = (int) (container.Size.Y * positionIndex);

            container.positionPixels.X = 32 * Level;
            container.sizePixels.X = -32 * Level;

            if (_icon != null)
            {
                title.positionPixels.X = 32;
                icon.Show();
            }
            else
            {
                title.positionPixels.X = 0;
                icon.Hide();
            }

            icon.texture = _icon;

            ChildrenVisibility(!_collapsed);
        }
    
        public void ChildrenVisibility(bool visible)
        {
            foreach (var i in children)
            {
                i.Visible = visible;
                i.ChildrenVisibility(visible);
            }
        }

        public void SetData(string name, object value)
        {
            if (_data.TryAdd(name, value))
                _data[name] = value;
        }
        public void RemoveData(string name)
        {
            if (_data.ContainsKey(name))
                _data.Remove(name);
        }
        public dynamic? GetData(string name, dynamic? defaultValue)
        {
            if (_data.TryGetValue(name, out var v))
                return v;
            else
                return defaultValue;
        }
        public dynamic? GetData(string name)
        {
            if (_data.TryGetValue(name, out var v))
                return v;
            else
                return null;
        }

        public void Delete()
        {
            Dispose();
            foreach (var i in children.ToArray()) i.Delete();
            children.Clear();
            parent?.children.Remove(this);
            parent = null;
        }
        ~TreeGraphItem()
        {
            Dispose(true);
        }
        public void Dispose(bool fromGC = false)
        {
            container.Free();
            if (!fromGC) GC.SuppressFinalize(this);
        }
    }

}