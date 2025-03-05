using System.Threading.Tasks;
using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Models;
using SiraUtil.Logging;

namespace CustomFloorPlugin.PlatformManagement;

public sealed class StandardGameplayEnvironmentHider : IEnvironmentHider
{
    private readonly IEnvironmentObjectSource _environmentObjectSource;
    private readonly GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
    private readonly AssetLoader _assetLoader;
    private readonly SiraLog _siraLog;

    public StandardGameplayEnvironmentHider(
        IEnvironmentObjectSource environmentObjectSource,
        GameplayCoreSceneSetupData gameplayCoreSceneSetupData,
        AssetLoader assetLoader,
        SiraLog siraLog)
    {
        _environmentObjectSource = environmentObjectSource;
        _gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
        _siraLog = siraLog;
        _assetLoader = assetLoader;
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
        
        _assetLoader.PlayersPlace.SetActive(false);
    }
}