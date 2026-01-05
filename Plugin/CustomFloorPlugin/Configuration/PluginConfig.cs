using System.Runtime.CompilerServices;

using IPA.Config.Stores;

using JetBrains.Annotations;


[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace CustomFloorPlugin.Configuration;

[UsedImplicitly]
public class PluginConfig
{
    public virtual bool Enabled { get; set; } = true;
    public virtual bool CustomSongPlatforms { get; set; } = true;
    public virtual bool OverrideIncompatibleRequirements { get; set; } = false;
    
    public virtual string? SingleplayerPlatformHash { get; set; }
    public virtual string? MultiplayerPlatformHash { get; set; }
    public virtual string? A360PlatformHash { get; set; }
    public virtual string? MenuPlatformHash { get; set; }
}