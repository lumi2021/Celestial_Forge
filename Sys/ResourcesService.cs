namespace GameEngine.Sys;

public static class ResourcesService
{

    private static uint _lastRID = 0;

    public static uint CreateNewResouce()
    { return _lastRID++; }

    public static bool IsValid(uint RID)
    { return RID > 0 && RID < _lastRID; }

}