> [!NOTE]
> Some platforms have shaders that are compiled for the old unity version used prior to Beat Saber 1.29.4, and may not render properly at all. However, a wide range of platforms are still completely functional and it is up to their creators to update the shaders they used if their models are broken.

To read more about migrating platforms to the newer versions of Beat Saber, read [this wiki page](https://bsmg.wiki/models/shader-migration.html).

# Custom Platforms
This is a fork of the Custom Platforms plugin for Beat Saber with the plan of continued maintenance as the game recieves updates.

## Manual Installation
> [!IMPORTANT]
> In addition to BSIPA, you must have [AssetBundleLoadingTools](https://github.com/nicoco007/AssetBundleLoadingTools), [SiraUtil](https://github.com/Auros/SiraUtil), and [BeatSaberMarkupLanguage](https://github.com/monkeymanboy/BeatSaberMarkupLanguage) installed for this mod to load. Install them using your mod manager i.e. [BSManager](https://bsmg.wiki/pc-modding.html#bsmanager).

Place the contents of the unzipped folder from the latest [release](https://github.com/qqrz997/CustomSabersLite/releases/latest) into your Beat Saber installation folder. If you need more information regarding manual installation of mods [this wiki page](https://bsmg.wiki/pc-modding.html#manual-installation) will help. For further help with installing mods, join the [Beat Saber Modding Group](https://discord.gg/beatsabermods) discord server.

Older versions of CustomPlatforms are not supported. If you find issues using an older version then I won't be able to help.

- After launching Beat Saber with the mod successfully installed, a `CustomPlatforms` folder will be created in your Beat Saber game folder. Plat files should be placed here.
- You can get more platforms from a model downloader such as [ModelMenu](https://github.com/qqrz997/ModelMenu) or BSManager, or you can visit [ModelSaber](https://modelsaber.com/Platforms/?pc) which supports One-Click installs.

After importing platforms, restart the game and you will see them in the Custom Platforms menu, accessed through a button in the mods section of the main menu.

## Creating New Platforms

There's a comprehensive guide at https://bsmg.wiki/models/platforms-guide.html written by Emma.
The following are the basic steps:

1. Download the Unity project from [Releases](https://github.com/affederaffe/CustomPlatforms/releases/latest), unzip it.

2. Open the Unity project
The project was created and tested in version [2019.3.15f1](https://unity3d.com/get-unity/download/archive), other versions may not be supported.

3. Create an empty GameObject and attach a "Custom Platform" component to it.
Fill out the fields for your name and the name of the platform.  You can also toggle the visibility of default environment parts if you need to make room for your platform.
Add an icon for your platform by importing an image, settings it to Sprite/UI in import settings, and dragging it into the icon field of your CustomPlatform

4. Create your custom platform as a child of this root object
You can use most of the built-in Unity components, custom shaders and materials, custom meshes, animators, etc.

1. When you are finished, select the root object you attached the "Custom Platform" component to.
In the inspector, click "Export". Navigate to your CustomPlatforms folder, and press save.

6. Share your custom platform with other players by uploading the Platforms' .plat file

## Building (Outdated but still functional)
1. Clone the repository with ```git clone https://github.com/qqrz997/CustomPlatforms.git```
2. Go to the ```./CustomPlatforms/Plugin/CustomFloorPlugin``` direcory and create a ```CustomFloorPlugin.csproj.user``` file, see the example below
3. Open the solution file in the ```Plugins``` directory with e.g. VisualStudio or Jetbrains Rider and build the project

#### Example csproj.user File:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <BeatSaberDir>Full\Path\To\Beat Saber</BeatSaberDir>
  </PropertyGroup>
</Project>
```

## Hall of Fame (Credits for major rework contributions)
#### AkaRaiden - (The QA Department, Beta Tester, Tome of Wisdom)
  - Without him this would have taken so much more time than it did.

#### Rolo - (The Master Mind, Inventor CustomPlatforms)
  - Went out of her way to help me clean up after six people messed with this.

#### Panics - (Chief Investigator)
  - Helped me get an initial grasp on the damage.

#### Tiruialon - (Top-Cat)
  - Thank you for your contributions!
 
#### boulders2000 - (Bug Hunter)
  - Stopped counting how many bugreports he sent.
