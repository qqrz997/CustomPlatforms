using System.Collections.Specialized;

using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Models;
using CustomFloorPlugin.PlatformManagement;
using HMUI;
using Zenject;

namespace CustomFloorPlugin.UI;

[HotReload(RelativePathToLayout = "./PlatformLists.bsml")]
[ViewDefinition("CustomFloorPlugin.UI.PlatformLists.bsml")]
internal class PlatformListsViewController : BSMLAutomaticViewController
{
    [Inject] private readonly PluginConfig _config = null!;
    [Inject] private readonly AssetLoader _assetLoader = null!;
    [Inject] private readonly PlatformManager _platformManager = null!;
    [Inject] private readonly IPlatformSpawner _platformSpawner = null!;

    [UIComponent("singleplayer-platforms-list")] private readonly CustomListTableData _singleplayerList = null!;
    [UIComponent("multiplayer-platforms-list")] private readonly CustomListTableData _multiplayerList = null!;
    [UIComponent("a360-platforms-list")] private readonly CustomListTableData _circleList = null!;
    [UIComponent("menu-platforms-list")] private readonly CustomListTableData _menuList = null!;

    private CustomListTableData[] _listTables = null!;
    private int _tabIndex;
    
    [UIAction("#post-parse")]
    public void PostParse()
    {
        _listTables = [_singleplayerList, _multiplayerList, _circleList, _menuList];
        for (int i = 0; i < _platformManager.AllPlatforms.Count; i++)
            AddCellForPlatform(_platformManager.AllPlatforms[i], i);
        for (int i = 0; i < _listTables.Length; i++)
        {
            int index = GetPlatformIndexForTabIndex(i);
            _listTables[i].TableView.ReloadData();
            _listTables[i].TableView.ScrollToCellWithIdx(index, TableView.ScrollPositionType.Beginning, false);
            _listTables[i].TableView.SelectCellWithIdx(index);
        }
    }

    public bool Enabled
    {
        get => _config.Enabled;
        set
        {
            _config.Enabled = value;
            OnDidSelectPlatform(null!, GetPlatformIndexForTabIndex(_tabIndex));
        }
    }

    public bool CustomSongPlatforms
    {
        get => _config.CustomSongPlatforms;
        set => _config.CustomSongPlatforms = value;
    }
    
    [UIAction("select-tab")]
    public void OnDidSelectTab(SegmentedControl segmentedControl, int _)
    {
        _tabIndex = segmentedControl.selectedCellNumber;
        int index = GetPlatformIndexForTabIndex(_tabIndex);
        _listTables[segmentedControl.selectedCellNumber].TableView.ScrollToCellWithIdx(index, TableView.ScrollPositionType.Beginning, false);
        _listTables[segmentedControl.selectedCellNumber].TableView.SelectCellWithIdx(index, true);
    }

    [UIAction("select-platform")]
    public async void OnDidSelectPlatform(TableView _, int index)
    {
        await _platformSpawner.SpawnPlatform(_platformManager.AllPlatforms[index]);
        SetPlatformForTabIndex(_tabIndex, _platformManager.AllPlatforms[index]);
    }

    protected override async void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        if (firstActivation)
            _platformManager.AllPlatforms.CollectionChanged += OnCollectionDidChange;
        RefreshListViews();
        await _platformSpawner.SpawnPlatform(GetPlatformForTabIndex(_tabIndex));
    }

    protected override async void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        if (removedFromHierarchy)
            _platformManager.AllPlatforms.CollectionChanged -= OnCollectionDidChange;
        int index = GetPlatformIndexForTabIndex(_tabIndex);
        _listTables[_tabIndex].TableView.SelectCellWithIdx(index);
        await _platformSpawner.SpawnPlatform(_platformManager.MenuPlatform);
    }

    private void OnCollectionDidChange(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (CustomPlatform platform in e.NewItems)
                AddCellForPlatform(platform, e.NewStartingIndex);
            RefreshListViews();
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (CustomPlatform platform in e.OldItems)
                RemoveCellForPlatform(platform, e.OldStartingIndex);
            RefreshListViews();
        }
    }

    private void RefreshListViews()
    {
        foreach (CustomListTableData t in _listTables)
            t.TableView.ReloadDataKeepingPosition();
    }

    private void AddCellForPlatform(CustomPlatform platform, int index)
    {
        CustomListTableData.CustomCellInfo cell = new(platform.platName, platform.platAuthor, platform.icon ? platform.icon : _assetLoader.FallbackCover);
        foreach (CustomListTableData listTable in _listTables)
            listTable.Data.Insert(index, cell);
    }

    private void RemoveCellForPlatform(CustomPlatform platform, int index)
    {
        foreach (CustomListTableData listTable in _listTables)
        {
            listTable.Data.RemoveAt(index);
            if (platform != GetPlatformForTabIndex(_tabIndex))
                continue;
            listTable.TableView.SelectCellWithIdx(0);
            listTable.TableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
        }
    }

    private int GetPlatformIndexForTabIndex(int tabIndex) => 
        _platformManager.AllPlatforms.IndexOf(GetPlatformForTabIndex(tabIndex));

    private CustomPlatform GetPlatformForTabIndex(int tabIndex) => tabIndex switch
    {
        0 => _platformManager.SingleplayerPlatform,
        1 => _platformManager.MultiplayerPlatform,
        2 => _platformManager.A360Platform,
        3 => _platformManager.MenuPlatform,
        _ => _platformManager.DefaultPlatform
    };

    private void SetPlatformForTabIndex(int tabIndex, CustomPlatform platform)
    {
        switch (tabIndex)
        {
            case 0:
                _platformManager.SingleplayerPlatform = platform;
                _config.SingleplayerPlatformHash = platform.platHash;
                break;
            case 1:
                _platformManager.MultiplayerPlatform = platform;
                _config.MultiplayerPlatformHash = platform.platHash;
                break;
            case 2:
                _platformManager.A360Platform = platform;
                _config.A360PlatformHash = platform.platHash;
                break;
            case 3:
                _platformManager.MenuPlatform = platform;
                _config.MenuPlatformHash = platform.platHash;
                break;
        }
    }
}