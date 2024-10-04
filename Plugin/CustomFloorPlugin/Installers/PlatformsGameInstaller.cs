using JetBrains.Annotations;

using Zenject;


namespace CustomFloorPlugin.Installers
{
    [UsedImplicitly]
    internal class PlatformsGameInstaller : Installer
    {
        private readonly PlatformSpawner _platformSpawner;
        private readonly ObstacleSaberSparkleEffectManager? _obstacleSaberSparkleEffectManager;
        private readonly MultiplayerPlayersManager? _multiplayerPlayersManager;

        public PlatformsGameInstaller(PlatformSpawner platformSpawner, [InjectOptional] PlayerSpaceConvertor? playerSpaceConvertor, [InjectOptional] MultiplayerPlayersManager? multiplayerPlayersManager)
        {
            _platformSpawner = platformSpawner;
            _multiplayerPlayersManager = multiplayerPlayersManager;
            if (playerSpaceConvertor != null)
                _obstacleSaberSparkleEffectManager = playerSpaceConvertor.GetComponentInChildren<ObstacleSaberSparkleEffectManager>();
        }

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BSEvents>().AsSingle();
            if (_obstacleSaberSparkleEffectManager != null && !Container.HasBinding<ObstacleSaberSparkleEffectManager>())
                Container.BindInstance(_obstacleSaberSparkleEffectManager).AsSingle();
            if (_multiplayerPlayersManager != null)
                _multiplayerPlayersManager.playerSpawningDidFinishEvent += OnPlayerSpawningDidFinish;
        }

        private void OnPlayerSpawningDidFinish()
        {
            _multiplayerPlayersManager!.playerSpawningDidFinishEvent -= OnPlayerSpawningDidFinish;
            _platformSpawner.OnTransitionDidFinish(GameScenesManager.SceneTransitionType.None, // not used
                                                   Container.Resolve<MultiplayerLevelScenesTransitionSetupDataSO>(),
                                                   Container);
        }
    }
}
