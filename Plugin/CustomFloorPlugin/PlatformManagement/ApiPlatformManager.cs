using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Helpers;
using Newtonsoft.Json;
using SiraUtil.Logging;
using SiraUtil.Web;
using SongCore;
using Zenject;

namespace CustomFloorPlugin.PlatformManagement;

internal class ApiPlatformManager : IInitializable, IDisposable
{
    private readonly SiraLog _siraLog;
    private readonly PluginConfig _pluginConfig;
    private readonly IHttpService _httpService;
    private readonly LevelCollectionViewController _levelCollectionViewController;
    private readonly PlatformManager _platformManager;

    public ApiPlatformManager(
        SiraLog siraLog,
        PluginConfig pluginConfig,
        IHttpService httpService,
        LevelCollectionViewController levelCollectionViewController,
        PlatformManager platformManager)
    {
        _siraLog = siraLog;
        _pluginConfig = pluginConfig;
        _httpService = httpService;
        _levelCollectionViewController = levelCollectionViewController;
        _platformManager = platformManager;
    }

    private readonly CancellationTokenSource _downloadPlatformTokenSource = new();
    
    public void Initialize()
    {
        _levelCollectionViewController.didSelectLevelEvent += OnLevelSelected;
    }

    public void Dispose()
    {
        _levelCollectionViewController.didSelectLevelEvent -= OnLevelSelected;
        _downloadPlatformTokenSource.Cancel();
        _downloadPlatformTokenSource.Dispose();
    }
    
    private async void OnLevelSelected(LevelCollectionViewController levelCollectionViewController, BeatmapLevel beatmapLevel)
    {
        _platformManager.APIRequestedPlatform = null;
        if (!_pluginConfig.CustomSongPlatforms || beatmapLevel.hasPrecalculatedData)
        {
            return;
        }

        var songData = Collections.GetCustomLevelSongData(beatmapLevel.levelID);
        if (songData == null)
        {
            return;
        }

        var token = _downloadPlatformTokenSource.Token;
        var (hash, name) = (songData._customEnvironmentHash, songData._customEnvironmentName);
        
        if (!string.IsNullOrWhiteSpace(hash))
        {
            _platformManager.APIRequestedPlatform = await DownloadPlatformByHash(hash, token);
        }
        else if (!string.IsNullOrWhiteSpace(name))
        {
            _platformManager.APIRequestedPlatform = await DownloadPlatformByName(name, token);
        }
    }

    private async Task<CustomPlatform?> DownloadPlatformByHash(string hash, CancellationToken token) =>
        _platformManager.AllPlatforms.TryGetFirst(x => x.platHash == hash, out var platform) ? platform 
        : await DownloadPlatform($"https://modelsaber.com/api/v2/get.php?type=platform&filter=hash:{hash}", token);

    private async Task<CustomPlatform?> DownloadPlatformByName(string name, CancellationToken token) =>
        _platformManager.AllPlatforms.TryGetFirst(x => x.platName == name, out var platform) ? platform 
        : await DownloadPlatform($"https://modelsaber.com/api/v2/get.php?type=platform&filter=name:{name}", token);
    
    private async Task<CustomPlatform?> DownloadPlatform(string url, CancellationToken token)
    {
        var httpResponse = await _httpService.GetAsync(url, null, token);
        if (!httpResponse.Successful)
        {
            return null;
        }

        string json = await httpResponse.ReadAsStringAsync();

        var response = JsonConvert.DeserializeObject<Dictionary<string, PlatformDownloadData>>(json)?.FirstOrDefault();
        if (!response.HasValue)
        {
            return null;
        }

        var (platformName, platformUrl) = response.Value.Value;
        
        if (platformName is null || platformUrl is null)
        {
            return null;
        }

        var platformHttpResponse = await _httpService.GetAsync(platformUrl, null, token);
        if (!platformHttpResponse.Successful)
        {
            return null;
        }

        var platformBytes = await platformHttpResponse.ReadAsByteArrayAsync();
        string path = Path.Combine(_platformManager.DirectoryPath, $"{platformName}.plat");
        int count = 0;
        while (File.Exists(path))
        {
            path = Path.Combine(_platformManager.DirectoryPath, $"{platformName} ({count++}).plat");
        }
        
        await File.WriteAllBytesAsync(path, platformBytes, token);
        
        var newPlatform = await _platformManager.CreatePlatformAsync(path);
        if (token.IsCancellationRequested || newPlatform == null)
        {
            return null;
        }

        _siraLog.Debug($"New platform downloaded from custom song: {newPlatform.platName}");
        _platformManager.AllPlatforms.AddSorted(
            PlatformManager.BuildInPlatformsCount,
            _platformManager.AllPlatforms.Count - PlatformManager.BuildInPlatformsCount,
            newPlatform);
        
        return newPlatform;
    }
}

[Serializable]
file record PlatformDownloadData(string? Name, string? Download);