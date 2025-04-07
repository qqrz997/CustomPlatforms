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
using Zenject;

namespace CustomFloorPlugin.PlatformManagement;

/// <summary>
/// Responsible for downloading platforms from custom levels that require a custom platform, and disabling custom
/// platforms on levels that require Cinema.
/// </summary>
internal class ConnectionManager : IInitializable, IDisposable
{
    private readonly SiraLog _siraLog;
    private readonly PlatformManager _platformManager;

    private readonly bool _isSongCoreInstalled;
    private readonly bool _isCinemaInstalled;

    public ConnectionManager(SiraLog siraLog, PlatformManager platformManager)
    {
        _siraLog = siraLog;
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
        if (_isCinemaInstalled)
            DisposeCinemaConnection();
        if (_isSongCoreInstalled)
            DisposeSongCoreConnection();
    }

    // IMPORTANT !!!
    // Make sure to check SongCore and Cinema respectively are installed before calling the following methods
    private void InitializeSongCoreConnection()
    {
        SongCore.Collections.RegisterCapability("Custom Platforms");
    }
    private void DisposeSongCoreConnection()
    {
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
}