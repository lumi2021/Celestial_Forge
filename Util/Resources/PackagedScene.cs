using GameEngine.Util.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameEngine.Util.Resources;

public class PackagedScene : Resource
{

    private string path = "";
    private PackagedNode? root;

    private PackagedScene(string jsonData, string path)
    {
        this.path = path;

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Converters = { new PackagedNodeFileConverter() }
        };

        var unpakaged = JsonConvert.DeserializeObject<PackagedNode>(jsonData);

        if (unpakaged == null)
            throw new ApplicationException("PackagedScene file corrupted!");

        root = unpakaged;
    }

    public static PackagedScene Load(string path)
    {
        var data = File.ReadAllText("../../../" + path);
        return new PackagedScene(data, path);
    }

    public Node Instantiate()
    {
        return root!.CreateNodeInstance();
    }

    /*
    INNER CLASSES
    */

    class PackagedNode
    {
        public Type NodeType {get;set;} = typeof(Node);
        public string Name {get;set;} = "";
        public Dictionary<string, object?> data = new();
        public PackagedNode[] Children {get;set;} = Array.Empty<PackagedNode>();

        public Node CreateNodeInstance()
        {
            var newNode = (Node) Activator.CreateInstance(NodeType)!;

            /* TODO PackagedNode
            ** implement logic to load this node and
            ** their data correctly
            */

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

                throw new ApplicationException(string.Format("Field {0} don't exist!", i.Key));

            }
            
            foreach (var i in Children)
                newNode.AddAsChild(i.CreateNodeInstance());
            
            return newNode;

        }
    }

    class PackagedNodeFileConverter : JsonConverter<PackagedNode>
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
                        var obj = (field != null)?
                        Activator.CreateInstance(field?.FieldType!, i.Value.Values<float>().ToArray()) :
                        Activator.CreateInstance(prop?.PropertyType!, i.Value.Values<float>().ToArray());

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
                    PackagedNode? a = JsonConvert.DeserializeObject<PackagedNode>(i.ToString());
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

}