using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomFloorPlugin.Helpers;
using IPA.Loader;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SiraUtil.Logging;
using SiraUtil.Web;
using Zenject;

namespace CustomFloorPlugin.PlatformManagement;

/// <summary>
/// Responsible for downloading platforms from custom levels that require a custom platform, and disabling custom
/// platforms on levels that require Cinema.
/// </summary>
internal class ConnectionManager : IInitializable, IDisposable
{
    private readonly SiraLog _siraLog;
    private readonly IHttpService _httpService;
    private readonly PlatformManager _platformManager;

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly bool _isSongCoreInstalled;
    private readonly bool _isCinemaInstalled;

    public ConnectionManager(SiraLog siraLog, IHttpService httpService, PlatformManager platformManager)
    {
        _siraLog = siraLog;
        _httpService = httpService;
        _platformManager = platformManager;
        _isSongCoreInstalled = PluginManager.GetPlugin("SongCore") is not null;
        _isCinemaInstalled = PluginManager.GetPlugin("Cinema") is not null;
    }

    public void Initialize()
    {
        if (_isCinemaInstalled)
        {
            InitializeCinemaConnection();
        }
        if (_isSongCoreInstalled)
        {
            InitializeSongCoreConnection();
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        if (_isCinemaInstalled)
            DisposeCinemaConnection();
        if (_isSongCoreInstalled)
            DisposeSongCoreConnection();
    }

    // IMPORTANT !!!
    // Make sure to check SongCore and Cinema respectively are installed before calling the following methods
    private void InitializeSongCoreConnection()
    {
        SongCore.Plugin.CustomSongPlatformSelectionDidChange += OnSongCoreEvent;
        SongCore.Collections.RegisterCapability("Custom Platforms");
    }
    private void DisposeSongCoreConnection()
    {
        SongCore.Plugin.CustomSongPlatformSelectionDidChange -= OnSongCoreEvent;
        SongCore.Collections.DeregisterCapability("Custom Platforms");
    }

    private void InitializeCinemaConnection() => BeatSaberCinema.Events.AllowCustomPlatform += OnCinemaEvent;
    private void DisposeCinemaConnection() => BeatSaberCinema.Events.AllowCustomPlatform -= OnCinemaEvent;

    private void OnCinemaEvent(bool allowPlatform)
    {
        if (!allowPlatform)
        {
            // Setting this will make the platform spawner use the default platform
            _siraLog.Debug("Setting platform to default for Cinema.");
            _platformManager.APIRequestedPlatform = _platformManager.DefaultPlatform;
        }
    }

    /// <summary>
    /// Sets up the requested platform and downloads it if needed
    /// </summary>
    /// <param name="usePlatform">Whether the selected song requests a platform or not</param>
    /// <param name="name">The name of the requested platform</param>
    /// <param name="hash">The hash of the requested platform</param>
    /// <param name="beatmapLevel">The <see cref="BeatmapLevel"/> the platform was requested for</param>
    // ReSharper disable once AsyncVoidMethod
    private async void OnSongCoreEvent(bool usePlatform, string? name, string? hash, BeatmapLevel beatmapLevel)
    {
        // No platform is requested, abort
        if (!usePlatform)
        {
            _platformManager.APIRequestedPlatform = null;
            return;
        }

        // Check if the requested platform is already downloaded
        if (_platformManager.AllPlatforms.TryGetFirst(x => x.platHash == hash || x.platName == name, out var platform))
        {
            _platformManager.APIRequestedPlatform = platform;
            return;
        }

        string url = hash is not null ? $"https://modelsaber.com/api/v2/get.php?type=platform&filter=hash:{hash}"
            : name is not null ? $"https://modelsaber.com/api/v2/get.php?type=platform&filter=name:{name}"
            : throw new ArgumentNullException($"{nameof(hash)}, {nameof(name)}", "Invalid platform request");

        await DownloadPlatform(url, _cancellationTokenSource.Token);
    }

    private async Task DownloadPlatform(string url, CancellationToken cancellationToken)
    {
        var httpResponse = await _httpService.GetAsync(url, null, cancellationToken);
        if (!httpResponse.Successful)
        {
            return;
        }

        string json = await httpResponse.ReadAsStringAsync();
        
        var platformDownloadData = JsonConvert
            .DeserializeObject<Dictionary<string, PlatformDownloadData>>(json)?
            .FirstOrDefault().Value;
        if (platformDownloadData?.download is null)
        {
            return;
        }

        var platformHttpResponse = await _httpService.GetAsync(platformDownloadData.download, null, cancellationToken);
        if (!platformHttpResponse.Successful)
        {
            return;
        }

        byte[] platData = await platformHttpResponse.ReadAsByteArrayAsync();
        string path = Path.Combine(_platformManager.DirectoryPath, $"{platformDownloadData.name}.plat");
        await File.WriteAllBytesAsync(path, platData, cancellationToken);
        
        var newPlatform = await _platformManager.CreatePlatformAsync(path);
        if (cancellationToken.IsCancellationRequested || newPlatform == null)
        {
            return;
        }

        _siraLog.Debug($"New platform downloaded from SongCore: {newPlatform.platName}");
        _platformManager.APIRequestedPlatform = newPlatform;
        _platformManager.AllPlatforms.AddSorted(
            PlatformManager.BuildInPlatformsCount,
            _platformManager.AllPlatforms.Count - PlatformManager.BuildInPlatformsCount,
            newPlatform);
    }
}

/// <summary>
/// The class the API response of ModelSaber is deserialized on
/// </summary>
[Serializable]
file class PlatformDownloadData
{
    public string? name;
    public string? download;
}