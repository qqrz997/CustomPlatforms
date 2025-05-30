using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Models;
using IPA.Utilities;
using JetBrains.Annotations;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using UnityEngine;
using Zenject;
using Random = System.Random;

namespace CustomFloorPlugin.PlatformManagement;

/// <summary>
/// Handles platform loading and saving
/// </summary>
[UsedImplicitly]
public sealed class PlatformManager : IAsyncInitializable, IDisposable
{
    private readonly SiraLog _siraLog;
    private readonly PluginConfig _config;
    private readonly AssetLoader _assetLoader;
    private readonly PlatformLoader _platformLoader;
    private readonly Random _random;
    private readonly MaterialSwapper _materialSwapper;
    
    private readonly Transform _anchor;
    private readonly string _directoryPath = Path.Combine(UnityGame.InstallPath, "CustomPlatforms");
    private readonly string _cacheFilePath;

    private CancellationTokenSource _spawnPlatformTokenSource = new(); 

    /// <summary>
    /// Getter for the CustomPlatforms directory path
    /// </summary>
    public string DirectoryPath => Directory.CreateDirectory(_directoryPath).FullName;

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of all currently loaded <see cref="CustomPlatform"/>s<br/>
    /// When a platform is loaded, the <see cref="ObservableCollection{T}"/> will be notified
    /// </summary>
    public ObservableCollection<CustomPlatform> AllPlatforms { get; }

    public CustomPlatform DefaultPlatform { get; }
    public CustomPlatform RandomPlatform { get; }
    public CustomPlatform ActivePlatform { get; internal set; }

    public CustomPlatform SingleplayerPlatform { get; internal set; }
    public CustomPlatform MultiplayerPlatform { get; internal set; }
    public CustomPlatform A360Platform { get; internal set; }
    public CustomPlatform MenuPlatform { get; internal set; }
    public CustomPlatform? APIRequestedPlatform { get; internal set; }

    /// <summary>
    /// The cache file version, change this to prevent loading older ones if something changes
    /// </summary>
    private const byte CacheFileVersion = 3;

    internal const int BuildInPlatformsCount = 2;

    public PlatformManager(
        SiraLog siraLog,
        PluginConfig config,
        AssetLoader assetLoader,
        PlatformLoader platformLoader,
        Random random,
        MaterialSwapper materialSwapper,
        [Inject(Id = "CustomPlatforms")] Transform anchor)
    {
        _siraLog = siraLog;
        _config = config;
        _assetLoader = assetLoader;
        _platformLoader = platformLoader;
        _random = random;
        _materialSwapper = materialSwapper;
        _anchor = anchor;
        _cacheFilePath = Path.Combine(DirectoryPath, "cache.dat");
        DefaultPlatform = CreateDefaultPlatform();
        RandomPlatform = CreateRandomPlatform();
        AllPlatforms = [DefaultPlatform, RandomPlatform];
        SingleplayerPlatform = MultiplayerPlatform = A360Platform = MenuPlatform = ActivePlatform = DefaultPlatform;
    }

    public async Task InitializeAsync(CancellationToken token) => await LoadPlatformsAsync(token);

    // Automatically save the descriptors on exit
    public void Dispose() => SavePlatformInfosToFile();
    
    public async Task<CustomPlatform> SpawnPlatform(CustomPlatform platform, DiContainer container)
    {
        if (!_config.Enabled)
        {
            DisablePlatform(ActivePlatform, container);
            return DefaultPlatform;
        }
        
        if (platform == ActivePlatform)
        {
            EnablePlatform(ActivePlatform, container);
            return ActivePlatform;
        }

        if (container.HasBinding<MultiplayerLevelScenesTransitionSetupDataSO>())
        {
            _assetLoader.MultiplayerLightEffects.PlatformDisabled();
        }
        DisablePlatform(ActivePlatform, container);
        
        _spawnPlatformTokenSource.Cancel();
        _spawnPlatformTokenSource.Dispose();
        _spawnPlatformTokenSource = new();
        
        if (platform == RandomPlatform)
        {
            // Pick a random platform
            platform = AllPlatforms.Count <= BuildInPlatformsCount ? DefaultPlatform 
                : AllPlatforms[_random.Next(BuildInPlatformsCount, AllPlatforms.Count)];
        }

        ActivePlatform = platform;
        
        if (platform.isDescriptor)
        {
            // Load the platform from its metadata
            platform = await ReplaceDescriptorAsync(platform);
        }

        if (_spawnPlatformTokenSource.Token.IsCancellationRequested)
        {
            return platform;
        }

        _siraLog.Debug($"Switching to {platform.name}");
        ActivePlatform = platform;

        // Spawn the new platform
        EnablePlatform(platform, container);
        _materialSwapper.ReplaceMaterials(platform.gameObject);
        
        return platform;
    }

    private static void EnablePlatform(CustomPlatform platform, DiContainer container)
    {
        platform.gameObject.SetActive(true);
        foreach (var notifyEnable in platform.GetComponentsInChildren<INotifyPlatformEnabled>(true))
        {
            notifyEnable.PlatformEnabled(container);
        }
    }

    private static void DisablePlatform(CustomPlatform platform, DiContainer container)
    {
        foreach (var notifyDisable in platform.GetComponentsInChildren<INotifyPlatformDisabled>(true))
        {
            notifyDisable.PlatformDisabled();
        }
        platform.gameObject.SetActive(false);
    }

    private async Task<CustomPlatform> ReplaceDescriptorAsync(CustomPlatform descriptor)
    {
        var platform = await CreatePlatformAsync(descriptor.fullPath);
        if (platform == null)
            return DefaultPlatform;
        AllPlatforms.Replace(descriptor, platform);
        UnityEngine.Object.Destroy(descriptor.gameObject);
        return platform;
    }

    /// <summary>
    /// Creates a fake <see cref="CustomPlatform"/> used to indicate that no platform should be used
    /// </summary>
    private CustomPlatform CreateDefaultPlatform()
    {
        CustomPlatform defaultPlatform = new GameObject("Default Platform").AddComponent<CustomPlatform>();
        defaultPlatform.transform.SetParent(_anchor);
        defaultPlatform.platName = defaultPlatform.platHash = "Default Environment";
        defaultPlatform.platAuthor = "Beat Saber";
        defaultPlatform.icon = _assetLoader.DefaultPlatformCover;
        defaultPlatform.isDescriptor = false;
        return defaultPlatform;
    }

    /// <summary>
    /// Creates a stub <see cref="CustomPlatform"/> that indicates that a random platform should be used
    /// </summary>
    private CustomPlatform CreateRandomPlatform()
    {
        CustomPlatform randomPlatform = new GameObject("Random Platform").AddComponent<CustomPlatform>();
        randomPlatform.transform.SetParent(_anchor);
        randomPlatform.platName = randomPlatform.platHash = "Random Platform";
        randomPlatform.platAuthor = "???";
        randomPlatform.icon = _assetLoader.RandomPlatformCover;
        randomPlatform.isDescriptor = false;
        return randomPlatform;
    }

    /// <summary>
    /// Restores the last platform selection
    /// </summary>
    private void LastSelectedPlatform(CustomPlatform platform)
    {
        if (platform.platHash == _config.SingleplayerPlatformHash)
            SingleplayerPlatform = platform;
        if (platform.platHash == _config.MultiplayerPlatformHash)
            MultiplayerPlatform = platform;
        if (platform.platHash == _config.A360PlatformHash)
            A360Platform = platform;
        if (platform.platHash == _config.MenuPlatformHash)
            MenuPlatform = platform;
    }

    /// <summary>
    /// Loads all platforms or their descriptors if a cache file exists
    /// </summary>
    private async Task LoadPlatformsAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            foreach (var platform in EnumeratePlatformDescriptorsFromFile())
                AllPlatforms.AddSorted(BuildInPlatformsCount, AllPlatforms.Count - BuildInPlatformsCount, platform);
        }
        catch (Exception e)
        {
            _siraLog.Debug($"Failed to read cache file:{Environment.NewLine}{e}");
            try
            {
                File.Delete(_cacheFilePath);
            }
            catch (Exception ex)
            {
                _siraLog.Warn($"Failed to delete corrupted cache file:{Environment.NewLine}{ex}");
            }
        }

        // Load all remaining platforms, or all if no cache file is found
        foreach (string path in Directory.EnumerateFiles(DirectoryPath, "*.plat").Where(x => AllPlatforms.All(y => y.fullPath != x)))
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            var platform = await CreatePlatformAsync(path);
            if (platform == null)
                continue;
            AllPlatforms.AddSorted(BuildInPlatformsCount, AllPlatforms.Count - BuildInPlatformsCount, platform);
        }

        stopwatch.Stop();
        _siraLog.Debug($"Loaded {AllPlatforms.Count - BuildInPlatformsCount} platforms in {stopwatch.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Asynchronously creates a <see cref="CustomPlatform"/> by loading an <see cref="AssetBundle"/> from the given <paramref name="fullPath"/>
    /// </summary>
    public async Task<CustomPlatform?> CreatePlatformAsync(string fullPath)
    {
        var platform = await _platformLoader.LoadPlatformFromFileAsync(fullPath);
        if (platform == null)
            return null;
        var newPlatform = UnityEngine.Object.Instantiate(platform, _anchor);
        UnityEngine.Object.Destroy(platform.gameObject);
        newPlatform.name = platform.name;
        newPlatform.isDescriptor = false;
        LastSelectedPlatform(newPlatform);
        return newPlatform;
    }

    /// <summary>
    /// Reads all saved platform descriptors from the cache file
    /// </summary>
    private IEnumerable<CustomPlatform> EnumeratePlatformDescriptorsFromFile()
    {
        using FileStream stream = new(_cacheFilePath, FileMode.Open, FileAccess.Read);
        using BinaryReader reader = new(stream, Encoding.UTF8);
        if (reader.ReadByte() != CacheFileVersion) yield break;
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            var platform = new GameObject().AddComponent<CustomPlatform>();
            platform.platName = reader.ReadString();
            platform.platAuthor = reader.ReadString();
            platform.platHash = reader.ReadString();
            platform.fullPath = reader.ReadString();
            platform.icon = reader.ReadNullableSprite();
            if (!File.Exists(platform.fullPath))
            {
                _siraLog.Debug($"File {platform.fullPath} no longer exists; skipped");
                UnityEngine.Object.Destroy(platform.gameObject);
                continue;
            }

            platform.name = $"{platform.platName} by {platform.platAuthor}";
            platform.transform.SetParent(_anchor);
            LastSelectedPlatform(platform);
            yield return platform;
        }
    }

    /// <summary>
    /// Saves descriptors of all loaded <see cref="CustomPlatform"/>s into a cache file
    /// </summary>
    private void SavePlatformInfosToFile()
    {
        using FileStream stream = new(_cacheFilePath, FileMode.OpenOrCreate, FileAccess.Write);
        using BinaryWriter writer = new(stream, Encoding.UTF8);

        writer.Write(CacheFileVersion);
        writer.Write(AllPlatforms.Count - BuildInPlatformsCount);
        for (int i = BuildInPlatformsCount; i < AllPlatforms.Count; i++)
        {
            writer.Write(AllPlatforms[i].platName);
            writer.Write(AllPlatforms[i].platAuthor);
            writer.Write(AllPlatforms[i].platHash);
            writer.Write(AllPlatforms[i].fullPath);
            writer.WriteNullableSprite(AllPlatforms[i].icon);
        }

        stream.SetLength(stream.Position);
        File.SetAttributes(_cacheFilePath, FileAttributes.Hidden);
    }
}