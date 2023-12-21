using GameEngine.Util.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameEngine.Util.Resources;

public class PackagedScene : Resource
{

    private string path = "";
    private PackagedNode? root;
    private PackagedResource[] resources;

    private PackagedScene(PackageSceneBase basePScene, string path)
    {
        this.path = path;

        if (basePScene.root == null)
            throw new ApplicationException("PackagedScene file corrupted!");

        root = basePScene.root;
        resources = basePScene.resources;
    }

    public static PackagedScene Load(string path)
    {
        var data = File.ReadAllText("../../../" + path);

        var settings = new JsonSerializerSettings
        {
            Converters = { new PackagedSceneFileConverter() }
        };

        var unpakaged = JsonConvert.DeserializeObject<PackageSceneBase>(data, settings);

        return new PackagedScene(unpakaged, path);
    }

    public Node Instantiate()
    {
        if (root != null)
            return root.CreateNodeInstance();
        
        else return new Node();
    }

    /*
    INNER CLASSES
    */

    struct PackageSceneBase
    {
        public PackagedNode? root = null;
        public PackagedResource[] resources = Array.Empty<PackagedResource>();

        public PackageSceneBase() {}
    }
    class PackagedNode
    {
        public Type NodeType {get;set;} = typeof(Node);
        public string Name {get;set;} = "";
        public Dictionary<string, object?> data = new();
        public PackagedNode[] Children {get;set;} = Array.Empty<PackagedNode>();

        public Node CreateNodeInstance()
        {
            var newNode = (Node) Activator.CreateInstance(NodeType)!;

            newNode.name = Name;

            foreach (var i in data)
            {
                var field = NodeType.GetField(i.Key);
                if (field != null)
                {
                    if (!field.FieldType.IsAssignableTo(typeof(Node)))
                        field.SetValue(newNode, Convert.ChangeType(i.Value, field.FieldType));
                    else newNode.AddToOnReady(i.Key, i.Value);
                    continue;
                }

                var prop = NodeType.GetProperty(i.Key);
                if (prop != null)
                {
                    if (!prop.PropertyType.IsAssignableTo(typeof(Node)))
                        prop.SetValue(newNode, Convert.ChangeType(i.Value, prop.PropertyType));
                    else newNode.AddToOnReady(i.Key, i.Value);
                    continue;
                }

                throw new ApplicationException(string.Format("Field {0} don't exist in type {1}!",
                i.Key, NodeType.Name));

            }
            
            foreach (var i in Children)
                newNode.AddAsChild(i.CreateNodeInstance());
            
            return newNode;

        }
    }
    class PackagedResource
    {

        public Type ResType {get;set;} = typeof(Resource);
        public Dictionary<string, object?> data = new();

        public Resource CreateResourceInstance()
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


    class PackagedSceneFileConverter : JsonConverter<PackageSceneBase>
    {
        public override PackageSceneBase ReadJson(JsonReader reader, Type objectType, PackageSceneBase existingValue, bool hasExistingValue, JsonSerializer serializer)
        {

            JObject jsonObject = JObject.Load(reader);
            
            var pSceneBase = new PackageSceneBase();

            var tree = jsonObject.GetValue("NodeTree");
            var resources = jsonObject.GetValue("Resources");

            // Load Node Tree data
            if (tree != null)
            {
                var settings = new JsonSerializerSettings
                {
                    Converters = { new PackagedNodeConverter() }
                };
                pSceneBase.root = JsonConvert.DeserializeObject<PackagedNode>(tree.ToString(), settings);
            }

            
            // Load Resources data
            if (resources != null)
            {
                var resList = new List<PackagedResource>();

                var settings = new JsonSerializerSettings
                {
                    Converters = { new PackagedResourceConverter() }
                };

                foreach (var i in resources)
                {
                    var res = JsonConvert.DeserializeObject<PackagedResource>(i.ToString(), settings);
                    if (res != null) resList.Add(res);
                }

                pSceneBase.resources = resList.ToArray();
            }
            

            return pSceneBase;

        }

        public override void WriteJson(JsonWriter writer, PackageSceneBase value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    class PackagedNodeConverter : JsonConverter<PackagedNode>
    {

        public override PackagedNode? ReadJson(JsonReader reader, Type objectType, PackagedNode? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            // Load type ...
            Type? t = Type.GetType("GameEngine.Util.Nodes." + jsonObject.GetValue("NodeType")!.Value<string>()!);
            // and see if it's valid
            if (t != null)
            {
                // Create the instance and preprocess main data
                PackagedNode node = new();
                node.NodeType = t;
                node.Name = jsonObject.GetValue("Name")?.Value<string>()!;

                var children = jsonObject.GetValue("Children");

                /*
                LOAD Data
                */
                string[] mainData = new string[] {"NodeType", "Name", "Children"};
                foreach (var i in jsonObject)
                {
                    if (mainData.Contains(i.Key)) continue;
                    
                    var field = t.GetField(i.Key);
                    var prop = t.GetProperty(i.Key);

                    // Check if data field exists.
                    if (field == null && prop == null) // if not, throw a error
                        throw new ApplicationException(string.Format("Field {0} don't exist in base {1}!",
                        i.Key, t.Name));

                    if (i.Value is JArray) // Unpack data in arrays
                    {
                        var obj = Activator.CreateInstance(field?.FieldType!, i.Value.Values<float>().ToArray());
                        if (obj!=null) node.data.Add(i.Key, obj);
                        
                    } else
                    {
                        if (
                            (field != null && field.FieldType.IsAssignableTo(typeof(Node))) ||
                            (prop != null && prop.PropertyType.IsAssignableTo(typeof(Node)))
                        )
                        {
                            node.data.Add(i.Key, i.Value!.Value<string>());
                            continue;
                        }

                        node.data.Add(i.Key, i.Value);
                    }
                }

                /*
                LOAD CHILDREN
                */
                List<PackagedNode> childrenList = new();

                if (children != null)
                foreach (var i in children)
                {
                    var settings = new JsonSerializerSettings
                    {
                        Converters = { new PackagedNodeConverter() }
                    };
                    PackagedNode? a = JsonConvert.DeserializeObject<PackagedNode>(i.ToString(), settings);
                    if (a != null) childrenList.Add(a);
                }
                // Convert the list to a static array
                node.Children = childrenList.ToArray();

                /*
                RETURN THE PackagedNode RESULT
                */
                return node;
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, PackagedNode? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

    }
    
    class PackagedResourceConverter : JsonConverter<PackagedResource>
    {

        public override PackagedResource? ReadJson(JsonReader reader, Type objectType, PackagedResource? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            
            JObject jsonObject = JObject.Load(reader);

            // Load type ...
            Type? t = Type.GetType("GameEngine.Util.Resources." + jsonObject.GetValue("ResourceType")!.Value<string>()!);
            // and see if it's valid
            if (t != null)
            {
                PackagedResource nRes = new();
                nRes.ResType = t;

                /*
                LOAD Data
                */
                string[] mainData = new string[] {"ResourceType"};
                foreach (var i in jsonObject)
                {
                    if (mainData.Contains(i.Key)) continue;
                    
                    var field = t.GetField(i.Key);
                    var prop = t.GetProperty(i.Key);

                    // Check if data field exists.
                    if (field == null && prop == null) // if not, throw a error
                        throw new ApplicationException(string.Format("Field {0} don't exist in base {1}!",
                        i.Key, t.Name));

                    if (i.Value is JArray) // Unpack data in arrays
                    {
                        var obj = Activator.CreateInstance(field?.FieldType!, i.Value.Values<float>().ToArray());
                        if (obj!=null) nRes.data.Add(i.Key, obj);
                        
                    }
                    else nRes.data.Add(i.Key, i.Value);
                }

                return nRes;
            }

            return null;

        }

        public override void WriteJson(JsonWriter writer, PackagedResource? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

    }

}