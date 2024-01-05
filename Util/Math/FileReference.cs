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

    public readonly string ReadAllFile()
    {
        return FileService.GetFile(path);
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

    public override string ToString()
    {
        return string.Format("path(\"{0}\")", path);
    }

}