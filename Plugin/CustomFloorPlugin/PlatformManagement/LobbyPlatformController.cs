using System;
using CustomFloorPlugin.Models;
using Zenject;

namespace CustomFloorPlugin.PlatformManagement;

/// <summary>
/// Responsible for swapping out the current menu custom platform when joining a lobby, as we do not want to use a
/// custom platform in a multiplayer lobby.
/// </summary>
public sealed class LobbyPlatformController : IInitializable, IDisposable
{
    private readonly GameServerLobbyFlowCoordinator _lobbyFlowCoordinator;
    private readonly PlatformManager _platformManager;
    private readonly IPlatformSpawner _platformSpawner;

    public LobbyPlatformController(
        GameServerLobbyFlowCoordinator lobbyFlowCoordinator,
        PlatformManager platformManager,
        IPlatformSpawner platformSpawner) 
    {
        _lobbyFlowCoordinator = lobbyFlowCoordinator;
        _platformSpawner = platformSpawner;
        _platformManager = platformManager;
    }

    public void Initialize()
    {
        _lobbyFlowCoordinator.didSetupEvent += LobbyFlowCoordinatorDidSetup;
        _lobbyFlowCoordinator.didFinishEvent += LobbyFlowCoordinatorDidFinish;
    }

    public void Dispose()
    {
        _lobbyFlowCoordinator.didSetupEvent -= LobbyFlowCoordinatorDidSetup;
        _lobbyFlowCoordinator.didFinishEvent -= LobbyFlowCoordinatorDidFinish;
    }

    private void LobbyFlowCoordinatorDidSetup()
    {
        _platformSpawner.SpawnPlatform(_platformManager.DefaultPlatform);
    }

    private void LobbyFlowCoordinatorDidFinish()
    {
        _platformSpawner.SpawnPlatform(_platformManager.MenuPlatform);
    }
}