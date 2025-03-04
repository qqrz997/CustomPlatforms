using JetBrains.Annotations;

using Zenject;


namespace CustomFloorPlugin.Installers;

[UsedImplicitly]
internal class PlayerInstaller : Installer
{
    private readonly ObstacleSaberSparkleEffectManager? _obstacleSaberSparkleEffectManager;

    public PlayerInstaller(
        [InjectOptional] PlayerSpaceConvertor? playerSpaceConvertor,
        [InjectOptional] MultiplayerPlayersManager? multiplayerPlayersManager)
    {
        if (playerSpaceConvertor != null)
            _obstacleSaberSparkleEffectManager = playerSpaceConvertor.GetComponentInChildren<ObstacleSaberSparkleEffectManager>();
    }

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<BSEvents>().AsSingle();
        if (_obstacleSaberSparkleEffectManager != null && !Container.HasBinding<ObstacleSaberSparkleEffectManager>())
            Container.BindInstance(_obstacleSaberSparkleEffectManager).AsSingle();
        // if (_multiplayerPlayersManager != null)
        //     _multiplayerPlayersManager.playerSpawningDidFinishEvent += OnPlayerSpawningDidFinish;
    }

    // private void OnPlayerSpawningDidFinish()
    // {
    //     _multiplayerPlayersManager!.playerSpawningDidFinishEvent -= OnPlayerSpawningDidFinish;
    //     _platformSpawner.OnTransitionDidFinish(GameScenesManager.SceneTransitionType.None, // not used
    //                                            Container.Resolve<MultiplayerLevelScenesTransitionSetupDataSO>(),
    //                                            Container);
    // }
}