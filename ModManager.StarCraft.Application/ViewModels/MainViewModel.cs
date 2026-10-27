using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using ModManager.StarCraft.Base;
using ModManager.StarCraft.Utilities.DependentProperties;
using ModManager.StarCraft.Services;
using ModManager.StarCraft.Services.Log;
using ModManager.StarCraft.Services.Log.IO;
using ModManager.StarCraft.Services.Settings;
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ModManager.StarCraft.Services.Updater;
using ModManager.StarCraft.Services.Log.LogTypes;
using MsBox.Avalonia.Base;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using ModManager.StarCraft.Base.Enum;
using ModManager.StarCraft.Base.Package;
using ModManager.StarCraft.Utilities;
using System.Linq;
using Avalonia.Input;

namespace ModManager.StarCraft.Application.ViewModels;

/// <summary>
/// Coordinates the campaign management state and commands displayed by the main window.
/// </summary>
public partial class MainViewModel: ViewModelBase
{
    #region Fields
    Startup _startup;

    private int _logLineCount = 0;
    #endregion

    #region RelayCommands
    /// <summary>
    /// Initializes the application and loads campaign data.
    /// </summary>
    public IRelayCommand WindowLoaded {get; private set;}

    /// <summary>
    /// Opens a file picker for the StarCraft II executable.
    /// </summary>
    public IRelayCommand SelectSc2 { get; private set; }

    /// <summary>
    /// Opens a file picker for campaign ZIP archives.
    /// </summary>
    public IRelayCommand SelectZipToImport { get; private set; }

    /// <summary>
    /// Updates the accepted effect while a campaign ZIP is dragged over the drop zone.
    /// </summary>
    public IRelayCommand<DragEventArgs> CampaignZipDragOver { get; private set; }

    /// <summary>
    /// Imports campaign ZIP archives dropped onto the drop zone.
    /// </summary>
    public IRelayCommand<DragEventArgs> CampaignZipDrop { get; private set; }

    #region Wings of Liberty
    /// <summary>
    /// Makes the selected Wings of Liberty mod active.
    /// </summary>
    public IRelayCommand WingsOfLibertySetActiveCampaign { get; private set; }

    /// <summary>
    /// Deletes the selected Wings of Liberty mod files.
    /// </summary>
    public IRelayCommand WingsOfLibertyDeleteModFiles {get; private set; }

    /// <summary>
    /// Restores the Wings of Liberty campaign to its unmodified state.
    /// </summary>
    public IRelayCommand WingsOfLibertyRestoreToUnmodified {get; private set; }
    #endregion

    #region Heart of the Swarm
    /// <summary>
    /// Makes the selected Heart of the Swarm mod active.
    /// </summary>
    public IRelayCommand HeartOfTheSwarmSetActiveCampaign { get; private set; }

    /// <summary>
    /// Deletes the selected Heart of the Swarm mod files.
    /// </summary>
    public IRelayCommand HeartOfTheSwarmDeleteModFiles {get; private set; }

    /// <summary>
    /// Restores the Heart of the Swarm campaign to its unmodified state.
    /// </summary>
    public IRelayCommand HeartOfTheSwarmRestoreToUnmodified {get; private set; }
    #endregion

    #region Legacy of the Void
    /// <summary>
    /// Makes the selected Legacy of the Void mod active.
    /// </summary>
    public IRelayCommand LegacyOfTheVoidSetActiveCampaign { get; private set; }

    /// <summary>
    /// Deletes the selected Legacy of the Void mod files.
    /// </summary>
    public IRelayCommand LegacyOfTheVoidDeleteModFiles {get; private set; }

    /// <summary>
    /// Restores the Legacy of the Void campaign to its unmodified state.
    /// </summary>
    public IRelayCommand LegacyOfTheVoidRestoreToUnmodified {get; private set; }
    #endregion
    
    #region Nova Covert Ops
    /// <summary>
    /// Makes the selected Nova Covert Ops mod active.
    /// </summary>
    public IRelayCommand NovaCovertOpsSetActiveCampaign { get; private set; }

    /// <summary>
    /// Deletes the selected Nova Covert Ops mod files.
    /// </summary>
    public IRelayCommand NovaCovertOpsDeleteModFiles {get; private set; }

    /// <summary>
    /// Restores the Nova Covert Ops campaign to its unmodified state.
    /// </summary>
    public IRelayCommand NovaCovertOpsRestoreToUnmodified {get; private set; }
    #endregion

    #endregion

    #region Properties
    /// <summary>
    /// Gets the application version displayed in the main window title.
    /// </summary>
    public string AppVersion 
    {
         get => _appVersion; 
         private set => this.Set(ref _appVersion, value);
    }
    private string _appVersion;

    /// <summary>
    /// Gets the application log displayed in the main window.
    /// </summary>
    public string Log
    {
        get => _log;
        private set => this.Set(ref _log, value);
    }
    public string _log = string.Empty;

    /// <summary>
    /// Gets or sets the download progress percent
    /// </summary>
    public int DownloadProgress
    {
        get => _downloadProgress;
        private set => this.Set(ref _downloadProgress, value);
    }
    private int _downloadProgress;

    /// <summary>
    /// Gets or sets whether to obsfucate the rest of the window and show the download progress bar
    /// </summary>
    public bool ShowDownloadProgress
    {
        get => _showDownloadProgress;
        private set => this.Set(ref _showDownloadProgress, value);
    }
    private bool _showDownloadProgress;
    #region Wings of Liberty
    /// <summary>
    /// Gets the currently active Wings of Liberty mod.
    /// </summary>
    public Mod? WingsOfLibertyActiveMod
    {
        get => _wingsOfLibertyActiveMod;
        private set => this.Set(ref _wingsOfLibertyActiveMod, value);
    }
    private Mod? _wingsOfLibertyActiveMod;

    /// <summary>
    /// Gets the title of the active Wings of Liberty mod.
    /// </summary>
    public string WingsOfLibertyActiveModTitle => this.Get(ref _wingsOfLibertyActiveModTitle, () => x =>
        x.WingsOfLibertyActiveMod != null ? x.WingsOfLibertyActiveMod.Title : "Default Campaign");
    private Dependent<string> _wingsOfLibertyActiveModTitle;

    /// <summary>
    /// Gets the author of the active Wings of Liberty mod.
    /// </summary>
    public string WingsOfLibertyActiveModAuthor => this.Get(ref _wingsOfLibertyActiveModAuthor, () => x =>
        x.WingsOfLibertyActiveMod != null ? x.WingsOfLibertyActiveMod.Author : "Blizzard");
    private Dependent<string> _wingsOfLibertyActiveModAuthor;

    /// <summary>
    /// Gets the version of the active Wings of Liberty mod.
    /// </summary>
    public string WingsOfLibertyActiveModVersion => this.Get(ref _wingsOfLibertyActiveModVersion, () => x =>
        x.WingsOfLibertyActiveMod != null ? x.WingsOfLibertyActiveMod.Version : "N/A");
    private Dependent<string> _wingsOfLibertyActiveModVersion;

    /// <summary>
    /// Gets a value indicating whether actions requiring a selected Wings of Liberty mod are enabled.
    /// </summary>
    public bool WingsOfLibertyButtonsEnabled => this.Get(ref _wingsOfLibertyButtonsEnabled, () => x => 
        x.WingsOfLibertyModListSelectedItem != null);
    private Dependent<bool> _wingsOfLibertyButtonsEnabled;

    /// <summary>
    /// Gets the installed Wings of Liberty mods.
    /// </summary>
    public ObservableCollection<Mod> WingsOfLibertyModList
    {
        get => _wingsOfLibertyModList;
        private set => this.Set(ref _wingsOfLibertyModList, value);
    }
    private ObservableCollection<Mod> _wingsOfLibertyModList = [];
    
    /// <summary>
    /// Gets or sets the selected Wings of Liberty mod.
    /// </summary>
    public Mod? WingsOfLibertyModListSelectedItem
    {
        get => _wingsOfLibertyModListSelectedItem;
        set => this.Set(ref _wingsOfLibertyModListSelectedItem, value);
    }
    private Mod? _wingsOfLibertyModListSelectedItem;

    /// <summary>
    /// Gets a value indicating whether the active Wings of Liberty mod warning is visible.
    /// </summary>
    public bool ShowWingsOfLibertyWarning
    {
        get => _showWingsOfLibertyWarning;
        private set => this.Set(ref _showWingsOfLibertyWarning, value);
    }
    private bool _showWingsOfLibertyWarning = false;
    #endregion

    #region Heart of the Swarm
    /// <summary>
    /// Gets the currently active Heart of the Swarm mod.
    /// </summary>
    public Mod? HeartOfTheSwarmActiveMod
    {
        get => _heartOfTheSwarmActiveMod;
        private set => this.Set(ref _heartOfTheSwarmActiveMod, value);
    }
    private Mod? _heartOfTheSwarmActiveMod;

    /// <summary>
    /// Gets the title of the active Heart of the Swarm mod.
    /// </summary>
    public string HeartOfTheSwarmActiveModTitle => this.Get(ref _heartOfTheSwarmActiveModTitle, () => x =>
        x.HeartOfTheSwarmActiveMod != null ? x.HeartOfTheSwarmActiveMod.Title : "Default Campaign");
    private Dependent<string> _heartOfTheSwarmActiveModTitle;

    /// <summary>
    /// Gets the author of the active Heart of the Swarm mod.
    /// </summary>
    public string HeartOfTheSwarmActiveModAuthor => this.Get(ref _heartOfTheSwarmActiveModAuthor, () => x =>
        x.HeartOfTheSwarmActiveMod != null ? x.HeartOfTheSwarmActiveMod.Author : "Blizzard");
    private Dependent<string> _heartOfTheSwarmActiveModAuthor;

    /// <summary>
    /// Gets the version of the active Heart of the Swarm mod.
    /// </summary>
    public string HeartOfTheSwarmActiveModVersion => this.Get(ref _heartOfTheSwarmActiveModVersion, () => x =>
        x.HeartOfTheSwarmActiveMod != null ? x.HeartOfTheSwarmActiveMod.Version : "N/A");
    private Dependent<string> _heartOfTheSwarmActiveModVersion;

    /// <summary>
    /// Gets a value indicating whether actions requiring a selected Heart of the Swarm mod are enabled.
    /// </summary>
    public bool HeartOfTheSwarmButtonsEnabled => this.Get(ref _heartOfTheSwarmButtonsEnabled, () => x => 
        x.HeartOfTheSwarmModListSelectedItem != null);
    private Dependent<bool> _heartOfTheSwarmButtonsEnabled;

    /// <summary>
    /// Gets the installed Heart of the Swarm mods.
    /// </summary>
    public ObservableCollection<Mod> HeartOfTheSwarmModList
    {
        get => _heartOfTheSwarmModList;
        private set => this.Set(ref _heartOfTheSwarmModList, value);
    }
    private ObservableCollection<Mod> _heartOfTheSwarmModList = [];
    
    /// <summary>
    /// Gets or sets the selected Heart of the Swarm mod.
    /// </summary>
    public Mod? HeartOfTheSwarmModListSelectedItem
    {
        get => _heartOfTheSwarmModListSelectedItem;
        set => this.Set(ref _heartOfTheSwarmModListSelectedItem, value);
    }
    private Mod? _heartOfTheSwarmModListSelectedItem;

    /// <summary>
    /// Gets a value indicating whether the active Heart of the Swarm mod warning is visible.
    /// </summary>
    public bool ShowHeartOfTheSwarmWarning
    {
        get => _showHeartOfTheSwarmWarning;
        private set => this.Set(ref _showHeartOfTheSwarmWarning, value);
    }
    private bool _showHeartOfTheSwarmWarning = false;
    #endregion

    #region Legacy of the Void
    /// <summary>
    /// Gets the currently active Legacy of the Void mod.
    /// </summary>
    public Mod? LegacyOfTheVoidActiveMod
    {
        get => _legacyOfTheVoidActiveMod;
        private set => this.Set(ref _legacyOfTheVoidActiveMod, value);
    }
    private Mod? _legacyOfTheVoidActiveMod;

    /// <summary>
    /// Gets the title of the active Legacy of the Void mod.
    /// </summary>
    public string LegacyOfTheVoidActiveModTitle => this.Get(ref _legacyOfTheVoidActiveModTitle, () => x =>
        x.LegacyOfTheVoidActiveMod != null ? x.LegacyOfTheVoidActiveMod.Title : "Default Campaign");
    private Dependent<string> _legacyOfTheVoidActiveModTitle;

    /// <summary>
    /// Gets the author of the active Legacy of the Void mod.
    /// </summary>
    public string LegacyOfTheVoidActiveModAuthor => this.Get(ref _legacyOfTheVoidActiveModAuthor, () => x =>
        x.LegacyOfTheVoidActiveMod != null ? x.LegacyOfTheVoidActiveMod.Author : "Blizzard");
    private Dependent<string> _legacyOfTheVoidActiveModAuthor;

    /// <summary>
    /// Gets the version of the active Legacy of the Void mod.
    /// </summary>
    public string LegacyOfTheVoidActiveModVersion => this.Get(ref _legacyOfTheVoidActiveModVersion, () => x =>
        x.LegacyOfTheVoidActiveMod != null ? x.LegacyOfTheVoidActiveMod.Version : "N/A");
    private Dependent<string> _legacyOfTheVoidActiveModVersion;
    
    /// <summary>
    /// Gets a value indicating whether actions requiring a selected Legacy of the Void mod are enabled.
    /// </summary>
    public bool LegacyOfTheVoidButtonsEnabled => this.Get(ref _legacyOfTheVoidButtonsEnabled, () => x => 
        x.LegacyOfTheVoidModListSelectedItem != null);
    private Dependent<bool> _legacyOfTheVoidButtonsEnabled;

    /// <summary>
    /// Gets the installed Legacy of the Void mods.
    /// </summary>
    public ObservableCollection<Mod> LegacyOfTheVoidModList
    {
        get => _legacyOfTheVoidModList;
        private set => this.Set(ref _legacyOfTheVoidModList, value);
    }
    private ObservableCollection<Mod> _legacyOfTheVoidModList = [];
    
    /// <summary>
    /// Gets or sets the selected Legacy of the Void mod.
    /// </summary>
    public Mod? LegacyOfTheVoidModListSelectedItem
    {
        get => _legacyOfTheVoidModListSelectedItem;
        set => this.Set(ref _legacyOfTheVoidModListSelectedItem, value);
    }
    private Mod? _legacyOfTheVoidModListSelectedItem;

    /// <summary>
    /// Gets a value indicating whether the active Legacy of the Void mod warning is visible.
    /// </summary>
    public bool ShowLegacyOfTheVoidWarning
    {
        get => _showLegacyOfTheVoidWarning;
        private set => this.Set(ref _showLegacyOfTheVoidWarning, value);
    }
    private bool _showLegacyOfTheVoidWarning = false;
    #endregion

    #region Nova Covert Ops
    /// <summary>
    /// Gets the currently active Nova Covert Ops mod.
    /// </summary>
    public Mod? NovaCovertOpsActiveMod
    {
        get => _novaCovertOpsActiveMod;
        private set => this.Set(ref _novaCovertOpsActiveMod, value);
    }
    private Mod? _novaCovertOpsActiveMod;

    /// <summary>
    /// Gets the title of the active Nova Covert Ops mod.
    /// </summary>
    public string NovaCovertOpsActiveModTitle => this.Get(ref _novaCovertOpsActiveModTitle, () => x =>
        x.NovaCovertOpsActiveMod != null ? x.NovaCovertOpsActiveMod.Title : "Default Campaign");
    private Dependent<string> _novaCovertOpsActiveModTitle;

    /// <summary>
    /// Gets the author of the active Nova Covert Ops mod.
    /// </summary>
    public string NovaCovertOpsActiveModAuthor => this.Get(ref _novaCovertOpsActiveModAuthor, () => x =>
        x.NovaCovertOpsActiveMod != null ? x.NovaCovertOpsActiveMod.Author : "Blizzard");
    private Dependent<string> _novaCovertOpsActiveModAuthor;

    /// <summary>
    /// Gets the version of the active Nova Covert Ops mod.
    /// </summary>
    public string NovaCovertOpsActiveModVersion => this.Get(ref _novaCovertOpsActiveModVersion, () => x =>
        x.NovaCovertOpsActiveMod != null ? x.NovaCovertOpsActiveMod.Version : "N/A");
    private Dependent<string> _novaCovertOpsActiveModVersion;
    
    /// <summary>
    /// Gets a value indicating whether actions requiring a selected Nova Covert Ops mod are enabled.
    /// </summary>
    public bool NovaCovertOpsButtonsEnabled => this.Get(ref _novaCovertOpsButtonsEnabled, () => x => 
        x.NovaCovertOpsActiveMod != null);
    private Dependent<bool> _novaCovertOpsButtonsEnabled;

    /// <summary>
    /// Gets the installed Nova Covert Ops mods.
    /// </summary>
    public ObservableCollection<Mod> NovaCovertOpsModList
    {
        get => _novaCovertOpsModList;
        private set => this.Set(ref _novaCovertOpsModList, value);
    }
    private ObservableCollection<Mod> _novaCovertOpsModList = [];
    
    /// <summary>
    /// Gets or sets the selected Nova Covert Ops mod.
    /// </summary>
    public Mod? NovaCovertOpsModListSelectedItem
    {
        get => _novaCovertOpsModListSelectedItem;
        set => this.Set(ref _novaCovertOpsModListSelectedItem, value);
    }
    private Mod? _novaCovertOpsModListSelectedItem;

    /// <summary>
    /// Gets a value indicating whether the active Nova Covert Ops mod warning is visible.
    /// </summary>
    public bool ShowNovaCovertOpsWarning
    {
        get => _showNovaCovertOpsWarning;
        private set => this.Set(ref _showNovaCovertOpsWarning, value);
    }
    private bool _showNovaCovertOpsWarning = false;
    #endregion

    #endregion

    #region Constructor
    public MainViewModel() 
    {
        WindowLoaded = new RelayCommand(OnWindowLoadedExecute);
        
        // Prepare relay commands
        SelectSc2 = new RelayCommand(OnSelectSc2Execute);
        SelectZipToImport = new RelayCommand(OnSelectZipToImportExecute);
        CampaignZipDragOver = new RelayCommand<DragEventArgs>(OnCampaignZipDragOverExecute);
        CampaignZipDrop = new RelayCommand<DragEventArgs>(OnCampaignZipDropExecute);

        WingsOfLibertySetActiveCampaign = new RelayCommand(OnWingsOfLibertySetActiveCampaignExecute);
        WingsOfLibertyDeleteModFiles = new RelayCommand(OnWingsOfLibertyDeleteModFilesExecute);
        WingsOfLibertyRestoreToUnmodified = new RelayCommand(OnWingsOfLibertyRestoreToUnmodifiedExecute);
        HeartOfTheSwarmSetActiveCampaign = new RelayCommand(OnHeartOfTheSwarmSetActiveCampaignExecute);
        HeartOfTheSwarmDeleteModFiles = new RelayCommand(OnHeartOfTheSwarmDeleteModFilesExecute);
        HeartOfTheSwarmRestoreToUnmodified = new RelayCommand(OnHeartOfTheSwarmRestoreToUnmodifiedExecute);
        LegacyOfTheVoidSetActiveCampaign = new RelayCommand(OnLegacyOfTheVoidSetActiveCampaignExecute);
        LegacyOfTheVoidDeleteModFiles = new RelayCommand(OnLegacyOfTheVoidDeleteModFilesExecute);
        LegacyOfTheVoidRestoreToUnmodified = new RelayCommand(OnLegacyOfTheVoidRestoreToUnmodifiedExecute);
        NovaCovertOpsSetActiveCampaign = new RelayCommand(OnNovaCovertOpsSetActiveCampaignExecute);
        NovaCovertOpsDeleteModFiles = new RelayCommand(OnNovaCovertOpsDeleteModFilesExecute);
        NovaCovertOpsRestoreToUnmodified = new RelayCommand(OnNovaCovertOpsRestoreToUnmodifiedExecute);
    }
    #endregion

    #region Event Handler Methods
    /// <summary>
    /// Appends a service log notification to the view model log and trims older entries when necessary.
    /// </summary>
    /// <param name="sender">The source of the notification.</param>
    /// <param name="e">The log entry to append.</param>
    private void HandleLogNotification(object sender, LogEntry e)
    {
        Log += $"[{e.Severity}]: {e.LogMessage} {e.AdditionalInfo}\n";

        _logLineCount += 1;

        // Trim the UI log string to ensure performant logging
        if(_logLineCount >= 66) // generic number choice
        {
            int startIndex = 0;
            for(int i = 0 ; i < 12 ; i ++)
            {
                startIndex = Log.IndexOf('\n', startIndex) + 1;
                if(startIndex == 0) break; // EOS
            }

            Log = Log[startIndex..];
        }
    }
    #endregion

    #region RelayCommand Handlers
    /// <summary>
    /// Creates the startup workflow, subscribes to its progress events, and begins initialization.
    /// </summary>
    private async void OnWindowLoadedExecute()
    {
        List<ModList> modLists = [];
        List<Mod> activeMods = [];
        _startup = Startup.Create();
        Service.Get<Logger>().LogNotification += HandleLogNotification;
        AppVersion = ApplicationSettings.QuerySettings(x => x.ApplicationVersion);

        _startup.CheckForUpdatesCompleted += OnCheckForUpdatesCompletedAsync;
        _startup.VerifyStarCraftLocationCompleted += OnVerifyStarCraftLocationCompletedAsync;
        _startup.GetModListsCompleted += OnGetModListsCompletedAsync;
        _startup.GetActiveModsCompleted += OnGetActiveModsCompletedAsync;
        _startup.StartupCompleted += OnStartupCompletedAsync;

        if(Service.Get<VelopackUpdateService>().RecentlyUpdated)
        {
           await MessageBoxManager.GetMessageBoxStandard(
                "StarCraft II Custom Campaign Manager Extended",
                $"Sucessfully updated StarCraft II Custom Campaign Manager Extended to {AppVersion}",
                ButtonEnum.Ok).ShowAsync();
        }

        await _startup.BeginStartupAsync();
    }

    /// <summary>
    /// Selects a StarCraft II installation and restarts startup processing when the location changes.
    /// </summary>
    private async void OnSelectSc2Execute()
    {
        if(await SelectStarCraftWithFilePickerAsync())
        {
            // If we selected a new starcraft 2 location, we need to re-run startup folder operations for this new path
            List<ModList> modLists = [];
            List<Mod> activeMods = [];

            _startup = Startup.Create();
            _startup.CheckForUpdatesCompleted += OnCheckForUpdatesCompletedAsync;
            _startup.VerifyStarCraftLocationCompleted += OnVerifyStarCraftLocationCompletedAsync;
            _startup.GetModListsCompleted += OnGetModListsCompletedAsync;
            _startup.GetActiveModsCompleted += OnGetActiveModsCompletedAsync;
            _startup.StartupCompleted += OnStartupCompletedAsync;

            await _startup.BeginStartupAsync();
        }
    }

    /// <summary>
    /// Opens the campaign ZIP picker and imports the selected archives.
    /// </summary>
    private async void OnSelectZipToImportExecute()
    {
        if(App.TopLevel == null)
        {
            await Service.Get<Logger>().LogAsync<ApplicationError>([
                "Could not open the ZIP picker because the application window is unavailable."
            ]);
            return;
        }

        IReadOnlyList<IStorageFile> files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = "Select Campaign ZIP Files",
            AllowMultiple = true,
            FileTypeFilter =
            [
                new FilePickerFileType("ZIP archives")
                {
                    Patterns = ["*.zip"]
                }
            ]
        });

        if(files.Count > 0)
        {
            await ImportCampaignZipsAsync(files.Select(file => file.Path.LocalPath));
        }
    }

    /// <summary>
    /// Accepts a copy drag effect only when the dragged data contains ZIP files.
    /// </summary>
    /// <param name="e">The drag event arguments supplied by Avalonia.</param>
    private void OnCampaignZipDragOverExecute(DragEventArgs e)
    {
        e.DragEffects = GetDroppedZipPaths(e).Length > 0
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    /// <summary>
    /// Imports ZIP files dropped onto the campaign import area.
    /// </summary>
    /// <param name="e">The drop event arguments supplied by Avalonia.</param>
    private async void OnCampaignZipDropExecute(DragEventArgs e)
    {
        string[] zipPaths = GetDroppedZipPaths(e);
        e.DragEffects = zipPaths.Length > 0
            ? DragDropEffects.Copy
            : DragDropEffects.None;

        if(zipPaths.Length > 0)
        {
            await ImportCampaignZipsAsync(zipPaths);
        }
    }

    /// <summary>
    /// Activates the selected Wings of Liberty mod and updates its warning state.
    /// </summary>
    private async void OnWingsOfLibertySetActiveCampaignExecute() =>
        ShowWingsOfLibertyWarning = !(
            await CustomCampaignManager.SetActiveModAsync(Campaign.WoL, WingsOfLibertyModListSelectedItem)
        );

    /// <summary>
    /// Deletes the selected Wings of Liberty mod and refreshes campaign state.
    /// </summary>
    private async void OnWingsOfLibertyDeleteModFilesExecute()
    {
        if(await CustomCampaignManager.DeleteModFilesAsync(WingsOfLibertyModListSelectedItem))
        {
            PopulateModLists(await CustomCampaignManager.GetModListsAsync());
            PopulateActiveMods(await CustomCampaignManager.GetActiveModsAsync());
        }
    }
    
    /// <summary>
    /// Restores the Wings of Liberty campaign files to their unmodified state.
    /// </summary>
    private async void OnWingsOfLibertyRestoreToUnmodifiedExecute()
    {
        if(await CustomCampaignManager.RestoreToUnmodifiedAsync(Campaign.WoL))
        {
            WingsOfLibertyActiveMod = null;
        }
    }

    /// <summary>
    /// Activates the selected Heart of the Swarm mod and updates its warning state.
    /// </summary>
    private async void OnHeartOfTheSwarmSetActiveCampaignExecute() =>
        ShowHeartOfTheSwarmWarning = !(
            await CustomCampaignManager.SetActiveModAsync(Campaign.HotS, HeartOfTheSwarmModListSelectedItem)
        );

    /// <summary>
    /// Deletes the selected Heart of the Swarm mod and refreshes campaign state.
    /// </summary>
    private async void OnHeartOfTheSwarmDeleteModFilesExecute()
    {
        if(await CustomCampaignManager.DeleteModFilesAsync(HeartOfTheSwarmModListSelectedItem))
        {
            PopulateModLists(await CustomCampaignManager.GetModListsAsync());
            PopulateActiveMods(await CustomCampaignManager.GetActiveModsAsync());
        }
    }

    /// <summary>
    /// Restores the Heart of the Swarm campaign files to their unmodified state.
    /// </summary>
    private async void OnHeartOfTheSwarmRestoreToUnmodifiedExecute()
    {
        if(await CustomCampaignManager.RestoreToUnmodifiedAsync(Campaign.HotS))
        {
            HeartOfTheSwarmActiveMod = null;
        }
    }

    /// <summary>
    /// Activates the selected Legacy of the Void mod and updates its warning state.
    /// </summary>
    private async void OnLegacyOfTheVoidSetActiveCampaignExecute() =>
        ShowLegacyOfTheVoidWarning = !(
            await CustomCampaignManager.SetActiveModAsync(Campaign.LotV, LegacyOfTheVoidModListSelectedItem)
        );

    /// <summary>
    /// Deletes the selected Legacy of the Void mod and refreshes campaign state.
    /// </summary>
    private async void OnLegacyOfTheVoidDeleteModFilesExecute()
    {
        if(await CustomCampaignManager.DeleteModFilesAsync(LegacyOfTheVoidModListSelectedItem))
        {
            PopulateModLists(await CustomCampaignManager.GetModListsAsync());
            PopulateActiveMods(await CustomCampaignManager.GetActiveModsAsync());
        }
    }

    /// <summary>
    /// Restores the Legacy of the Void campaign files to their unmodified state.
    /// </summary>
    private async void OnLegacyOfTheVoidRestoreToUnmodifiedExecute()
    {
        if(await CustomCampaignManager.RestoreToUnmodifiedAsync(Campaign.LotV))
        {
            LegacyOfTheVoidActiveMod = null;
        }
    }

    /// <summary>
    /// Activates the selected Nova Covert Ops mod and updates its warning state.
    /// </summary>
    private async void OnNovaCovertOpsSetActiveCampaignExecute() =>
        ShowNovaCovertOpsWarning = !(
            await CustomCampaignManager.SetActiveModAsync(Campaign.NCO, NovaCovertOpsModListSelectedItem)
        );

    /// <summary>
    /// Deletes the selected Nova Covert Ops mod and refreshes campaign state.
    /// </summary>
    private async void OnNovaCovertOpsDeleteModFilesExecute()
    {
        if(await CustomCampaignManager.DeleteModFilesAsync(NovaCovertOpsModListSelectedItem))
        {
            PopulateModLists(await CustomCampaignManager.GetModListsAsync());
            PopulateActiveMods(await CustomCampaignManager.GetActiveModsAsync());
        }
    }
    
    /// <summary>
    /// Restores the Nova Covert Ops campaign files to their unmodified state.
    /// </summary>
    private async void OnNovaCovertOpsRestoreToUnmodifiedExecute()
    {
        if(await CustomCampaignManager.RestoreToUnmodifiedAsync(Campaign.NCO))
        {
            NovaCovertOpsActiveMod = null;
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Extracts local ZIP paths from drag data and filters out other file types.
    /// </summary>
    /// <param name="e">The drag event containing the transferred files.</param>
    /// <returns>The local paths of all dragged ZIP files.</returns>
    private static string[] GetDroppedZipPaths(DragEventArgs e) =>
        e.DataTransfer.TryGetFiles()?
            .Select(file => file.Path.LocalPath)
            .Where(path => string.Equals(Path.GetExtension(path), ".zip", StringComparison.OrdinalIgnoreCase))
            .ToArray() ?? [];

    /// <summary>
    /// Imports campaign archives, refreshes the displayed campaigns, and offers to activate a new mod.
    /// </summary>
    /// <param name="paths">The local paths of the campaign ZIP archives to import.</param>
    private async Task ImportCampaignZipsAsync(IEnumerable<string> paths)
    {
        ImportCampaignZipsResult result = await CustomCampaignManager.ImportZipsAsync(paths);

        PopulateModLists(result.ModLists);
        PopulateActiveMods(result.ActiveMods);

        if(result.ModAvailableToActivate is Mod importedMod)
        {
            IMsBox<ButtonResult> messageBox = MessageBoxManager.GetMessageBoxStandard(
                "StarCraft II Custom Campaign Manager Extended",
                $"Import of {importedMod.Title} successful. Would you like to make it the active campaign?",
                ButtonEnum.YesNo);

            if(await messageBox.ShowAsync() == ButtonResult.Yes &&
                await CustomCampaignManager.SetActiveModAsync(importedMod.Campaign, importedMod))
            {
                PopulateActiveMods(await CustomCampaignManager.GetActiveModsAsync());
            }
        }
    }

    /// <summary>
    /// Handles StarCraft II location verification and continues startup after resolving a missing location.
    /// </summary>
    /// <param name="_">The event source.</param>
    /// <param name="e">The result of StarCraft II location verification.</param>
    private async void OnVerifyStarCraftLocationCompletedAsync(object _, VerifyStarCraftLocationResult e)
    {
        if(e == VerifyStarCraftLocationResult.DoesNotExist)
        {
            IMsBox<ButtonResult> messageBox = MessageBoxManager.GetMessageBoxStandard(
                "StarCraft II Custom Campaign Manager Extended",
                "It looks like StarCraft II isn't in the default spot!\nPlease use the file browser and select Starcraft II.exe",
                ButtonEnum.Ok
            );
            await messageBox.ShowAsync();

            if(!await SelectStarCraftWithFilePickerAsync())
            {
                App.ShutdownApplication();
            }
        }

        await _startup.ContinueStartupAsync();
    }

    /// <summary>
    /// Handles update checks and continues startup after processing an available update.
    /// </summary>
    /// <param name="_">The event source.</param>
    /// <param name="e">The update check result.</param>
    private async void OnCheckForUpdatesCompletedAsync(object _, CheckForUpdatesResult e)
    {
        if(e.UpdateAvailable && e.Update != null)
        {
            IMsBox<ButtonResult> messageBox = MessageBoxManager.GetMessageBoxStandard(
                "StarCraft II Custom Campaign Manager Extended",
                "A new version of SC2CCME is available to download!\nWould you like to install it now?",
                ButtonEnum.YesNo
            );
            ButtonResult shouldUpdate = await messageBox.ShowAsync();

            if(shouldUpdate == ButtonResult.Yes)
            {
                try
                {
                    ShowDownloadProgress = true;
                    await Service.Get<VelopackUpdateService>().ApplyUpdateAsync(e.Update, OnUpdateProgress);
                }
                catch(Exception ex)
                {
                    await MessageBoxManager.GetMessageBoxStandard(
                        "StarCraft II Custom Campaign Manager Extended",
                        $"An error occurred while trying to update!\n{ex}\n\n" +
                        $"An update failure log file should exist in {ApplicationDataHelper.CCMDataPath}.",
                        ButtonEnum.Ok
                    ).ShowAsync();

                    App.ShutdownApplication();
                }
                finally
                {
                    ShowDownloadProgress = false;
                }
            }
        }

        await _startup.ContinueStartupAsync();
    }
    
    /// <summary>
    /// Applies loaded mod lists to the campaign-specific observable collections.
    /// </summary>
    /// <param name="_">The event source.</param>
    /// <param name="e">The loaded campaign mod lists.</param>
    private async void OnGetModListsCompletedAsync(object _, List<ModList> e)
    {
        if(e != null)
        {
            PopulateModLists(e);
        }

        await _startup.ContinueStartupAsync();
    }

    /// <summary>
    /// Applies loaded active mods to the campaign properties and selected list items.
    /// </summary>
    /// <param name="_">The event source.</param>
    /// <param name="e">The loaded active mods.</param>
    private async void OnGetActiveModsCompletedAsync(object _, List<Mod> e)
    {
        if(e != null)
        {
            PopulateActiveMods(e);
        }

        await _startup.ContinueStartupAsync();
    }

    /// <summary>
    /// Unsubscribes from startup events and records that application startup has completed.
    /// </summary>
    /// <param name="_">The event source.</param>
    /// <param name="e">The startup completion event data.</param>
    private async void OnStartupCompletedAsync(object _, EventArgs e)
    {
        _startup.CheckForUpdatesCompleted -= OnCheckForUpdatesCompletedAsync;
        _startup.VerifyStarCraftLocationCompleted -= OnVerifyStarCraftLocationCompletedAsync;
        _startup.GetModListsCompleted -= OnGetModListsCompletedAsync;
        _startup.GetActiveModsCompleted -= OnGetActiveModsCompletedAsync;
        _startup.StartupCompleted -= OnStartupCompletedAsync;

        _startup = null;  

        await Service.Get<Logger>().LogAsync<ApplicationInfo>([
            "SC2CCME Loaded"
        ]);
    }

    /// <summary>
    /// Opens a file picker for the StarCraft II executable and stores the selected location.
    /// </summary>
    /// <returns><see langword="true"/> when one executable was selected; otherwise, <see langword="false"/>.</returns>
    private async Task<bool> SelectStarCraftWithFilePickerAsync()
    {
        bool retVal = false;
        if(App.TopLevel != null)
        {
            IReadOnlyList<IStorageFile> files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new()
            {
                Title = "Select StarCraft II.exe",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Executable")
                    {
                        Patterns = ["*.exe"]
                    }
                ]
            });

            if(files.Count == 1)
            {
                ApplicationSettings.QuerySettings(x => x.Sc2Path = files[0].Path.ToString());
                ApplicationSettings.QuerySettings(x => x.Sc2Dir = Path.GetDirectoryName(files[0].Path.ToString()));
                retVal = true;
            }
        }
        else
        {
            await Service.Get<Logger>().LogAsync<ApplicationError>([
                "Could not find application top level visual element... There is a problem with the application."
            ]);
        }

        return retVal;
    }

    /// <summary>
    /// Splits loaded mod lists into the four campaign-specific collections.
    /// </summary>
    /// <param name="modLists">The loaded campaign mod lists.</param>
    private void PopulateModLists(List<ModList> modLists)
    {
        WingsOfLibertyModList = [ .. modLists.FirstOrDefault(x => x.ModListCampaign == Campaign.WoL)?.Mods ];
        HeartOfTheSwarmModList = [ .. modLists.FirstOrDefault(x => x.ModListCampaign == Campaign.HotS)?.Mods ];
        LegacyOfTheVoidModList = [ .. modLists.FirstOrDefault(x => x.ModListCampaign == Campaign.LotV)?.Mods ];
        NovaCovertOpsModList = [ .. modLists.FirstOrDefault(x => x.ModListCampaign == Campaign.NCO)?.Mods ];

    }

    /// <summary>
    /// Assigns active mods and synchronizes each campaign's selected list item.
    /// </summary>
    /// <param name="activeMods">The currently active campaign mods.</param>
    private void PopulateActiveMods(List<Mod> activeMods)
    {
        WingsOfLibertyActiveMod = activeMods.FirstOrDefault(x => x.Campaign == Campaign.WoL);
        HeartOfTheSwarmActiveMod = activeMods.FirstOrDefault(x => x.Campaign == Campaign.HotS);
        LegacyOfTheVoidActiveMod = activeMods.FirstOrDefault(x => x.Campaign == Campaign.LotV);
        NovaCovertOpsActiveMod = activeMods.FirstOrDefault(x => x.Campaign == Campaign.NCO);

        WingsOfLibertyModListSelectedItem = WingsOfLibertyModList?.FirstOrDefault(x => x.Title == WingsOfLibertyActiveMod?.Title);
        HeartOfTheSwarmModListSelectedItem = HeartOfTheSwarmModList?.FirstOrDefault(x => x.Title == HeartOfTheSwarmActiveMod?.Title);
        LegacyOfTheVoidModListSelectedItem = LegacyOfTheVoidModList?.FirstOrDefault(x => x.Title == LegacyOfTheVoidActiveMod?.Title);
        NovaCovertOpsModListSelectedItem = NovaCovertOpsModList?.FirstOrDefault(x => x.Title == NovaCovertOpsActiveMod?.Title);
    }

    private void OnUpdateProgress(int progress) => DownloadProgress = progress; 
    #endregion
}
