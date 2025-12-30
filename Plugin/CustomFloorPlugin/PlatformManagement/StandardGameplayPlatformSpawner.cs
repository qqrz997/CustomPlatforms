using System;
using System.Threading.Tasks;
using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Models;
using SiraUtil.Logging;
using Zenject;

namespace CustomFloorPlugin.PlatformManagement;

public sealed class StandardGameplayPlatformSpawner : IPlatformSpawner, IInitializable, IDisposable
{
    private readonly PlatformManager _platformManager;
    private readonly DiContainer _container;
    private readonly IEnvironmentHider _environmentHider;
    private readonly StandardLevelScenesTransitionSetupDataSO _scenesTransitionSetupData;

    // private readonly SiraLog logger;
    
    public StandardGameplayPlatformSpawner(
        PlatformManager platformManager,
        DiContainer container,
        IEnvironmentHider environmentHider,
        StandardLevelScenesTransitionSetupDataSO scenesTransitionSetupData, 
        SiraLog logger)
    {
        _platformManager = platformManager;
        _container = container;
        _environmentHider = environmentHider;
        _scenesTransitionSetupData = scenesTransitionSetupData;
        // this.logger = logger;
    }

    public async void Initialize()
    {
        // todo: we cannot re-enable the default env when another mod, like chroma, is in charge of the env
        // var isNoodle = _scenesTransitionSetupData.beatmapKey.RequiresNoodleExtensions();
        
        var currentPlatform = _platformManager.APIRequestedPlatform != null ? _platformManager.APIRequestedPlatform
            : _scenesTransitionSetupData.HasRotationEvents() ? _platformManager.A360Platform
            : _platformManager.SingleplayerPlatform;
        
        await SpawnPlatform(currentPlatform);
    }

    public async void Dispose()
    {
        // Return to the menu platform when the level ends
        await SpawnPlatform(_platformManager.MenuPlatform);
    }

    public async Task SpawnPlatform(CustomPlatform customPlatform)
    {
        var spawnedPlatform = await _platformManager.SpawnPlatform(customPlatform, _container);
        await _environmentHider.HideObjectsForPlatform(spawnedPlatform);
    }
}