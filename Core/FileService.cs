using System;

namespace GameEngine.Core;

public static class FileService
{
    /* READ */
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
    public static string[] GetFileLines(string path)
    {
        var gPath = GetGlobalPath(path);
        try
        {
            return File.ReadLines(gPath).ToArray();
        }
        catch(Exception e)
        {
            throw new ApplicationException("File can't be loaded!", e);
        }
    }

    /* WRITE */
    public static void WriteFile(string path, string content)
    {
        var gPath = GetGlobalPath(path);
        try
        {
            File.WriteAllText(path, content);
        }
        catch(Exception e)
        {
            throw new ApplicationException("File can't be loaded!", e);
        }
    }

    public static FileSystemInfo[] GetDirectory(string path)
    {
        var gPath = GetGlobalPath(path);

        DirectoryInfo info = new DirectoryInfo(gPath);
        FileSystemInfo[] itens = info.GetFileSystemInfos();

        return itens;
    }

    public static string GetProjRelativePath(string path)
    {
        string p = path.Replace("\\", "/");

        if (p.StartsWith(Engine.projectSettings.projectPath))
            return string.Concat("res://", p.AsSpan(Engine.projectSettings.projectPath.Length));

        return p;
    }
    public static string GetGlobalPath(string path)
    {
        string p = path.Replace("\\", "/");

        if (p.StartsWith("res://"))
            p = Engine.projectSettings.projectPath
            + p[6..];
        
        else if (p.StartsWith("c:/") || p.StartsWith("C:/"))
            return p;

        else
            p ="../../../" + p;

        return p;
    }
}
