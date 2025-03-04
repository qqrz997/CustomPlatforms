using System.Threading.Tasks;
using CustomFloorPlugin.Models;
using Zenject;

namespace CustomFloorPlugin.PlatformManagement;

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