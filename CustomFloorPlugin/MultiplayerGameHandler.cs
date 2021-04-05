﻿using System;
using System.Threading.Tasks;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    internal class MultiplayerGameHandler : IInitializable, IDisposable
    {
        private readonly AssetLoader _assetLoader;
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;
        private readonly MultiplayerPlayersManager _multiplayerPlayersManager;
        private readonly DiContainer _container;

        public MultiplayerGameHandler(AssetLoader assetLoader,
                                     PlatformManager platformManager,
                                     PlatformSpawner platformSpawner,
                                     MultiplayerPlayersManager multiplayerPlayersManager,
                                     DiContainer container)
        {
            _assetLoader = assetLoader;
            _platformManager = platformManager;
            _platformSpawner = platformSpawner;
            _multiplayerPlayersManager = multiplayerPlayersManager;
            _container = container;
        }

        public async void Initialize()
        {
            int platformIndex = _platformManager.GetIndexForType(PlatformType.Multiplayer);
            if (platformIndex != 0)
            {
                _multiplayerPlayersManager.playerDidFinishEvent += HandlePlayerDidFinish;
                await SpawnLightEffects();
                await _platformSpawner.SetContainerAndShowAsync(platformIndex, _container);
            }
        }

        public void Dispose()
        {
            _multiplayerPlayersManager.playerDidFinishEvent -= HandlePlayerDidFinish;
        }

        private async void HandlePlayerDidFinish(LevelCompletionResults results)
        {
            await _platformSpawner.ChangeToPlatformAsync(0);
        }

        /// <summary>
        /// Instantiates the light effects prefab for multiplayer levels
        /// </summary>
        private async Task SpawnLightEffects()
        {
            await _assetLoader.loadAssetsTask;
            GameObject lightEffects = _container.InstantiatePrefab(_assetLoader.lightEffects);
            _platformManager.spawnedObjects.Add(lightEffects);
            lightEffects.SetActive(true);
        }
    }
}