﻿using CustomFloorPlugin.Helpers;

using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class PlatformsGameInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (Container.HasBinding<GameplayCoreSceneSetupData>())
            {
                GameplayCoreSceneSetupData sceneSetupData = Container.Resolve<GameplayCoreSceneSetupData>();
                float lastNoteTime = sceneSetupData.difficultyBeatmap.beatmapData.GetLastNoteTime();
                Container.BindInterfacesAndSelfTo<BSEvents>().AsSingle().WithArguments(lastNoteTime);
            }
            if (Container.HasBinding<MultiplayerPlayersManager>())
            {
                Container.BindInterfacesTo<MultiplayerGameManager>().AsSingle();
            }
        }
    }
}