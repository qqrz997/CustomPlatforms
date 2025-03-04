﻿using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Installers;

using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using IPA.Logging;

using JetBrains.Annotations;

using SiraUtil.Zenject;


namespace CustomFloorPlugin;

/// <summary>
/// Main Plugin executable, loaded and instantiated by BSIPA before the game starts<br/>
/// Different callbacks will be notified throughout the games lifespan, and can be used as hooks.
/// </summary>
[UsedImplicitly]
[NoEnableDisable]
[Plugin(RuntimeOptions.DynamicInit)]
public class Plugin
{
    /// <summary>
    /// Initializes the Plugin
    /// </summary>
    /// <param name="pluginMetadata">This plugin's <see cref="PluginMetadata"/></param>
    /// <param name="logger">The instance of the IPA logger that BSIPA hands to plugins on initialization</param>
    /// <param name="config">The config BSIPA provides</param>
    /// <param name="zenjector">The zenjector that SiraUtil passes to this plugin</param>
    [Init]
    public Plugin(PluginMetadata pluginMetadata, Logger logger, Config config, Zenjector zenjector)
    {
        zenjector.UseLogger(logger);
        zenjector.UseHttpService();
        zenjector.Install<AppInstaller>(Location.App, pluginMetadata.Assembly, config.Generated<PluginConfig>());
        zenjector.Install<MenuInstaller>(Location.Menu);
        zenjector.Install<PlayerInstaller>(Location.Player);
        zenjector.Install<StandardPlayerInstaller>(Location.StandardPlayer);
        zenjector.Install<MultiPlayerInstaller>(Location.MultiPlayer);
    }
}