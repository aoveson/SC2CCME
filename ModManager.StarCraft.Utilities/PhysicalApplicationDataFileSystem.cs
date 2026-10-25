using ModManager.StarCraft.Utilities.Interface;

namespace ModManager.StarCraft.Utilities;

public sealed class PhysicalApplicationDataFileSystem : IApplicationDataFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
}