using System.Threading.Tasks;
using CustomFloorPlugin.Models;
using Zenject;

namespace CustomFloorPlugin.PlatformManagement;

public sealed class MenuPlatformSpawner : IPlatformSpawner, IInitializable
{
    private readonly PlatformManager _platformManager;
    private readonly DiContainer _container;
    private readonly IEnvironmentHider _environmentHider;

    public MenuPlatformSpawner(
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
        var spawnedPlatform = await SpawnPlatform(_platformManager.MenuPlatform);
        await _environmentHider.HideObjectsForPlatform(spawnedPlatform);
    }

    public async Task<CustomPlatform> SpawnPlatform(CustomPlatform platform)
    {
        return await _platformManager.SpawnPlatform(platform, _container);
    }
}