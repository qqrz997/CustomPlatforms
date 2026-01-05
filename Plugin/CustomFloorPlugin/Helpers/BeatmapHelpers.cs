using System.Collections.Generic;
using CustomFloorPlugin.PlatformManagement;
using SongCore;

namespace CustomFloorPlugin.Helpers;

internal static class BeatmapHelpers
{
    public static bool HasRotationEvents(this StandardLevelScenesTransitionSetupDataSO setupData) =>
        setupData.beatmapKey.beatmapCharacteristic.containsRotationEvents;
    
    public static bool RequiresAny(this BeatmapKey beatmapKey, params IEnumerable<string> requirementNames) => 
        InstalledMods.SongCore && beatmapKey.MapHasAnyRequirement(requirementNames);
    
    /// <summary>
    /// This method depends on SongCore. Check it is installed before calling this method.
    /// </summary>
    private static bool MapHasAnyRequirement(this BeatmapKey beatmapKey, IEnumerable<string> requirementNames)
    {
        var requirementData = Collections.GetCustomLevelSongDifficultyData(beatmapKey)?.additionalDifficultyData;
        if (requirementData == null)  return false;
        foreach (var requirementName in requirementNames)
        {
            foreach (var suggestion in requirementData._requirements)
                if (requirementName == suggestion) return true;
        }
        return false;
    }
}