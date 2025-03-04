using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Models;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using UnityEngine;
using Zenject;

namespace CustomFloorPlugin.PlatformManagement;

public sealed class MultiplayerEnvironmentGrabber : IEnvironmentObjectSource
{
    private readonly Transform _envRoot;
    
    private readonly List<GameObject> _playersPlace = [];
    private readonly List<GameObject> _highway = [];

    public MultiplayerEnvironmentGrabber(MultiplayerLocalActivePlayerFacade multiplayerLocalActivePlayerFacade)
    {
        _envRoot = multiplayerLocalActivePlayerFacade.transform;

        InitializationTask = Initialize();
    }

    public Task InitializationTask { get; }

    public IEnumerable<GameObject> AlwaysHide => [];
    public IEnumerable<GameObject> PlayersPlace => _playersPlace;
    public IEnumerable<GameObject> SmallRings => [];
    public IEnumerable<GameObject> BigRings => [];
    public IEnumerable<GameObject> Visualizer => [];
    public IEnumerable<GameObject> Towers => [];
    public IEnumerable<GameObject> Highway => _highway;
    public IEnumerable<GameObject> BackColumns => [];
    public IEnumerable<GameObject> DoubleColorLasers => [];
    public IEnumerable<GameObject> BackLasers => [];
    public IEnumerable<GameObject> RotatingLasers => [];
    public IEnumerable<GameObject> TrackLights => [];

    public async Task Initialize()
    {
        await Task.CompletedTask;
        
        FindPlayersPlace();
        FindHighway();
    }

    private void FindPlayersPlace()
    {
        _playersPlace.AddIfNotNull(_envRoot.FindObject("IsActiveObjects/Construction/PlayersPlace/Mirror"));
        _playersPlace.AddIfNotNull(_envRoot.FindObject("IsActiveObjects/Construction/PlayersPlace/Construction"));
        _playersPlace.AddIfNotNull(_envRoot.FindObject("IsActiveObjects/Construction/PlayersPlace/RectangleFakeGlow"));
        _playersPlace.AddIfNotNull(_envRoot.FindObject("IsActiveObjects/Construction/PlayersPlace/Frame"));
        _playersPlace.AddIfNotNull(_envRoot.FindObject("IsActiveObjects/Construction/PlayersPlace/SaberBurnMarksParticles"));
        _playersPlace.AddIfNotNull(_envRoot.FindObject("IsActiveObjects/Construction/PlayersPlace/SaberBurnMarksArea"));
        _playersPlace.AddIfNotNull(_envRoot.FindObject("IsActiveObjects/Construction/PlayersPlace/Collider"));
    }
    
    private void FindHighway()
    {
        _highway.AddIfNotNull(_envRoot.FindObject("IsActiveObjects/Construction/ConstructionL"));
        _highway.AddIfNotNull(_envRoot.FindObject("IsActiveObjects/Construction/ConstructionR"));
        _highway.AddIfNotNull(_envRoot.FindObject("IsActiveObjects/Lasers"));
        if (_envRoot.transform.Find("IsActiveObjects/CenterRings"))
        {
            _highway.AddIfNotNull(_envRoot.FindObject("IsActiveObjects/CenterRings"));
            _highway.AddIfNotNull(_envRoot.FindObject("IsActiveObjects/PlatformEnd"));
        }
        else
        {
            _highway.AddIfNotNull(_envRoot.FindObject("Construction"));
            _highway.AddIfNotNull(_envRoot.FindObject("Lasers"));
        }
    }
}

public sealed class MultiplayerEnvironmentHider : IEnvironmentHider
{
    private readonly IEnvironmentObjectSource _environmentObjectSource;
    private readonly GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
    private readonly SiraLog _siraLog;

    public MultiplayerEnvironmentHider(
        IEnvironmentObjectSource environmentObjectSource,
        GameplayCoreSceneSetupData gameplayCoreSceneSetupData,
        SiraLog siraLog)
    {
        _environmentObjectSource = environmentObjectSource;
        _gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
        _siraLog = siraLog;
    }

    public async Task HideObjectsForPlatform(CustomPlatform customPlatform)
    {
        await _environmentObjectSource.InitializationTask;
        
        _siraLog.Debug($"Hiding objects for {_gameplayCoreSceneSetupData.targetEnvironmentInfo.serializedName}");

        _environmentObjectSource.AlwaysHide.SetActive(false);
        _environmentObjectSource.PlayersPlace.SetActive(!customPlatform.hideDefaultPlatform);
        _environmentObjectSource.SmallRings.SetActive(!customPlatform.hideSmallRings);
        _environmentObjectSource.BigRings.SetActive(!customPlatform.hideBigRings);
        _environmentObjectSource.Visualizer.SetActive(!customPlatform.hideEQVisualizer);
        _environmentObjectSource.Towers.SetActive(!customPlatform.hideTowers);
        _environmentObjectSource.Highway.SetActive(!customPlatform.hideHighway);
        _environmentObjectSource.BackColumns.SetActive(!customPlatform.hideBackColumns);
        _environmentObjectSource.BackLasers.SetActive(!customPlatform.hideBackLasers);
        _environmentObjectSource.DoubleColorLasers.SetActive(!customPlatform.hideDoubleColorLasers);
        _environmentObjectSource.RotatingLasers.SetActive(!customPlatform.hideRotatingLasers);
        _environmentObjectSource.TrackLights.SetActive(!customPlatform.hideTrackLights);
    }
}

public sealed class MultiplayerPlatformSpawner : IPlatformSpawner, IInitializable
{
    private readonly PlatformManager _platformManager;
    private readonly DiContainer _container;
    private readonly IEnvironmentHider _environmentHider;

    public MultiplayerPlatformSpawner(
        PlatformManager platformManager,
        DiContainer container,
        IEnvironmentHider environmentHider)
    {
        _platformManager = platformManager;
        _container = container;
        _environmentHider = environmentHider;
    }

    public async void Initialize()
    {
        await SpawnPlatform(_platformManager.MultiplayerPlatform);
        await _environmentHider.HideObjectsForPlatform(_platformManager.MultiplayerPlatform);
    }

    public async Task<CustomPlatform> SpawnPlatform(CustomPlatform customPlatform)
    {
        return await _platformManager.SpawnPlatform(customPlatform, _container);
    }
}