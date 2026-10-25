namespace ModManager.StarCraft.Utilities.Interface;

public interface IApplicationDataFileSystem
{
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
}