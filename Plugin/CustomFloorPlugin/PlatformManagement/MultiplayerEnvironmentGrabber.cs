using System.Collections.Generic;
using System.Threading.Tasks;
using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Models;
using UnityEngine;

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
        FindPlayersPlace();
        FindHighway();
        
        await Task.CompletedTask;
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