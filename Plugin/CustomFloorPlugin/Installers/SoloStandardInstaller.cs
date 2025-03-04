using CustomFloorPlugin.PlatformManagement;
using Zenject;

namespace CustomFloorPlugin.Installers;

internal class SoloStandardInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<StandardGameplayPlatformSpawner>().AsSingle();
        Container.BindInterfacesTo<StandardGameplayEnvironmentHider>().AsSingle();
        Container.BindInterfacesTo<StandardGameplayEnvironmentGrabber>().AsSingle();
    }
}