using GameEngine.Util.Nodes;
using GameEngineEditor.Editor;

namespace GameEngine.Util.Resources;

public class EditorPlugin : Resource
{

    public bool active = false;

    public virtual bool Start() => false;
    public virtual void Destroy() {}

    protected static void AddCustomType(Type type, Type? typeParent)
    {
        Editor.RequestRegisterType(type, typeParent);
    }
    protected static void AddCustomNode(Type nodeType, Type? nodeParent)
    {
        if (nodeType.IsAssignableTo(typeof(Node)))
            Editor.RequestRegisterType(nodeType, nodeParent);  
    }
    protected static void AddCustomResource(Type resourceType, Type? resourceParent)
    {
        if (resourceType.IsAssignableTo(typeof(Resource)))
            Editor.RequestRegisterType(resourceType, resourceParent);  
    }

}
