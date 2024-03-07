using System.Diagnostics.CodeAnalysis;
using GameEngine.Core;

namespace GameEngine.Util;

public struct FileReference
{
    private string _globalPath;

    public readonly string GlobalPath => _globalPath;
    public readonly string RelativePath => FileService.GetProjRelativePath(_globalPath);

    public FileReference(string path)
    {
        _globalPath = FileService.GetGlobalPath(path);
    }

    /* CHECK */
    public readonly bool Exists => File.Exists(_globalPath);

    /* READ */
    public readonly string ReadAllFile()
    {
        return FileService.GetFile(_globalPath);
    }
    public readonly string[] ReadFileLines()
    {
        return FileService.GetFileLines(_globalPath);
    }

    /* WRITE */
    public readonly void Write(string content)
    {
        FileService.WriteFile(_globalPath, content);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is FileReference fileRef)
            return fileRef.GlobalPath == GlobalPath;

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
        return string.Format("path(\"{0}\")", _globalPath);
    }

}