﻿using CustomFloorPlugin.PlatformManagement;
using CustomFloorPlugin.UI;

using JetBrains.Annotations;

using Zenject;


namespace CustomFloorPlugin.Installers;

[UsedImplicitly]
internal class MenuInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<MenuEnvironmentGrabber>().AsSingle();
        Container.BindInterfacesTo<MenuPlatformSpawner>().AsSingle();
        Container.BindInterfacesTo<MenuEnvironmentHider>().AsSingle();
            
        Container.BindInterfacesTo<LobbyPlatformController>().AsSingle();

        Container.BindInterfacesTo<PlatformFileUpdater>().AsSingle();
        Container.BindInterfacesTo<ApiPlatformManager>().AsSingle();
        
        Container.Bind<PlatformListsViewController>().FromNewComponentAsViewController().AsSingle();
        Container.Bind<PlatformsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
        Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
    }
}