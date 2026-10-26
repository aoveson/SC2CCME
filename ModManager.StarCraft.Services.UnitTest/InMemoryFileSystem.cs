using ModManager.StarCraft.Utilities.Interface;

namespace ModManager.StarCraft.Services.UnitTest;

public class InMemoryFileSystem : IFileSystem
{
    public bool DirectoryExists(string path) => false;
    public void CreateDirectory(string path) { }

    public string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        throw new NotImplementedException();
    }

    public string[] GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        throw new NotImplementedException();
    }

    public bool FileExists(string path)
    {
        throw new NotImplementedException();
    }

    public void DeleteFile(string path)
    {
        throw new NotImplementedException();
    }

    public void DeleteDirectory(string path, bool recursive)
    {
        throw new NotImplementedException();
    }

    public void CopyFile(string sourcePath, string destinationPath, bool overwrite)
    {
        throw new NotImplementedException();
    }

    public void MoveFile(string sourcePath, string destinationPath)
    {
        throw new NotImplementedException();
    }

    public void MoveDirectory(string sourcePath, string destinationPath)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<string> ReadLinesAsync(string path)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> ReadAllBytesAsync(string path)
    {
        throw new NotImplementedException();
    }
}