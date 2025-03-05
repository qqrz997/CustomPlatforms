using System.Threading.Tasks;
using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Models;

namespace CustomFloorPlugin.PlatformManagement;

public sealed class MenuEnvironmentHider : IEnvironmentHider
{
    private readonly PlatformManager _platformManager;
    private readonly IEnvironmentObjectSource _environmentObjectSource;
    private readonly AssetLoader _assetLoader;

    public MenuEnvironmentHider(
        PlatformManager platformManager,
        IEnvironmentObjectSource environmentObjectSource,
        AssetLoader assetLoader)
    {
        _platformManager = platformManager;
        _environmentObjectSource = environmentObjectSource;
        _assetLoader = assetLoader;
    }

    public async Task HideObjectsForPlatform(CustomPlatform platform)
    {
        await _environmentObjectSource.InitializationTask;
        
        // Show the environment only when there is no custom platform
        bool isCustomPlatform = _platformManager.DefaultPlatform != platform;
        _environmentObjectSource.AlwaysHide.SetActive(!isCustomPlatform);
        
        _assetLoader.PlayersPlace.SetActive(isCustomPlatform && !platform.hideDefaultPlatform);
    }
}