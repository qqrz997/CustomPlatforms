using CustomFloorPlugin.Models;
using Zenject;

namespace CustomFloorPlugin.Helpers;

public static class CustomPlatformExtensions
{
    public static void Enable(this CustomPlatform platform, DiContainer container)
    {
        platform.gameObject.SetActive(true);
        foreach (var notifyEnable in platform.GetComponentsInChildren<INotifyPlatformEnabled>(true))
        {
            notifyEnable.PlatformEnabled(container);
        }
    }

    public static void Disable(this CustomPlatform platform)
    {
        foreach (var notifyDisable in platform.GetComponentsInChildren<INotifyPlatformDisabled>(true))
        {
            notifyDisable.PlatformDisabled();
        }
        platform.gameObject.SetActive(false);
    }
}