using System;
using System.Threading.Tasks;
using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Models;
using Zenject;

namespace CustomFloorPlugin.PlatformManagement;

public sealed class StandardGameplayPlatformSpawner : IPlatformSpawner, IInitializable, IDisposable
{
    private readonly PlatformManager _platformManager;
    private readonly DiContainer _container;
    private readonly IEnvironmentHider _environmentHider;
    private readonly ScenesTransitionSetupDataSO _scenesTransitionSetupData;
    
    public StandardGameplayPlatformSpawner(
        PlatformManager platformManager,
        DiContainer container,
        IEnvironmentHider environmentHider,
        StandardLevelScenesTransitionSetupDataSO scenesTransitionSetupData)
    {
        _platformManager = platformManager;
        _container = container;
        _environmentHider = environmentHider;
        _scenesTransitionSetupData = scenesTransitionSetupData;
    }

    public async void Initialize()
    {
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