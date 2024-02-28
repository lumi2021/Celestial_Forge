using System.Diagnostics.CodeAnalysis;
using GameEngine.Core;

namespace GameEngine.Util;

public struct FileReference
{
    public string path;

    public FileReference(string path)
    {
        this.path = path;
    }

    /* READ */
    public readonly string ReadAllFile()
    {
        return FileService.GetFile(path);
    }
    public readonly string[] ReadFileLines()
    {
        return FileService.GetFileLines(path);
    }

    /* WRITE */
    public readonly void Write(string content)
    {
        FileService.WriteFile(path, content);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not FileReference)
            return ((FileReference)obj!).path == path;
        return base.Equals(obj);
    }

    public static bool operator ==(FileReference left, FileReference right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(FileReference left, FileReference right)
    {
        return !(left == right);
    }
    public override readonly int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override readonly string ToString()
    {
        return string.Format("path(\"{0}\")", path);
    }

}