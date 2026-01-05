using System;
using System.Threading.Tasks;
using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Models;
using Zenject;

namespace CustomFloorPlugin.PlatformManagement;

public sealed class StandardGameplayPlatformSpawner : IPlatformSpawner, IInitializable, IDisposable
{
    private readonly PlatformManager _platformManager;
    private readonly DiContainer _container;
    private readonly IEnvironmentHider _environmentHider;
    private readonly StandardLevelScenesTransitionSetupDataSO _scenesTransitionSetupData;
    private readonly PluginConfig _config;

    public StandardGameplayPlatformSpawner(
        PlatformManager platformManager,
        DiContainer container,
        IEnvironmentHider environmentHider,
        StandardLevelScenesTransitionSetupDataSO scenesTransitionSetupData,
        PluginConfig config)
    {
        _platformManager = platformManager;
        _container = container;
        _environmentHider = environmentHider;
        _scenesTransitionSetupData = scenesTransitionSetupData;
        _config = config;
    }

    public async void Initialize()
    {
        if (_platformManager.APIRequestedPlatform != null)
        {
            await SpawnPlatform(_platformManager.APIRequestedPlatform);
            return;
        }
        
        if (!_config.OverrideIncompatibleRequirements)
        {
            var hasIncompatibleRequirement = _scenesTransitionSetupData.beatmapKey.RequiresAny(
                "Noodle Extensions", "Vivify", "Cinema");

            if (hasIncompatibleRequirement)
            {
                // Calling SpawnPlatform handles this, but we only want to disable the menu platform in this case
                _platformManager.MenuPlatform.Disable();
                return;
            }
        }
        
        var currentPlatform = _scenesTransitionSetupData.HasRotationEvents() ? _platformManager.A360Platform
            : _platformManager.SingleplayerPlatform;
        
        await SpawnPlatform(currentPlatform);
    }

    public async void Dispose()
    {
        // Return to the menu platform when the level ends
        // todo: might want to use an event to hand the responsibility of re-spawning this to MenuPlatformSpawner
        await _platformManager.SpawnPlatform(_platformManager.MenuPlatform, _container);
    }

    public async Task SpawnPlatform(CustomPlatform customPlatform)
    {
        var spawnedPlatform = await _platformManager.SpawnPlatform(customPlatform, _container);
        await _environmentHider.HideObjectsForPlatform(spawnedPlatform);
    }
}