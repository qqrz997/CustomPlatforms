using System;
using System.IO;
using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Models;
using IPA.Utilities;
using SiraUtil.Logging;
using Zenject;

namespace CustomFloorPlugin.PlatformManagement;

internal class PlatformFileUpdater : IInitializable, IDisposable
{
    private readonly SiraLog _siraLog;
    private readonly PlatformManager _platformManager;
    private readonly IPlatformSpawner _platformSpawner;
    
    private readonly FileSystemWatcher _fileSystemWatcher;

    public PlatformFileUpdater(SiraLog siraLog, PlatformManager platformManager, IPlatformSpawner platformSpawner)
    {
        _siraLog = siraLog;
        _platformManager = platformManager;
        _platformSpawner = platformSpawner;
        
        _fileSystemWatcher = new(_platformManager.DirectoryPath, "*.plat");
    }
    
    public void Initialize()
    {
        _fileSystemWatcher.Changed += OnFileChanged;
        _fileSystemWatcher.Created += OnFileCreated;
        _fileSystemWatcher.Deleted += OnFileDeleted;
        _fileSystemWatcher.EnableRaisingEvents = true;
    }

    public void Dispose()
    {
        _fileSystemWatcher.Changed -= OnFileChanged;
        _fileSystemWatcher.Created -= OnFileCreated;
        _fileSystemWatcher.Deleted -= OnFileDeleted;
        _fileSystemWatcher.Dispose();
    }
    
    // TODO: this currently doesn't update a platform's metadata if it were to change
    private async void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _siraLog.Debug("A platform file was changed...");
        if (!UnityGame.OnMainThread)
        {
            await UnityGame.SwitchToMainThreadAsync();
        }

        if (_platformManager.AllPlatforms.TryGetFirst(x => x.fullPath == e.FullPath, out var platform))
        {
            bool wasActivePlatform = platform == _platformManager.ActivePlatform;
            var newPlatform = await _platformManager.CreatePlatformAsync(e.FullPath);
            if (wasActivePlatform && newPlatform != null)
            {
                _siraLog.Debug("Current platform was updated");
                await _platformSpawner.SpawnPlatform(newPlatform);
            }
        }
    }

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        _siraLog.Debug("A new platform file was found...");
        if (!UnityGame.OnMainThread)
        {
            await UnityGame.SwitchToMainThreadAsync();
        }

        var  newPlatform = await _platformManager.CreatePlatformAsync(e.FullPath);
        if (newPlatform != null)
        {
            _siraLog.Debug($"Adding new platform \"{newPlatform.platName}\" to platform list");
            _platformManager.AllPlatforms.AddSorted(
                PlatformManager.BuildInPlatformsCount,
                _platformManager.AllPlatforms.Count - PlatformManager.BuildInPlatformsCount,
                newPlatform);
        }
    }
    
    private async void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        _siraLog.Debug("A platform file was deleted...");
        if (!UnityGame.OnMainThread)
        {
            await UnityGame.SwitchToMainThreadAsync();
        }

        if (_platformManager.AllPlatforms.TryGetFirst(x => x.fullPath == e.FullPath, out var platform))
        {
            if (platform == _platformManager.ActivePlatform)
            {
                // Switch off of the selected platform if it is deleted
                await _platformSpawner.SpawnPlatform(_platformManager.DefaultPlatform);
            }
            _platformManager.AllPlatforms.Remove(platform);
            UnityEngine.Object.Destroy(platform.gameObject);
        }
    }
}