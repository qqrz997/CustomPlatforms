namespace CustomFloorPlugin.Helpers;

internal static class BeatmapHelpers
{
    public static bool HasRotationEvents(this ScenesTransitionSetupDataSO setupData) =>
        setupData is StandardLevelScenesTransitionSetupDataSO levelSetupData && levelSetupData.HasRotationEvents();
    
    public static bool HasRotationEvents(this StandardLevelScenesTransitionSetupDataSO setupData) =>
        setupData.beatmapKey.beatmapCharacteristic.containsRotationEvents;
}