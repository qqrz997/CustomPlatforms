using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Models;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using UnityEngine;

namespace CustomFloorPlugin.PlatformManagement;

public class StandardGameplayEnvironmentGrabber : IEnvironmentObjectSource, IAffinity
{
    private readonly SiraLog _siraLog;
    private readonly Transform _envRoot;
    private readonly string _envName;
    
    private readonly List<GameObject> _playersPlace = [];
    private readonly List<GameObject> _smallRings = [];
    private readonly List<GameObject> _bigRings = [];
    private readonly List<GameObject> _visualizer = [];
    private readonly List<GameObject> _towers = [];
    private readonly List<GameObject> _highway = [];
    private readonly List<GameObject> _backColumns = [];
    private readonly List<GameObject> _doubleColorLasers = [];
    private readonly List<GameObject> _backLasers = [];
    private readonly List<GameObject> _rotatingLasers = [];
    private readonly List<GameObject> _trackLights = [];

    public StandardGameplayEnvironmentGrabber(
        SiraLog siraLog,
        LightWithIdManager lightWithIdManager,
        GameplayCoreSceneSetupData gameplayCoreSceneSetupData)
    {
        _siraLog = siraLog;
        _envRoot = lightWithIdManager.transform.parent;
        _envName = gameplayCoreSceneSetupData.targetEnvironmentInfo.serializedName;

        InitializationTask = Initialize();
    }

    public Task InitializationTask { get; }

    public IEnumerable<GameObject> AlwaysHide => [];
    public IEnumerable<GameObject> PlayersPlace => _playersPlace;
    public IEnumerable<GameObject> SmallRings => _smallRings;
    public IEnumerable<GameObject> BigRings => _bigRings;
    public IEnumerable<GameObject> Visualizer => _visualizer;
    public IEnumerable<GameObject> Towers => _towers;
    public IEnumerable<GameObject> Highway => _highway;
    public IEnumerable<GameObject> BackColumns => _backColumns;
    public IEnumerable<GameObject> DoubleColorLasers => _doubleColorLasers;
    public IEnumerable<GameObject> BackLasers => _backLasers;
    public IEnumerable<GameObject> RotatingLasers => _rotatingLasers;
    public IEnumerable<GameObject> TrackLights => _trackLights;

    private bool _environmentDidStart;
    
    [AffinityPatch(typeof(TrackLaneRingsManager), nameof(TrackLaneRingsManager.Start))]
    private void TrackLaneRingsManagerStart() => _environmentDidStart = true;

    private async Task WaitForRingsStart(int timeoutMilliseconds)
    {
        if (!EnvironmentHasTrackLaneRingsManager(_envName)) return;
        while (!_environmentDidStart && (timeoutMilliseconds -= 50) > 0)
            await Task.Delay(50);
    }

    public async Task Initialize()
    {
        await WaitForRingsStart(timeoutMilliseconds: 850);
        
        FindPlayersPlace();
        FindSmallRings();
        FindBigRings();
        FindVisualizers();
        FindTowers();
        FindHighway();
        FindBackColumns();
        FindRotatingLasers();
        FindDoubleColorLasers();
        FindBackLasers();
        FindTrackLights();
        
        _siraLog.Debug($"EnvironmentGrabber found objects for \"{_envName}\".");
    }
    
    private void FindPlayersPlace()
    {
        _playersPlace.AddIfNotNull(_envRoot.FindObject("PlayersPlace/Mirror"));
        _playersPlace.AddIfNotNull(_envRoot.FindObject("PlayersPlace/Construction"));
        _playersPlace.AddIfNotNull(_envRoot.FindObject("PlayersPlace/RectangleFakeGlow"));
        _playersPlace.AddIfNotNull(_envRoot.FindObject("PlayersPlace/Frame"));
        _playersPlace.AddIfNotNull(_envRoot.FindObject("PlayersPlace/SaberBurnMarksParticles"));
        _playersPlace.AddIfNotNull(_envRoot.FindObject("PlayersPlace/SaberBurnMarksArea"));
        _playersPlace.AddIfNotNull(_envRoot.FindObject("PlayersPlace/Collider"));
    }
    
    private void FindSmallRings()
    {
        TrackLaneRingsManager? ringsManager = null;
        switch (_envName)
        {
            case "TutorialEnvironment":
            case "DefaultEnvironment":
            case "NiceEnvironment":
            case "MonstercatEnvironment":
            case "CrabRaveEnvironment":
                ringsManager = _envRoot.Find("SmallTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                break;
            case "TriangleEnvironment":
                ringsManager = _envRoot.Find("TriangleTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                break;
            case "DragonsEnvironment":
                ringsManager = _envRoot.Find("PanelsTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                break;
            case "PanicEnvironment":
                ringsManager = _envRoot.Find("Panels4TrackLaneRings").GetComponent<TrackLaneRingsManager>();
                break;
            case "GreenDayEnvironment":
                ringsManager = _envRoot.Find("LightLinesTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                break;
            case "TimbalandEnvironment":
                ringsManager = _envRoot.Find("PairLaserTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                break;
            case "FitBeatEnvironment":
                ringsManager = _envRoot.Find("PanelsLightsTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                break;
            case "KaleidoscopeEnvironment":
                ringsManager = _envRoot.Find("SmallTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                break;
            case "SkrillexEnvironment":
                ringsManager = _envRoot.Find("TrackLaneRings1").GetComponent<TrackLaneRingsManager>();
                break;
            case "TheSecondEnvironment":
                _smallRings.AddIfNotNull(_envRoot.FindObject("SmallTrackLaneRingsGroup"));
                return;
        }
    
        if (ringsManager == null) return;
        _smallRings.AddIfNotNull(ringsManager.gameObject);
        foreach (var ringObject in ringsManager.Rings.Select(static x => x.gameObject))
        {
            _smallRings.AddIfNotNull(ringObject);
        }
    }
    
    private void FindBigRings()
    {
        TrackLaneRingsManager? ringsManager = null;
        switch (_envName)
        {
            case "DefaultEnvironment":
            case "TriangleEnvironment":
            case "NiceEnvironment":
            case "BigMirrorEnvironment":
            case "DragonsEnvironment":
                ringsManager = _envRoot.Find("BigTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                break;
            case "OriginsEnvironment":
                ringsManager = _envRoot.Find("BigLightsTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                break;
            case "FitBeatEnvironment":
                ringsManager = _envRoot.Find("BigCenterLightsTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                break;
            case "KaleidoscopeEnvironment":
                ringsManager = _envRoot.Find("DistantRings").GetComponent<TrackLaneRingsManager>();
                break;
            case "SkrillexEnvironment":
                ringsManager = _envRoot.Find("TrackLaneRings2").GetComponent<TrackLaneRingsManager>();
                break;
            case "TheSecondEnvironment":
                _bigRings.AddIfNotNull(_envRoot.FindObject("BigTrackLaneRingsGroup"));
                return;
        }
    
        if (ringsManager == null) return;
        _bigRings.AddIfNotNull(ringsManager.gameObject);
        foreach (var ringObject in ringsManager.Rings.Select(static x => x.gameObject))
        {
            _bigRings.AddIfNotNull(ringObject);
        }
    }
    
    private void FindVisualizers()
    {
        switch (_envName)
        {
            case "TheSecondEnvironment":
                _visualizer.AddIfNotNull(_envRoot.FindObject("SpectrogramsTheSecond"));
                break;
            default:
                _visualizer.AddIfNotNull(_envRoot.FindObject("Spectrograms"));
                break;
        }
    }
    
    private void FindTowers()
    {
        switch (_envName)
        {
            case "GlassDesertEnvironment":
                for (int i = 2; i < 25; i++)
                    _towers.AddIfNotNull(_envRoot.FindObject($"GameObject ({i})"));
                break;
            case "TutorialEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("Buildings"));
                break;
            case "DefaultEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("NearBuildingLeft (1)"));
                _towers.AddIfNotNull(_envRoot.FindObject("NearBuildingRight (1)"));
                _towers.AddIfNotNull(_envRoot.FindObject("NearBuildingLeft (2)"));
                _towers.AddIfNotNull(_envRoot.FindObject("NearBuildingRight (2)"));
                break;
            case "TriangleEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("NearBuildingLeft"));
                _towers.AddIfNotNull(_envRoot.FindObject("NearBuildingRight"));
                break;
            case "NiceEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("NearBuildingLeft (1)"));
                _towers.AddIfNotNull(_envRoot.FindObject("NearBuildingRight (1)"));
                _towers.AddIfNotNull(_envRoot.FindObject("NearBuildingLeft (2)"));
                _towers.AddIfNotNull(_envRoot.FindObject("NearBuildingRight (2)"));
                break;
            case "BigMirrorEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("NearBuildingLeft"));
                _towers.AddIfNotNull(_envRoot.FindObject("NearBuildingRight"));
                break;
            case "DragonsEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("HallConstruction"));
                break;
            case "KDAEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("TentacleLeft"));
                _towers.AddIfNotNull(_envRoot.FindObject("TentacleRight"));
                break;
            case "MonstercatEnvironment":
            case "CrabRaveEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("NearBuildingLeft"));
                _towers.AddIfNotNull(_envRoot.FindObject("NearBuildingRight"));
                _towers.AddIfNotNull(_envRoot.FindObject("FarBuildings"));
                break;
            case "PanicEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("TopCones"));
                _towers.AddIfNotNull(_envRoot.FindObject("BottomCones"));
                break;
            case "RocketEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("RocketCarL"));
                _towers.AddIfNotNull(_envRoot.FindObject("RocketCarR"));
                _towers.AddIfNotNull(_envRoot.FindObject("RocketArena"));
                _towers.AddIfNotNull(_envRoot.FindObject("RocketArenaLight"));
                _towers.AddIfNotNull(_envRoot.FindObject("EnvLight0"));
                for (int i = 2; i < 10; i++)
                    _towers.AddIfNotNull(_envRoot.FindObject($"EnvLight0 ({i})"));
                break;
            case "GreenDayEnvironment":
            case "GreenDayGrenadeEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("GreenDayCity"));
                break;
            case "TimbalandEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("Buildings"));
                _towers.AddIfNotNull(_envRoot.FindObject("MainStructure"));
                _towers.AddIfNotNull(_envRoot.FindObject("TopStructure"));
                _towers.AddIfNotNull(_envRoot.FindObject("TimbalandLogo"));
                for (int i = 0; i < 4; i++)
                    _towers.AddIfNotNull(_envRoot.FindObject($"TimbalandLogo ({i})"));
                break;
            case "BTSEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("PillarTrackLaneRingsR"));
                _towers.AddIfNotNull(_envRoot.FindObject("PillarTrackLaneRingsR (1)"));
                _towers.AddIfNotNull(_envRoot.FindObject("PillarsMovementEffect"));
                _towers.AddIfNotNull(_envRoot.FindObject("PillarPair"));
                for (int i = 1; i < 5; i++)
                    _towers.AddIfNotNull(_envRoot.FindObject($"PillarPair ({i})"));
                _towers.AddIfNotNull(_envRoot.FindObject("SmallPillarPair"));
                for (int i = 1; i < 4; i++)
                    _towers.AddIfNotNull(_envRoot.FindObject($"SmallPillarPair ({i})"));
                break;
            case "BillieEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("Mountains"));
                _towers.AddIfNotNull(_envRoot.FindObject("Clouds"));
                break;
            case "GagaEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("TeslaTower1L"));
                _towers.AddIfNotNull(_envRoot.FindObject("TeslaTower1R"));
                _towers.AddIfNotNull(_envRoot.FindObject("TeslaTower2L"));
                _towers.AddIfNotNull(_envRoot.FindObject("TeslaTower2R"));
                _towers.AddIfNotNull(_envRoot.FindObject("TeslaTower3L"));
                _towers.AddIfNotNull(_envRoot.FindObject("TeslaTower3R"));
                _towers.AddIfNotNull(_envRoot.FindObject("TubeR"));
                _towers.AddIfNotNull(_envRoot.FindObject("TubeL"));
                _towers.AddIfNotNull(_envRoot.FindObject("TubeL (1)"));
                break;
            case "TheSecondEnvironment":
                _towers.AddIfNotNull(_envRoot.FindObject("Buildings"));
                break;
        }
    }
    
    private void FindHighway()
    {
        switch (_envName)
        {
            case "GlassDesertEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("Cube"));
                _highway.AddIfNotNull(_envRoot.FindObject("Floor"));
                break;
            case "TutorialEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("Floor"));
                break;
            case "DefaultEnvironment":
            case "PanicEnvironment":
            case "GreenDayEnvironment":
            case "GreenDayGrenadeEnvironment":
            case "TimbalandEnvironment":
            case "FitBeatEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("TrackMirror"));
                _highway.AddIfNotNull(_envRoot.FindObject("TrackConstruction"));
                break;
            case "OriginsEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("Construction"));
                _highway.AddIfNotNull(_envRoot.FindObject("TrackMirror"));
                _highway.AddIfNotNull(_envRoot.FindObject("TrackConstruction"));
                break;
            case "TriangleEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("FloorConstruction"));
                _highway.AddIfNotNull(_envRoot.FindObject("TrackMirror"));
                break;
            case "NiceEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("Floor"));
                break;
            case "BigMirrorEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("Floor"));
                _highway.AddIfNotNull(_envRoot.FindObject("Construction"));
                break;
            case "DragonsEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("TrackMirror"));
                _highway.AddIfNotNull(_envRoot.FindObject("TrackConstruction"));
                _highway.AddIfNotNull(_envRoot.FindObject("Underground"));
                break;
            case "KDAEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("Construction"));
                _highway.AddIfNotNull(_envRoot.FindObject("FloorMirror"));
                break;
            case "MonstercatEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("TrackMirror"));
                _highway.AddIfNotNull(_envRoot.FindObject("VConstruction"));
                _highway.AddIfNotNull(_envRoot.FindObject("MonstercatLogoL"));
                _highway.AddIfNotNull(_envRoot.FindObject("MonstercatLogoR"));
                _highway.AddIfNotNull(_envRoot.FindObject("Construction"));
                break;
            case "CrabRaveEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("TrackMirror"));
                _highway.AddIfNotNull(_envRoot.FindObject("VConstruction"));
                _highway.AddIfNotNull(_envRoot.FindObject("Construction"));
                break;
            case "RocketEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("Mirror"));
                _highway.AddIfNotNull(_envRoot.FindObject("Construction"));
                break;
            case "LinkinParkEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("TrackMirror"));
                _highway.AddIfNotNull(_envRoot.FindObject("TrackConstruction"));
                _highway.AddIfNotNull(_envRoot.FindObject("Tunnel"));
                for (int i = 1; i < 11; i++)
                    _highway.AddIfNotNull(_envRoot.FindObject($"Tunnel ({i})"));
                _highway.AddIfNotNull(_envRoot.FindObject("TunnelRings"));
                _highway.AddIfNotNull(_envRoot.FindObject("LinkinParkSoldier"));
                _highway.AddIfNotNull(_envRoot.FindObject("LinkinParkTextLogoL"));
                _highway.AddIfNotNull(_envRoot.FindObject("LinkinParkTextLogoR"));
                break;
            case "BTSEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("TrackMirror"));
                _highway.AddIfNotNull(_envRoot.FindObject("Construction"));
                _highway.AddIfNotNull(_envRoot.FindObject("Clouds"));
                _highway.AddIfNotNull(_envRoot.FindObject("StarHemisphere"));
                _highway.AddIfNotNull(_envRoot.FindObject("StarEmitterPS"));
                _highway.AddIfNotNull(_envRoot.FindObject("BTSStarTextEffectEvent"));
                break;
            case "SkrillexEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("TrackBL"));
                _highway.AddIfNotNull(_envRoot.FindObject("TrackBR"));
                _highway.AddIfNotNull(_envRoot.FindObject("TrackTR"));
                _highway.AddIfNotNull(_envRoot.FindObject("TrackTL"));
                break;
            case "KaleidoscopeEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("TrackMirror"));
                _highway.AddIfNotNull(_envRoot.FindObject("Construction"));
                break;
            case "InterscopeEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("Logo"));
                _highway.AddIfNotNull(_envRoot.FindObject("Floor"));
                _highway.AddIfNotNull(_envRoot.FindObject("Spectrograms"));
                _highway.AddIfNotNull(_envRoot.FindObject("Pillar/PillarL"));
                _highway.AddIfNotNull(_envRoot.FindObject("Pillar/PillarR"));
                for (int i = 1; i < 5; i++)
                {
                    string baseName = $"Pillar ({i})";
                    _highway.AddIfNotNull(_envRoot.FindObject($"{baseName}/PillarL"));
                    _highway.AddIfNotNull(_envRoot.FindObject($"{baseName}/PillarR"));
                }
    
                _highway.AddIfNotNull(_envRoot.FindObject("RearPillar"));
                for (int i = 1; i < 6; i++)
                {
                    _highway.AddIfNotNull(_envRoot.FindObject($"RearPillar ({i})"));
                    _highway.AddIfNotNull(_envRoot.FindObject($"Plane ({i})"));
                }
    
                for (int i = 1; i < 5; i++)
                {
                    _highway.AddIfNotNull(_envRoot.FindObject($"Car{i}"));
                    _highway.AddIfNotNull(_envRoot.FindObject($"FarCar{i}"));
                }
    
                break;
            case "BillieEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("Rain"));
                _highway.AddIfNotNull(_envRoot.FindObject("Waterfall"));
                _highway.AddIfNotNull(_envRoot.FindObject("LeftRail"));
                _highway.AddIfNotNull(_envRoot.FindObject("RightRail"));
                _highway.AddIfNotNull(_envRoot.FindObject("LeftFarRail1"));
                _highway.AddIfNotNull(_envRoot.FindObject("LeftFarRail2"));
                _highway.AddIfNotNull(_envRoot.FindObject("RightFarRail1"));
                _highway.AddIfNotNull(_envRoot.FindObject("RightFarRail2"));
                _highway.AddIfNotNull(_envRoot.FindObject("RailingFullBack"));
                _highway.AddIfNotNull(_envRoot.FindObject("RailingFullFront"));
                _highway.AddIfNotNull(_envRoot.FindObject("LastRailingCurve"));
                _highway.AddIfNotNull(_envRoot.FindObject("WaterRainRipples"));
                break;
            case "HalloweenEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("Ground"));
                for (int i = 1; i < 92; i++)
                    _highway.AddIfNotNull(_envRoot.FindObject($"GroundStone ({i})"));
                _highway.AddIfNotNull(_envRoot.FindObject("Fence")?.AppendToName("renamed"));
                _highway.AddIfNotNull(_envRoot.FindObject("Fence"));
                for (int i = 1; i < 25; i++)
                    _highway.AddIfNotNull(_envRoot.FindObject($"Fence ({i})"));
                _highway.AddIfNotNull(_envRoot.FindObject("Grave")?.AppendToName("renamed"));
                _highway.AddIfNotNull(_envRoot.FindObject("Grave")?.AppendToName("renamed"));
                _highway.AddIfNotNull(_envRoot.FindObject("Grave0"));
                _highway.AddIfNotNull(_envRoot.FindObject("Grave0 (1)"));
                _highway.AddIfNotNull(_envRoot.FindObject("Grave0 (2)"));
                _highway.AddIfNotNull(_envRoot.FindObject("Grave1"));
                _highway.AddIfNotNull(_envRoot.FindObject("Grave1 (1)"));
                _highway.AddIfNotNull(_envRoot.FindObject("Castle"));
                for (int i = 1; i < 4; i++)
                    _highway.AddIfNotNull(_envRoot.FindObject($"Castle ({i})"));
                _highway.AddIfNotNull(_envRoot.FindObject("Tree1 (1)"));
                _highway.AddIfNotNull(_envRoot.FindObject("Tree2"));
                _highway.AddIfNotNull(_envRoot.FindObject("Tree3"));
                _highway.AddIfNotNull(_envRoot.FindObject("Tree5"));
                for (int i = 1; i < 4; i++)
                {
                    _highway.AddIfNotNull(_envRoot.FindObject($"Tree3 ({i})"));
                    _highway.AddIfNotNull(_envRoot.FindObject($"Tree2 ({i})"));
                }
    
                for (int i = 2; i < 25; i++)
                    _highway.AddIfNotNull(_envRoot.FindObject($"TombStone ({i})"));
                _highway.AddIfNotNull(_envRoot.FindObject("ZombieHand"));
                for (int i = 1; i < 7; i++)
                    _highway.AddIfNotNull(_envRoot.FindObject($"ZombieHand ({i})"));
                _highway.AddIfNotNull(_envRoot.FindObject("Crow"));
                for (int i = 1; i < 4; i++)
                    _highway.AddIfNotNull(_envRoot.FindObject($"Crow ({i})"));
                _highway.AddIfNotNull(_envRoot.FindObject("Bats"));
                _highway.AddIfNotNull(_envRoot.FindObject("GroundFog"));
                break;
            case "GagaEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("Construction"));
                _highway.AddIfNotNull(_envRoot.FindObject("Runway"));
                _highway.AddIfNotNull(_envRoot.FindObject("RunwayPillar"));
                _highway.AddIfNotNull(_envRoot.FindObject("RunwayPillarLow (1)"));
                _highway.AddIfNotNull(_envRoot.FindObject("RunwayPillarLow (2)"));
                _highway.AddIfNotNull(_envRoot.FindObject("BackCube"));
                _highway.AddIfNotNull(_envRoot.FindObject("Logo"));
                break;
            case "PyroEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("PlayerSetup"));
                _highway.AddIfNotNull(_envRoot.FindObject("Runway"));
                _highway.AddIfNotNull(_envRoot.FindObject("Fire"));
                _highway.AddIfNotNull(_envRoot.FindObject("SmokeLeft"));
                _highway.AddIfNotNull(_envRoot.FindObject("CrowdFlipbookGroup"));
                _highway.AddIfNotNull(_envRoot.FindObject("ScreenSetupLeft"));
                _highway.AddIfNotNull(_envRoot.FindObject("ScreenSetupRight"));
                _highway.AddIfNotNull(_envRoot.FindObject("StageRing"));
                _highway.AddIfNotNull(_envRoot.FindObject("FrontScaffolding"));
                _highway.AddIfNotNull(_envRoot.FindObject("ProjectorArray"));
                break;
            case "EDMEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("Spectrograms"));
                _highway.AddIfNotNull(_envRoot.FindObject("Spectrograms (1)"));
                break;
            case "TheSecondEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("TrackMirror"));
                _highway.AddIfNotNull(_envRoot.FindObject("TrackConstruction"));
                break;
        }
    }
    
    private void FindBackColumns()
    {
        switch (_envName)
        {
            case "GlassDesertEnvironment":
                _backColumns.AddIfNotNull(_envRoot.FindObject("SeparatorWall"));
                for (int i = 1; i < 16; i++)
                    _backColumns.AddIfNotNull(_envRoot.FindObject($"SeparatorWall ({i})"));
                break;
            case "MonstercatEnvironment":
                _backColumns.AddIfNotNull(_envRoot.FindObject("SpectrogramEnd"));
                break;
            default:
                _backColumns.AddIfNotNull(_envRoot.FindObject("BackColumns"));
                break;
        }
    }
    
    private void FindRotatingLasers()
    {
        switch (_envName)
        {
            case "GlassDesertEnvironment":
                for (int i = 9; i < 13; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"LightPillar ({i})"));
                for (int i = 19; i < 26; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"LightPillar ({i})"));
                break;
            case "DefaultEnvironment":
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("RotatingLasersPair"));
                for (int i = 1; i < 4; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"RotatingLasersPair ({i})"));
                break;
            case "OriginsEnvironment":
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("RotatingLasersPair"));
                for (int i = 1; i < 5; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"RotatingLasersPair ({i})"));
                break;
            case "TriangleEnvironment":
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("RotatingLasersPair"));
                for (int i = 1; i < 7; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"RotatingLasersPair ({i})"));
                break;
            case "NiceEnvironment":
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("RotatingLaserLeft"));
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("RotatingLaserRight"));
                for (int i = 1; i < 4; i++)
                {
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"RotatingLaserLeft ({i})"));
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"RotatingLaserRight ({i})"));
                }
    
                break;
            case "BigMirrorEnvironment":
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("RotatingLasersPair"));
                for (int i = 1; i < 4; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"RotatingLasersPair ({i})"));
                break;
            case "DragonsEnvironment":
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("RotatingLasersPair"));
                for (int i = 1; i < 5; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"RotatingLasersPair ({i})"));
                break;
            case "KDAEnvironment":
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("RotatingLasersPair"));
                for (int i = 1; i < 7; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"RotatingLasersPair ({i})"));
                break;
            case "MonstercatEnvironment":
            case "CrabRaveEnvironment":
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("RotatingLasersPair"));
                for (int i = 1; i < 5; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"RotatingLasersPair ({i})"));
                break;
            case "PanicEnvironment":
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("RotatingLasersPair"));
                for (int i = 1; i < 7; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"RotatingLasersPair ({i})"));
                break;
            case "RocketEnvironment":
                for (int i = 7; i < 14; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"RotatingLasersPair ({i})"));
                break;
            case "GreenDayEnvironment":
            case "GreenDayGrenadeEnvironment":
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("RotatingLasersPair"));
                for (int i = 1; i < 6; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"RotatingLasersPair ({i})"));
                break;
            case "FitBeatEnvironment":
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("RotatingLasersPair"));
                for (int i = 1; i < 8; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"RotatingLasersPair ({i})"));
                break;
            case "LinkinParkEnvironment":
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("TunnelRotatingLasersPair"));
                for (int i = 1; i < 18; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"TunnelRotatingLasersPair ({i})"));
                break;
            case "BillieEnvironment":
                for (int i = 4; i < 15; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"TunnelRotatingLasersPair ({i})"));
                break;
            case "HalloweenEnvironment":
                for (int i = 7; i < 24; i++)
                    _rotatingLasers.AddIfNotNull(_envRoot.FindObject($"RotatingLasersPair ({i})"));
                break;
            case "TheSecondEnvironment":
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("SpotlightGroupLeft"));
                _rotatingLasers.AddIfNotNull(_envRoot.FindObject("SpotlightGroupRight"));
                break;
        }
    }
    
    private void FindDoubleColorLasers()
    {
        switch (_envName)
        {
            case "TutorialEnvironment":
                for (int i = 10; i < 20; i++)
                    _doubleColorLasers.AddIfNotNull(_envRoot.FindObject($"DoubleColorLaser ({i})"));
                break;
            case "DefaultEnvironment":
                _doubleColorLasers.AddIfNotNull(_envRoot.FindObject("DoubleColorLaserL"));
                _doubleColorLasers.AddIfNotNull(_envRoot.FindObject("DoubleColorLaserR"));
                for (int i = 1; i < 5; i++)
                {
                    _doubleColorLasers.AddIfNotNull(_envRoot.FindObject($"DoubleColorLaserL ({i})"));
                    _doubleColorLasers.AddIfNotNull(_envRoot.FindObject($"DoubleColorLaserR ({i})"));
                }
    
                break;
            case "OriginsEnvironment":
                _doubleColorLasers.AddIfNotNull(_envRoot.FindObject("Laser"));
                for (int i = 1; i < 4; i++)
                    _doubleColorLasers.AddIfNotNull(_envRoot.FindObject($"Laser ({i})"));
                break;
            case "TriangleEnvironment":
                _doubleColorLasers.AddIfNotNull(_envRoot.FindObject("DoubleColorLaser"));
                for (int i = 1; i < 10; i++)
                    _doubleColorLasers.AddIfNotNull(_envRoot.FindObject($"DoubleColorLaser ({i})"));
                break;
            case "NiceEnvironment":
                _doubleColorLasers.AddIfNotNull(_envRoot.FindObject("DoubleColorLaser"));
                for (int i = 1; i < 8; i++)
                    _doubleColorLasers.AddIfNotNull(_envRoot.FindObject($"DoubleColorLaser ({i})"));
                break;
            case "BigMirrorEnvironment":
                _doubleColorLasers.AddIfNotNull(_envRoot.FindObject("DoubleColorLaser"));
                for (int i = 1; i < 10; i++)
                    _doubleColorLasers.AddIfNotNull(_envRoot.FindObject($"DoubleColorLaser ({i})"));
                break;
            case "KDAEnvironment":
                for (int i = 2; i < 5; i++)
                    _doubleColorLasers.AddIfNotNull(_envRoot.FindObject($"Laser ({i})"));
                for (int i = 7; i < 10; i++)
                    _doubleColorLasers.AddIfNotNull(_envRoot.FindObject($"Laser ({i})"));
                break;
            case "MonstercatEnvironment":
            case "CrabRaveEnvironment":
                for (int i = 4; i < 13; i++)
                    _doubleColorLasers.AddIfNotNull(_envRoot.FindObject($"Laser ({i})"));
                break;
            case "BillieEnvironment":
                _doubleColorLasers.AddIfNotNull(_envRoot.FindObject("BottomPairLasers"));
                for (int i = 1; i < 9; i++)
                    _doubleColorLasers.AddIfNotNull(_envRoot.FindObject($"BottomPairLasers ({i})"));
                break;
            case "TheSecondEnvironment":
                _doubleColorLasers.AddIfNotNull(_envRoot.FindObject("Main Lasers Top"));
                _doubleColorLasers.AddIfNotNull(_envRoot.FindObject("Main Lasers Bottom"));
                break;
        }
    }
    
    private void FindBackLasers()
    {
        switch (_envName)
        {
            case "PanicEnvironment":
                _backLasers.AddIfNotNull(_envRoot.FindObject("Window")?.AppendToName("renamed"));
                _backLasers.AddIfNotNull(_envRoot.FindObject("Window"));
                break;
            case "RocketEnvironment":
                _backLasers.AddIfNotNull(_envRoot.FindObject("FrontLights"));
                _backLasers.AddIfNotNull(_envRoot.FindObject("RocketGateLight"));
                _backLasers.AddIfNotNull(_envRoot.FindObject("GateLight0"));
                _backLasers.AddIfNotNull(_envRoot.FindObject("GateLight1"));
                _backLasers.AddIfNotNull(_envRoot.FindObject("GateLight1 (4)"));
                break;
            case "GreenDayEnvironment":
            case "GreenDayGrenadeEnvironment":
                _backLasers.AddIfNotNull(_envRoot.FindObject("Logo"));
                _backLasers.AddIfNotNull(_envRoot.FindObject("FrontLight"));
                break;
            case "TimbalandEnvironment":
                _backLasers.AddIfNotNull(_envRoot.FindObject("FrontLights"));
                for (int i = 4; i < 8; i++)
                    _backLasers.AddIfNotNull(_envRoot.FindObject($"Light ({i})"));
                break;
            case "LinkinParkEnvironment":
                _backLasers.AddIfNotNull(_envRoot.FindObject("Logo"));
                _backLasers.AddIfNotNull(_envRoot.FindObject("LogoLight"));
                break;
            case "BTSEnvironment":
                _backLasers.AddIfNotNull(_envRoot.FindObject("MagicDoorSprite"));
                break;
            case "SkrillexEnvironment":
                _backLasers.AddIfNotNull(_envRoot.FindObject("SkrillexLogo"));
                _backLasers.AddIfNotNull(_envRoot.FindObject("SkrillexLogo (1)"));
                break;
            case "BillieEnvironment":
                _backLasers.AddIfNotNull(_envRoot.FindObject("DayAndNight/Day"));
                _backLasers.AddIfNotNull(_envRoot.FindObject("DayAndNight/Night"));
                break;
            case "HalloweenEnvironment":
                _backLasers.AddIfNotNull(_envRoot.FindObject("Moon"));
                _backLasers.AddIfNotNull(_envRoot.FindObject("GateLight"));
                break;
            case "GagaEnvironment":
                _backLasers.AddIfNotNull(_envRoot.FindObject("FrontLasers"));
                break;
            case "PyroEnvironment":
                _backLasers.AddIfNotNull(_envRoot.FindObject("PyroLogo"));
                break;
            case "TheSecondEnvironment":
                _backLasers.AddIfNotNull(_envRoot.FindObject("FrontLogo"));
                break;
            default:
                _backLasers.AddIfNotNull(_envRoot.FindObject("FrontLights"));
                break;
        }
    }
    
    private void FindTrackLights()
    {
        switch (_envName)
        {
            case "GlassDesertEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("TopLaser"));
                for (int i = 1; i < 6; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"TopLaser ({i})"));
                for (int i = 4; i < 13; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"DownLaser ({i})"));
                for (int i = 0; i < 7; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject("TopLightMesh")?.AppendToName("renamed"));
                break;
            case "TutorialEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLines"));
                break;
            case "DefaultEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTubeL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTubeR"));
                break;
            case "OriginsEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTube"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTube (1)"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("LightAreaL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("LightAreaR"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("SidePSL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("SidePSR"));
                break;
            case "TriangleEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTubeDirectionalL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTubeDirectionalR"));
                break;
            case "NiceEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineR"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineFarL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineFarR"));
                break;
            case "BigMirrorEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTubeDirectionalL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTubeDirectionalR"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTubeDirectionalFL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTubeDirectionalFR"));
                break;
            case "DragonsEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineR"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("ConstructionGlowLine (1)"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("ConstructionGlowLine (4)"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("ConstructionGlowLine (5)"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("ConstructionGlowLine (6)"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("DragonsSidePSL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("DragonsSidePSR"));
                break;
            case "KDAEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineLVisible"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineRVisible"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowTopLine"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineFarL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineFarR"));
                for (int i = 1; i < 5; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"GlowTopLine ({i})"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLine"));
                for (int i = 1; i < 77; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"GlowLine ({i})"));
                break;
            case "MonstercatEnvironment":
            case "CrabRaveEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineL (1)"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineR"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineR (1)"));
                for (int i = 5; i < 12; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"GlowTopLine ({i})"));
                break;
            case "PanicEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("Light (5)"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("ConstructionGlowLine (15)"));
                for (int i = 4; i < 9; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"ConstructionGlowLine ({i})"));
                break;
            case "RocketEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineR (1)"));
                for (int i = 1; i < 10; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"GlowLineL ({i})"));
                break;
            case "GreenDayEnvironment":
            case "GreenDayGrenadeEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineR"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineL (2)"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineL (4)"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineL (7)"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineL (8)"));
                for (int i = 13; i < 25; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"GlowLineL ({i})"));
                break;
            case "TimbalandEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineR"));
                break;
            case "LinkinParkEnvironment":
                for (int i = 2; i < 5; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"LaserFloor ({i})"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("LaserTop"));
                for (int i = 1; i < 8; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"LaserTop ({i})"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("LaserL"));
                // ReSharper disable once StringLiteralTypo
                _trackLights.AddIfNotNull(_envRoot.FindObject("LarerR"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("LaserL (2)"));
                // ReSharper disable once StringLiteralTypo
                _trackLights.AddIfNotNull(_envRoot.FindObject("LarerR (2)"));
                break;
            case "BTSEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineR"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineH"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineH (2)"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("LaserL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("LaserR"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("GlowLineC"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("BottomGlow"));
                for (int i = 0; i < 4; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject("SideLaser")?.AppendToName("renamed"));
                break;
            case "SkrillexEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("LeftLaser"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("RightLaser"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonSide"));
                for (int i = 1; i < 18; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"NeonSide ({i})"));
                break;
            case "InterscopeEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTop"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("Pillar/NeonLightL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("Pillar/NeonLightR"));
                for (int i = 1; i < 5; i++)
                {
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"NeonTop ({i})"));
                    string baseName = $"Pillar ({i})";
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"{baseName}/NeonLightL"));
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"{baseName}/NeonLightR"));
                }
    
                break;
            case "BillieEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("LightRailingSegment"));
                for (int i = 1; i < 4; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"LightRailingSegment ({i})"));
                break;
            case "HalloweenEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTubeL"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTubeL (1)"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTubeR"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTubeR (1)"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("NeonTubeC"));
                for (int i = 6; i < 10; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"GlowLineL ({i})"));
                break;
            case "GagaEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("Aurora"));
                break;
            case "WeaveEnvironment":
                for (int i = 0; i < 16; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"LightGroup{i}"));
                break;
            case "PyroEnvironment":
                _highway.AddIfNotNull(_envRoot.FindObject("Behind"));
                _highway.AddIfNotNull(_envRoot.FindObject("Video"));
                _highway.AddIfNotNull(_envRoot.FindObject("MainLasers"));
                _highway.AddIfNotNull(_envRoot.FindObject("Stairs"));
                _highway.AddIfNotNull(_envRoot.FindObject("MainStageSetup"));
                _highway.AddIfNotNull(_envRoot.FindObject("LightBoxesScaffoldingLeft"));
                _highway.AddIfNotNull(_envRoot.FindObject("LightBoxesScaffoldingRight"));
                break;
            case "EDMEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("CloseCircle"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("DistantCircle1"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("DistantCircle2"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("Laser"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("TopCircle"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("SingleSourceCircularLY"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("SingleSourceCircularRY"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("SingleSourceLaserUp"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("SingleSourceLaserLeftMid"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("SingleSourceLaserDown"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("SingleSourceLaserUp"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("SingleSourceLaserRightMid"));
                _trackLights.AddIfNotNull(_envRoot.FindObject("SingleSourceLaserDown"));
                for (int i = 1; i < 5; i++)
                    _trackLights.AddIfNotNull(_envRoot.FindObject($"Laser ({i})"));
                break;
            case "TheSecondEnvironment":
                _trackLights.AddIfNotNull(_envRoot.FindObject("RunwayLasers"));
                break;
        }
    }

    private static bool EnvironmentHasTrackLaneRingsManager(string environmentName) => environmentName
        is "DefaultEnvironment"
        or "NiceEnvironment"
        or "MonstercatEnvironment"
        or "CrabRaveEnvironment"
        or "TriangleEnvironment"
        or "BigMirrorEnvironment"
        or "DragonsEnvironment"
        or "OriginsEnvironment"
        or "PanicEnvironment"
        or "GreenDayEnvironment"
        or "TimbalandEnvironment"
        or "FitBeatEnvironment"
        or "KaleidoscopeEnvironment"
        or "SkrillexEnvironment";
}