using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Models;
using SiraUtil.Logging;
using UnityEngine;

namespace CustomFloorPlugin.PlatformManagement;

public sealed class MenuEnvironmentGrabber : IEnvironmentObjectSource
{
    private readonly MenuEnvironmentManager _menuEnvironmentManager;
    private readonly SiraLog _siraLog;
    
    private readonly List<GameObject> _menuEnvironmentObjects = [];

    public MenuEnvironmentGrabber(
        MenuEnvironmentManager menuEnvironmentManager,
        SiraLog siraLog)
    {
        _menuEnvironmentManager = menuEnvironmentManager;
        _siraLog = siraLog;

        InitializationTask = Initialize();
    }

    public Task InitializationTask { get; }
    
    public IEnumerable<GameObject> AlwaysHide => _menuEnvironmentObjects;
    public IEnumerable<GameObject> PlayersPlace => [];
    public IEnumerable<GameObject> SmallRings => [];
    public IEnumerable<GameObject> BigRings => [];
    public IEnumerable<GameObject> Visualizer => [];
    public IEnumerable<GameObject> Towers => [];
    public IEnumerable<GameObject> Highway => [];
    public IEnumerable<GameObject> BackColumns => [];
    public IEnumerable<GameObject> DoubleColorLasers => [];
    public IEnumerable<GameObject> BackLasers => [];
    public IEnumerable<GameObject> RotatingLasers => [];
    public IEnumerable<GameObject> TrackLights => [];

    public async Task Initialize()
    {
        FindMenuEnvironmentObjects();
        
        await Task.CompletedTask;
    }

    private void FindMenuEnvironmentObjects()
    {
        _menuEnvironmentObjects.Clear();
        
        var activeEnv = _menuEnvironmentManager._data
            .FirstOrDefault(env => env.wrapper.activeInHierarchy)?
            .wrapper;
        if (activeEnv == null) return;
        var env = activeEnv.transform;
        
        _menuEnvironmentObjects.AddIfNotNull(env.FindObject("MenuFogRing"));
        _menuEnvironmentObjects.AddIfNotNull(env.FindObject("BasicMenuGround"));
        _menuEnvironmentObjects.AddIfNotNull(env.FindObject("Notes"));
        _menuEnvironmentObjects.AddIfNotNull(env.FindObject("PileOfNotes"));
        
        _siraLog.Debug($"EnvironmentGrabber found objects for \"{activeEnv.name}\".");
    }
}