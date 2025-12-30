using System.Linq;
using CustomFloorPlugin.PlatformManagement;
using SongCore;

namespace CustomFloorPlugin.Helpers;

internal static class BeatmapHelpers
{
    public static bool HasRotationEvents(this ScenesTransitionSetupDataSO setupData) =>
        setupData is StandardLevelScenesTransitionSetupDataSO levelSetupData && levelSetupData.HasRotationEvents();
    
    public static bool HasRotationEvents(this StandardLevelScenesTransitionSetupDataSO setupData) =>
        setupData.beatmapKey.beatmapCharacteristic.containsRotationEvents;
    
    public static bool RequiresNoodleExtensions(this BeatmapKey beatmapKey) =>
        InstalledMods.SongCore && beatmapKey.MapHasRequirement("Noodle Extensions");
    
    private static bool MapHasRequirement(this BeatmapKey beatmapKey, string requirementName) =>
        Collections.GetCustomLevelSongDifficultyData(beatmapKey)?.additionalDifficultyData._requirements
            .Any(req => req == requirementName) is true;
}