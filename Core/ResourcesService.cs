using GameEngine.Util.Nodes;
using GameEngine.Util.Resources;

namespace GameEngine.Core;

public static class ResourcesService
{

    private static readonly Dictionary<uint, WeakReference<Node>> NidTable = [];
    private static readonly Dictionary<uint, WeakReference<Resource>> RidTable = [];

    public static uint CreateNewNode(Node nodeRef)
    {
        var id = FindFirstNullNidKey();
        NidTable.Add(id, new(nodeRef) );
        return id;
    }
    public static uint CreateNewResouce(Resource resRef)
    {
        var id = FindFirstNullRidKey();
        RidTable.Add(id, new(resRef) );
        return id;
    }

    public static void FreeNode(uint NID)
    {
        NidTable.Remove(NID);
        DrawService.DeleteCanvasItem(NID);
    }
    public static void FreeResouce(uint RID)
    {
        RidTable.Remove(RID);
    }

    public static bool IsNodeValid(uint NID)
    { return NidTable.ContainsKey(NID); }
    public static bool IsResourceValid(uint NID)
    { return RidTable.ContainsKey(NID); }


    private static uint FindFirstNullNidKey()
    {
        for (uint i = 0; i < NidTable.Count; i++)
        if (!NidTable.ContainsKey(i)) return i;
        return (uint) NidTable.Count;
    }

    private static uint FindFirstNullRidKey()
    {
        for (uint i = 0; i < RidTable.Count; i++)
        if (!RidTable.ContainsKey(i)) return i;
        return (uint) RidTable.Count;
    }

}