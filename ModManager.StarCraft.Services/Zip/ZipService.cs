using System.IO;
using ModManager.StarCraft.Services.Log;
using ModManager.StarCraft.Services.Log.LogTypes;
using ModManager.StarCraft.Services.Zip.Interface;

namespace ModManager.StarCraft.Services.Zip;

/// <summary>
/// Service for handling zip files.
/// </summary>
public static class ZipService
{
    /// <summary>Gets or sets the filesystem used for ZIP operations.</summary>
    public static IZipFileSystem FileSystem { get; set; } = new PhysicalZipFileSystem();

    /// <summary>
    /// The path to the custom campaigns directory within the SC2 installation.
    /// </summary>
    public static string CUSTOM_CAMPAIGNS_PATH {get;} = Path.Combine("Maps", "CustomCampaigns");

    /// <summary>The destination path for the Legacy of the Void prologue directory.</summary>
    public static string LOTV_PROLOGUE_PROPER_NAME {get;} = Path.Combine("Maps", "Campaign", "voidprologue");

    /// <summary>The directory name used by archives before prologue relocation.</summary>
    public static string LOTV_PROLOGUE_DIRTY_NAME = "lotvprologue";

    /// <summary>
    /// Unzips any .zip files found in the custom campaigns directory.
    /// This is called on reload (on button, on drag, or on start).
    /// </summary>
    /// <param name="path">The path to unzip zips in</param>
    /// <returns></returns>
    public static async Task UnzipZipsAsync(string path)
    {
        string[] files = FileSystem.GetFiles(Path.Combine(path, CUSTOM_CAMPAIGNS_PATH));
        foreach (string file in files)
        {
            if (file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                // Establish variables.
                string modFolderName = Path.GetFileNameWithoutExtension(file);
                string zipExtractPath = Path.Combine(path, CUSTOM_CAMPAIGNS_PATH, modFolderName);
                string renameLotvPrologueDestDir = Path.Combine(path, LOTV_PROLOGUE_PROPER_NAME);
                FileSystem.SetNormalAttributes(file);

                await FileSystem.ExtractToDirectoryAsync(file, zipExtractPath);
                await Service.Get<Logger>().LogAsync<ZipInfo>([file, zipExtractPath]);

                // Move any LOTV Prologue directories to the proper location.
                string[] subdirs = FileSystem.GetDirectories(zipExtractPath, LOTV_PROLOGUE_DIRTY_NAME);
                foreach (string dir in subdirs)
                {
                    FileSystem.MoveDirectory(dir, renameLotvPrologueDestDir);
                    await Service.Get<Logger>().LogAsync<DirectoryMoved>([
                        dir, renameLotvPrologueDestDir, "Moved a LOTV Prologue directory to the proper location."
                    ]);
                }

                try
                {
                    // Delete the zip file after extraction.
                    FileSystem.DeleteFile(file);
                }
                catch (IOException ex)
                {
                    await Service.Get<Logger>().LogAsync<IOError>([$"Could not delete zip file {file} - file in use.", ex.ToString()]);
                }
            }
        }
    }
}
