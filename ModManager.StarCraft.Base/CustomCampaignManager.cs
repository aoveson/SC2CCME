using ModManager.StarCraft.Base.Enum;
using ModManager.StarCraft.Base.Package;
using ModManager.StarCraft.Services;
using ModManager.StarCraft.Services.Log;
using ModManager.StarCraft.Services.Settings;
using ModManager.StarCraft.Services.Zip;
using ModManager.StarCraft.Services.Log.LogTypes;
using ModManager.StarCraft.Utilities;
using ModManager.StarCraft.Utilities.Interface;

namespace ModManager.StarCraft.Base;

/// <summary>
/// Manages installed and active custom campaign files.
/// </summary>
public static class CustomCampaignManager
{
    /// <summary>
    /// Gets or sets the filesystem used by campaign operations.
    /// </summary>
    public static IFileSystem FileSystem { get; set; } = new PhysicalFileSystem();

    #region Public Methods
    /// <summary>
    /// Replaces the selected campaign files with the files from a mod.
    /// </summary>
    /// <param name="campaign">The campaign to activate.</param>
    /// <param name="mod">The mod whose files should be copied.</param>
    /// <returns><see langword="true"/> when the campaign was activated.</returns>
    public static async Task<bool> SetActiveModAsync(Campaign campaign, Mod mod)
    {
        bool retVal = false;
        string campaignString = GetCampaignString(campaign);
        string modDestPath = GetModPathForCampaign(campaign);
        string modPath = mod.Path;

        if(await ClearDirectoryAsync(modDestPath))
        {
            await CopyFilesAndFoldersAsync(modPath, modDestPath);
            await Service.Get<Logger>().LogAsync<CampaignManagerInfo>([
                $"Set {campaignString} campaign to {mod.Title}!"
            ]);

            retVal = true;
        }
        else
        {
            await Service.Get<Logger>().LogAsync<CampaignManagerError>([
                $"ERROR: Could not set {campaignString} campaign - SC2 files in use."
            ]);
        }

        return retVal;
    }

    /// <summary>
    /// Deletes an installed mod and its containing directory.
    /// </summary>
    /// <param name="mod">The installed mod to delete.</param>
    /// <returns><see langword="true"/> when the mod was deleted.</returns>
    public static async Task<bool> DeleteModFilesAsync(Mod mod)
    {
        bool retVal = false;
        string modPath = mod.Path;
        string? parentPath = Path.GetDirectoryName(modPath);
        while(parentPath != null &&
            !string.Equals(Path.GetFileName(parentPath), "CustomCampaigns", StringComparison.OrdinalIgnoreCase))
        {
            modPath = parentPath;
            parentPath = Path.GetDirectoryName(modPath);
        }

        if(parentPath == null)
        {
            await Service.Get<Logger>().LogAsync<CampaignManagerError>([
                $"ERROR: Could not delete {mod.Title} - its path is outside CustomCampaigns."
            ]);
            return false;
        }

        if(await ClearDirectoryAsync(modPath))
        {
            FileSystem.DeleteDirectory(modPath, false);
            await Service.Get<Logger>().LogAsync<CampaignManagerInfo>([
                $"Deleted {mod.Title} from local storage."
            ]);

            retVal = true;
        }
        else
        {
            await Service.Get<Logger>().LogAsync<CampaignManagerError>([
                $"ERROR: Could not delete {mod.Title} - a file may be open somewhere"
            ]);
        }

        return retVal;
    }

    /// <summary>
    /// Removes custom files from a campaign while preserving built-in campaign directories.
    /// </summary>
    /// <param name="campaign">The campaign to restore.</param>
    /// <returns><see langword="true"/> when the campaign was restored.</returns>
    public static async Task<bool> RestoreToUnmodifiedAsync(Campaign campaign)
    {
        bool retVal = false;
        string campaignString = GetCampaignString(campaign);
        string modPath = GetModPathForCampaign(campaign);

        await Service.Get<Logger>().LogAsync<CampaignManagerInfo>([
            $"Restting {campaignString} campaign to default."
        ]);
        
        if(await ClearDirectoryAsync(modPath))
        {
            await Service.Get<Logger>().LogAsync<CampaignManagerInfo>([
                $"Succesfully reset {campaignString} campaign to default."
            ]);
            retVal = true;
        }
        else
        {
            await Service.Get<Logger>().LogAsync<CampaignManagerError>([
                $"ERROR: Could not set {campaignString} - SC2 files in use."
            ]);
        }
        return retVal;
    }

    /// <summary>
    /// Loads all installed custom campaigns and their metadata.
    /// </summary>
    /// <returns>The campaign lists discovered below the custom campaigns directory.</returns>
    public static async Task<List<ModList>> GetModListsAsync()
    {
        List<ModList> modLists = [
            new(Campaign.None),
            new(Campaign.WoL),
            new(Campaign.HotS),
            new(Campaign.LotV),
            new(Campaign.NCO)
        ];

        string sc2Dir = ApplicationSettings.QuerySettings(x => x.Sc2Dir);
        string searchPath = Path.Combine(sc2Dir, "Maps", "CustomCampaigns");
        string[] dirPaths = FileSystem.GetDirectories(searchPath);
        foreach(string dir in dirPaths)
        {
            string[] files = FileSystem.GetFiles(dir, "metadata.txt", SearchOption.AllDirectories);
            if(files.Length == 0)
            {
                await Service.Get<Logger>().LogAsync<StartupError>([
                    $"FAILED TO LOAD: Unable to find metadata.txt for: {Path.GetFileName(dir)}"
                ]);
            }
            else
            {
                if (files.Length > 1)
                {
                    await Service.Get<Logger>().LogAsync<StartupWarning>([
                        $"WARNING: Multiple metadata.txt found for {dir}"
                    ]);
                }

                Mod mod = await ProcessModFromMetadataAsync(files[0]);
                
                if(modLists.Where(x => x.ModListCampaign == mod.Campaign)?.FirstOrDefault() is ModList modList)
                {
                    modList.Mods.Add(mod);
                }
            }
        }

        return modLists;
    }

    /// <summary>
    /// Loads metadata for the currently active campaign mods.
    /// </summary>
    /// <returns>The active mods found in each campaign directory.</returns>
    public static async Task<List<Mod>> GetActiveModsAsync()
    {
        List<Mod> activeMods = [];
        string wolPath = Path.Combine(GetModPathForCampaign(Campaign.WoL), "metadata.txt");
        string hotsPath = Path.Combine(GetModPathForCampaign(Campaign.HotS), "metadata.txt");
        string lotvPath = Path.Combine(GetModPathForCampaign(Campaign.LotV), "metadata.txt");
        string ncoPath = Path.Combine(GetModPathForCampaign(Campaign.NCO), "metadata.txt");

        if(FileSystem.FileExists(wolPath))
        {
            activeMods.Add(await ProcessModFromMetadataAsync(wolPath));
        }

        if(FileSystem.FileExists(hotsPath))
        {
            activeMods.Add(await ProcessModFromMetadataAsync(hotsPath)); 
        }

        if(FileSystem.FileExists(lotvPath))
        {
            activeMods.Add(await ProcessModFromMetadataAsync(lotvPath));
        }

        if(FileSystem.FileExists(ncoPath))
        {
            activeMods.Add(await ProcessModFromMetadataAsync(ncoPath));
        }

        return activeMods;
    }

    /// <summary>
    /// Imports ZIP archives, extracts them through the ZIP service, and reloads campaign state.
    /// </summary>
    /// <param name="paths">The archive paths to import.</param>
    /// <returns>The refreshed campaign state and any uniquely identified imported mod.</returns>
    public static async Task<ImportCampaignZipsResult> ImportZipsAsync(IEnumerable<string> paths)
    {
        string sc2Dir = ApplicationSettings.QuerySettings(x => x.Sc2Dir);
        string customCampaignsPath = Path.Combine(sc2Dir, ZipService.CUSTOM_CAMPAIGNS_PATH);
        List<string> importedArchivePaths = [];

        foreach(string path in paths)
        {
            if(string.IsNullOrWhiteSpace(path) || !string.Equals(Path.GetExtension(path), ".zip", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string fileName = Path.GetFileName(path);
            if (!FileSystem.FileExists(path) || string.IsNullOrWhiteSpace(fileName))
            {
                await Service.Get<Logger>().LogAsync<IOError>([
                    $"ERROR: Could not find ZIP file {path}"
                ]);
                continue;
            }

            string destinationPath = Path.Combine(customCampaignsPath, fileName);
            try
            {
                StringComparison pathComparison = OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;
                if (string.Equals(Path.GetFullPath(path), Path.GetFullPath(destinationPath), pathComparison))
                {
                    importedArchivePaths.Add(path);
                    continue;
                }

                FileSystem.CopyFile(path, destinationPath, overwrite: true);
                importedArchivePaths.Add(path);
                await Service.Get<Logger>().LogAsync<IOInfo>([
                    $"Imported {fileName} to {destinationPath}"
                ]);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                await Service.Get<Logger>().LogAsync<IOError>([
                    $"ERROR: Could not import ZIP file {fileName}",
                    e.ToString()
                ]);
            }
        }

        await ZipService.UnzipZipsAsync(sc2Dir);

        List<ModList> modLists = await GetModListsAsync();
        List<Mod> activeMods = await GetActiveModsAsync();
        Mod? modAvailableToActivate = null;

        if(importedArchivePaths.Count == 1)
        {
            string importedDirectory = Path.Combine(
                customCampaignsPath,
                Path.GetFileNameWithoutExtension(importedArchivePaths[0]));
            modAvailableToActivate = modLists
                .SelectMany(modList => modList.Mods)
                .FirstOrDefault(mod => mod.Campaign != Campaign.None && IsWithinDirectory(importedDirectory, mod.Path));
        }

        return new(modLists, activeMods, modAvailableToActivate);
    }
    #endregion

    #region Private Methods
    private static bool IsWithinDirectory(string directory, string path)
    {
        string relativePath = Path.GetRelativePath(directory, path);
        return !Path.IsPathRooted(relativePath) &&
            relativePath != ".." &&
            !relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
    }

    private static async Task<bool> ClearDirectoryAsync(string path)
    {
        bool retVal = true;
        try
        {
            foreach (string file in FileSystem.GetFiles(path))
            {
                try
                {
                    FileSystem.DeleteFile(file);
                }
                catch(IOException e)
                {
                    await Service.Get<Logger>().LogAsync<IOError>([
                        $"ERROR: Could not delete campaign file {Path.GetFileNameWithoutExtension(file)}",
                        e.ToString()
                    ]);
                    retVal = false;
                }
            }
        }
        catch (IOException e)
        {
            await Service.Get<Logger>().LogAsync<IOError>([
                $"ERROR: Did not find directory, {path}, to clear",
                e.ToString()
            ]);
        }

        foreach (string subDir in FileSystem.GetDirectories(path))
        {
            string[] preserveSubDirs = [
                Path.Combine("Campaign", "swarm"),
                Path.Combine("Campaign", "void"),
                Path.Combine("Campaign", "nova"),
                Path.Combine("Campaign", "swarm", "evolution"),
                Path.Combine("Campaign", "voidprologue")
            ];

            if(!preserveSubDirs.Any(subDir.EndsWith))
            {
                try
                {
                    FileSystem.DeleteDirectory(subDir, true);
                }
                catch(IOException e)
                {
                    await Service.Get<Logger>().LogAsync<IOError>([
                        $"ERROR: Could not delete campaign directory {Path.GetFileName(subDir)}",
                        e.ToString()
                    ]);
                    retVal = false;
                }
            }
        }

        return retVal;
    }

    private static async Task CopyFilesAndFoldersAsync(string sourcePath, string targetPath)
    {
        foreach(string dir in FileSystem.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            FileSystem.CreateDirectory(dir.Replace(sourcePath, targetPath));
            await Service.Get<Logger>().LogAsync<IOInfo>([
                $"Created directory at {dir.Replace(sourcePath, targetPath)}"
            ]);
        }

        foreach(string oldPath in FileSystem.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            FileSystem.CopyFile(oldPath, oldPath.Replace(sourcePath, targetPath), true);
            await Service.Get<Logger>().LogAsync<IOInfo>([
                $"Copied file at {oldPath} to {oldPath.Replace(sourcePath, targetPath)}"
            ]);
        }
    }

    private static string GetCampaignString(Campaign campaign)
    {
        string campaignString = string.Empty;
        switch (campaign)
        {
            case Campaign.WoL:
                campaignString = "Wings of Liberty";
                break;
            case Campaign.HotS:
                campaignString = "Heart of the Swarm";
                break;
            case Campaign.LotV:
                campaignString = "Legacy of the Void";
                break;
            case Campaign.NCO:
                campaignString = "Nova Covert Ops";
                break;
        }

        return campaignString;
    }

    private static string GetModPathForCampaign(Campaign campaign)
    {
        string modDir = string.Empty;
        string sc2Dir = ApplicationSettings.QuerySettings(x => x.Sc2Dir);
        switch (campaign)
        {
            case Campaign.WoL:
                modDir = Path.Combine(sc2Dir, "Maps", "Campaign");
                break;
            case Campaign.HotS:
                modDir = Path.Combine(sc2Dir, "Maps", "Campaign", "swarm");
                break;
            case Campaign.LotV:
                modDir = Path.Combine(sc2Dir, "Maps", "Campaign", "void");
                break;
            case Campaign.NCO:
                modDir = Path.Combine(sc2Dir, "Maps", "Campaign", "nova");
                break;
        }

        return modDir;
    }

    private static async Task<Mod> ProcessModFromMetadataAsync(string path)
    {
        Mod retVal = null;
        if(FileSystem.FileExists(path))
        {
            retVal = new();
            await foreach(string line in FileSystem.ReadLinesAsync(path))
            {
                string[] lineParts = line.Split("=", 2);
                if(lineParts.Length == 2)
                {
                    switch(lineParts[0].ToLower())
                    {
                        case "title":
                            retVal.Title = lineParts[1];
                            break;
                        case "desc":
                            retVal.Description = lineParts[1];
                            break;
                        case "campaign":
                            retVal.SetCampaign(lineParts[1]);
                            break;
                        case "version":
                            retVal.Version = lineParts[1];
                            break;
                        case "author":
                            retVal.Author = lineParts[1];
                            break;
                    }
                } 
            }
            
            retVal.Path = Path.GetDirectoryName(path);
        }

        return retVal;
    }
    #endregion
}