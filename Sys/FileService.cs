namespace GameEngine.Sys;

public static class FileService
{

    public static string GetFile(string path)
    {
        var gPath = GetGlobalPath(path);
        try
        {
            return File.ReadAllText(gPath);
        }
        catch(Exception e)
        {
            throw new ApplicationException("File can't be loaded!", e);
        }
    }

    public static string[] GetDirectory(string path)
    {
        var gPath = GetGlobalPath(path);
        return Directory.GetFiles(gPath);
    }


    public static string GetGlobalPath(string path)
    {
        string p = path;

        if (path.StartsWith("res://"))
            p = Engine.projectSettings.projectPath
            + path.Substring(6);
        
        else if (path.StartsWith("c:/"))
            return path;

        else
            p ="../../../" + path;

        return p;
    }

}
