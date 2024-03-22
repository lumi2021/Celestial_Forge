using GameEngine.Core;
using GameEngine.Core.Scripting;
using GameEngine.Util.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameEngine.Util.Resources;

public class PackagedScene : Resource
{

    public string Path { get; private set; }
    private PackagedNode root;
    private PackagedResource[] resources;
    private Script[] scriptsToCompile = [];

    private PackagedScene(string path, PackagedNode root, PackagedResource[] resources, FileReference[] scripts)
    {
        List<Script> stcList = [];
        foreach(var i in scripts)
            stcList.Add(new(i, "cs"));
        
        scriptsToCompile = [.. stcList];

        Path = path;
        this.root = root;
        this.resources = resources;
    }

    public static PackagedScene? Load(string path)
    {
        string data = FileService.GetFile(path);

        var settings = new JsonSerializerSettings
        { Converters = [new PackagedSceneFileConverter()] };

        return JsonConvert.DeserializeObject<PackagedScene>(data, settings);
    }

    public Node Instantiate()
    {
        ScriptService.Compile([.. scriptsToCompile]);

        List<Resource> resRepository = [];

        foreach (var i in resources)
            resRepository.Add( i.CreateResourceInstance() );

        var res = root.CreateNodeInstance( [.. resRepository] );

        GC.Collect();

        return res;
    }

    struct PackagedNode
    {
        public string ScriptPath {get;set;} = null!;

        public bool alreadyCompiled = true;
        public Type NodeType {get;set;} = null!;
        public string NodeTypeName {get;set;} = "";

        public string Name {get;set;} = "";
        public Dictionary<string, JToken?> rawData = [];
        public Dictionary<string, object?> data = [];
        public PackagedNode[] Children {get;set;} = [];

        public PackagedNode() {}

        public Node CreateNodeInstance( Resource[] resRepo )
        {
            Node newNode;

            if (alreadyCompiled && NodeType != null)
                newNode = (Node) Activator.CreateInstance(NodeType)!;

            else
            {
                var t = ScriptService.GetDinamicCompiledType(NodeTypeName);
                if (t != null)
                {
                    NodeType = t;
                    newNode = (Node) Activator.CreateInstance(t)!;
                    alreadyCompiled = true;

                    foreach (var i in rawData)
                    {
                        var res = RawData2FinalData(i, NodeType);
                        data.Add(res.Key, res.Value);
                    }
                }

                else throw new Exception($"Invalid type \"{NodeTypeName}\"");

            }

            foreach (var i in data)
            {
                var field = NodeType.GetField(i.Key);
                if (field != null)
                {
                    if (field.FieldType.IsAssignableTo(typeof(Node)))
                        newNode.AddToOnReady(i.Key, i.Value);

                    else if (field.FieldType.IsAssignableTo(typeof(Resource)))
                        field.SetValue(newNode, resRepo[int.Parse(i.Value!.ToString()!)]);

                    else field.SetValue(newNode, Convert.ChangeType(i.Value, field.FieldType));
                    continue;
                }

                var prop = NodeType.GetProperty(i.Key);
                if (prop != null)
                {
                    if (prop.PropertyType.IsAssignableTo(typeof(Node)))
                        newNode.AddToOnReady(i.Key, i.Value);

                    else if (prop.PropertyType.IsAssignableTo(typeof(Resource)))
                        prop.SetValue(newNode, resRepo[int.Parse(i.Value!.ToString()!)]);

                    else if (i.Value is IConvertible) 
                        prop.SetValue(newNode, Convert.ChangeType(i.Value, prop.PropertyType));
                    else prop.SetValue(newNode, i.Value);

                    continue;
                }

                throw new ApplicationException(string.Format("Field {0} don't exist in type {1}!",
                i.Key, NodeType.Name));

            }


            newNode.name = Name;

            foreach (var i in Children)
                newNode.AddAsChild(i.CreateNodeInstance( resRepo ));
            
            return newNode;

        }
    }
    struct PackagedResource
    {

        public Type ResType {get;set;} = typeof(Resource);
        public Dictionary<string, object?> data = new();

        public PackagedResource() {}

        public readonly Resource CreateResourceInstance()
        {
            var newRes = (Resource) Activator.CreateInstance(ResType)!;

            foreach (var i in data)
            {
                var field = ResType.GetField(i.Key);
                if (field != null)
                {
                    field.SetValue(newRes, Convert.ChangeType(i.Value, field.FieldType));
                    continue;
                }

                var prop = ResType.GetProperty(i.Key);
                if (prop != null)
                {
                    prop.SetValue(newRes, Convert.ChangeType(i.Value, prop.PropertyType));
                    continue;
                }

                throw new ApplicationException(string.Format("Field {0} don't exist in type {1}!",
                i.Key, ResType.Name));

            }

            return newRes;
        }

    }

    private class PackagedSceneFileConverter : JsonConverter<PackagedScene>
    {
        public override PackagedScene? ReadJson(JsonReader reader, Type objectType, PackagedScene? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            
            JObject json = JObject.Load(reader);

            PackagedNode root = new();
            List<PackagedResource> resources = [];
            List<FileReference> scriptsToCompile = [];

            // Load Tree
            if (json["NodeTree"] != null && json["NodeTree"] is JObject)
            {
                
                var treeResult = LoadPackagedNodeFromJson((JObject) json["NodeTree"]!, out var scripts);
                scriptsToCompile.AddRange(scripts);
                if (treeResult != null) root = (PackagedNode) treeResult;

            } else throw new ApplicationException("PackagedScene File is corrupted!");

            // Load Resources
            if (json["Resources"] != null && json["Resources"] is JArray)
            {
                foreach (var i in ((JArray)json["Resources"]!)!)
                if (i is JObject @object)
                {
                    var res = LoadPackagedResourceFromJson(@object, out var script);
                    if (script.HasValue) scriptsToCompile.Add(script.Value);
                    if (res != null) resources.Add( (PackagedResource) res );
                }
            }

            return new PackagedScene("", root, [.. resources], [.. scriptsToCompile]);

        }

        public override void WriteJson(JsonWriter writer, PackagedScene? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private PackagedNode? LoadPackagedNodeFromJson(JObject data, out FileReference[] scripts)
        {
            scripts = [];

            PackagedNode? referedScene = null;

            bool alreadyCompiledClass = true;
            Type? t = null;
            string tname = "";
            
            if (data.TryGetValue("NodeType", out var tkn1))
                t = Type.GetType("GameEngine.Util.Nodes." + tkn1.Value<string>());

            // compile the script and get the class back
            else if (data.TryGetValue("NodeScript", out var tkn2))
            {
                string[] classRef = tkn2.Value<string>()!.Split('>');
                scripts = [.. scripts, new(classRef[0].Trim())];
                tname = classRef[1].Trim();
                alreadyCompiledClass = false;
            }

            else if (data.TryGetValue("SceneRef", out var tkn3))
            {
                PackagedScene? scene = Load(tkn3.Value<string>()!);
                if (scene != null)
                {
                    
                    scene.root.Name = data.Value<string>("Name")!;
                    referedScene = scene.root;
                    
                    if (scene.root.alreadyCompiled)
                        t = scene.root.NodeType;
                    else
                        tname = scene.root.NodeTypeName;
                
                }
                else return null;
            }

            if (t != null || !alreadyCompiledClass)
            {

                PackagedNode node = referedScene ?? new()
                {
                    Name = data.GetValue("Name")!.ToString()
                };

                if (alreadyCompiledClass)
                {
                    node.NodeType = t!;
                    node.alreadyCompiled = true;
                }
                else
                {
                    node.NodeTypeName = tname;
                    node.alreadyCompiled = false;
                }

                // LOAD DATA
                string[] ignore = ["NodeType", "NodeScript", "SceneRef", "Name", "Children"];
                foreach (var i in data)
                {
                    if (ignore.Contains(i.Key)) continue;

                    if (!node.rawData.TryAdd(i.Key, i.Value)) node.rawData[i.Key] = i.Value;
                    if (t != null)
                    {
                        var res = RawData2FinalData(i, t);
                        if(!node.data.TryAdd(res.Key, res.Value)) node.data[res.Key] = res.Value;
                    }
                }

                // LOAD CHILDREN
                if (referedScene == null)
                {
                    var children = data.GetValue("Children");
                    List<PackagedNode> childrenList = [];

                    if (children != null && children is JArray)
                    foreach( var i in children)
                    {
                        var c = LoadPackagedNodeFromJson((JObject) i, out var childScripts);
                        if (c != null) 
                        {
                            childrenList.Add( (PackagedNode) c );
                            scripts = [.. scripts, .. childScripts];
                        }
                    }

                    node.Children = [.. childrenList];
                }

                return node;

            }

            return null; // Error
        }
        private PackagedResource? LoadPackagedResourceFromJson(JObject data, out FileReference? script)
        {

            script = null;

            Type? t = Type.GetType("GameEngine.Util.Resources." + data.GetValue("ResourceType")!.Value<string>());

            if (t != null)
            {

                PackagedResource res = new()
                {
                    ResType = t
                };

                // LOAD DATA
                string[] ignore = new string[] {"ResourceType"};
                foreach (var i in data)
                {
                    if (ignore.Contains(i.Key)) continue;

                    var field = t.GetField(i.Key);
                    var prop = t.GetProperty(i.Key);

                    // Check if data field exists
                    if (field == null && prop == null)
                        throw new ApplicationException(string.Format("Field {0} don't exist in base {1}!",
                        i.Key, t.Name));
                    
                    if (i.Value is JArray) // Unpack custom values in arrays
                    {
                        var obj = Activator.CreateInstance(
                            field != null ? field.FieldType : prop!.PropertyType,
                            i.Value.Values<float>().ToArray()
                        );
                        
                        if (obj!=null) res.data.Add(i.Key, obj);
                    }
                    else
                        res.data.Add(i.Key, i.Value);
                }

                return res;

            }

            return null; // Error
        }
    }

    private static KeyValuePair<string, object?> RawData2FinalData(KeyValuePair<string, JToken?> i, Type t)
    {

        object? data = null;

        var field = t.GetField(i.Key);
        var prop = t.GetProperty(i.Key);

        // Check if data field exists
        if (field == null && prop == null)
            throw new ApplicationException(string.Format("Field {0} don't exist in base {1}!",
            i.Key, t.Name));
        
        var type = field?.FieldType ?? prop?.PropertyType!;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            type = type.GenericTypeArguments[0];
        
        if (i.Value is JArray) // Unpack custom values in arrays
        {
            var obj = Activator.CreateInstance(type, i.Value.Values<float>().ToArray());
            
            if (obj!=null) data = obj;
        }
        else {
            if (
                (field != null && field.FieldType.IsAssignableTo(typeof(Node))) ||
                (prop != null && prop.PropertyType.IsAssignableTo(typeof(Node)))
            )
                data = i.Value!.ToString();
            
            else data = i.Value;
        }

        return new(i.Key, data);

    }

}