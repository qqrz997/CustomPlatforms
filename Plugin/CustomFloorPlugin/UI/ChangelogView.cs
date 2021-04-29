﻿using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

using HMUI;


namespace CustomFloorPlugin.UI
{
    /// <summary>
    /// A <see cref="BSMLAutomaticViewController"/> generated by Zenject and maintained by BSML at runtime.<br/>
    /// BSML uses the <see cref="ViewDefinitionAttribute"/> to determine the Layout of the GameObjects and their Components<br/>
    /// Tagged functions and variables from this class may be used/called by BSML if the .bsml file mentions them.<br/>
    /// </summary>
    [ViewDefinition("CustomFloorPlugin.Views.Changelog.bsml")]
    [HotReload(RelativePathToLayout = "CustomFloorPlugin/Views/Changelog.bsml")]
    internal class ChangelogView : BSMLAutomaticViewController
    {
        [UIComponent("credits-modal")]
        private readonly ModalView? _creditsModal = null;

        /// <summary>
        /// The string displayed in the changelog   
        /// </summary>
        [UIValue("changelog-text")]
        public static string Changelog =>
        $@"  <size=150%><color=#888888>Version 6.1.3</color></size>
        - Updated for 1.15.0
        - Reduced startup times

  <size=150%><color=#888888>Version 6.1.2</color></size>
        - Updated for 1.14.0
        - Internal code 'cleanup'
        - Added 'Shuffle Platforms' option
         - Added manual config option to change your CustomPlatforms directory
        - New feature: nested components
        - Fixed issue with Chroma's environment removal

  <size=150%><color=#888888>Version 6.1.1</color></size>
        - Security fix: removed embedded CustomScripts

  <size=150%><color=#888888>Version 6.1.0</color></size>
        - Many new features for platform makers
        - Further reduced loading time
        - Performance improvements
        - Fixed a bug where the credits environment wouldn't be hidden

  <size=150%><color=#888888>Version 6.0.0</color></size>
        - Basically a complete rewrite of the plugin and updated for 1.13.2
        - Platforms can now be used in the menu
         - Redesigned menu, moved settings section here and added a changelog
        - Removed environment override setting because it's implemented in 
           the base game itself
        - Added light effects in multiplayer
        - Removed BS Utils as a dependency
        - Simplified custom scripts
        - You can now select platforms for multi-, singleplayer and 
           360°-levels separately
        - Revived the API, mappers can specify a platform for their songs again
         - Compatibility with Cinema: When a video is played, the default platform 
           will be used, if not configured otherwise
        - Platforms installed while the game is running 
           are automatically loaded now
        - Also platforms are now loaded lazily, that means the startup time
           decreased drastically
        - Fixed so many bugs I can't even count them";

        /// <summary>
        /// The string displayed in the credits
        /// </summary>
        [UIValue("credits-text")]
        public static string Credits =>
        @"<size=150%><color=#888888><align=center>Credits</align></color></size>
        boulders2000: This guy is just awesome
        Kyle 1413: Helped me rewrite the API
        Dakari: Compatibility with Cinema
        AkaRaiden: Features for platform makers
        Nicolas: Faster platform loading
<size=125%><align=center>CustomPlatforms developed by affederaffe</align></size>";

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            _creditsModal!.gameObject.SetActive(false);
        }
    }
}