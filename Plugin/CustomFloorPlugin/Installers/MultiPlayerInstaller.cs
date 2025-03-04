using CustomFloorPlugin.PlatformManagement;
using Zenject;

namespace CustomFloorPlugin.Installers;

internal class MultiPlayerInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<MultiplayerPlatformSpawner>().AsSingle();
        Container.BindInterfacesTo<MultiplayerEnvironmentHider>().AsSingle();
        Container.BindInterfacesTo<MultiplayerEnvironmentGrabber>().AsSingle();
    }
}