using IPA.Loader;

namespace CustomFloorPlugin.PlatformManagement;

internal static class InstalledMods
{
    public static bool SongCore { get; } = IsInstalled("SongCore");
    public static bool Cinema { get; } = IsInstalled("Cinema");

    private static bool IsInstalled(string modId) => PluginManager.GetPluginFromId(modId) is not null;
}