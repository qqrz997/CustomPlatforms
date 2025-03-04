using System.Threading.Tasks;
using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Models;

namespace CustomFloorPlugin.PlatformManagement;

public sealed class MenuEnvironmentHider : IEnvironmentHider
{
    private readonly PlatformManager _platformManager;
    private readonly IEnvironmentObjectSource _environmentObjectSource;

    public MenuEnvironmentHider(
        PlatformManager platformManager,
        IEnvironmentObjectSource environmentObjectSource)
    {
        _platformManager = platformManager;
        _environmentObjectSource = environmentObjectSource;
    }

    public async Task HideObjectsForPlatform(CustomPlatform platform)
    {
        await _environmentObjectSource.InitializationTask;
        
        _environmentObjectSource.AlwaysHide.SetActive(platform == _platformManager.DefaultPlatform);
        // bool showPlayersPlace = _envName == "MainMenu" && !platform.hideDefaultPlatform && platform != _platformManager.DefaultPlatform;
        // _assetLoader.PlayersPlace.SetActive(showPlayersPlace);
    }
}