using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomFloorPlugin.Helpers;
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

    public ConnectionManager(SiraLog siraLog, PlatformManager platformManager)
    {
        _siraLog = siraLog;
        _platformManager = platformManager;
    }

    public void Initialize()
    {
        if (InstalledMods.Cinema)
        {
            InitializeCinemaConnection();
        }
        if (InstalledMods.SongCore)
        {
            InitializeSongCoreConnection();
        }
    }

    public void Dispose()
    {
        if (InstalledMods.Cinema)
            DisposeCinemaConnection();
        if (InstalledMods.SongCore)
            DisposeSongCoreConnection();
    }

    // IMPORTANT !!!
    // Make sure to check SongCore and Cinema respectively are installed before calling the following methods
    private void InitializeSongCoreConnection() => SongCore.Collections.RegisterCapability("Custom Platforms");
    private void DisposeSongCoreConnection() => SongCore.Collections.DeregisterCapability("Custom Platforms");

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