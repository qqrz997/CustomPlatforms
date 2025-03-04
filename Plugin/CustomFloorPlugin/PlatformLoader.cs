using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using AssetBundleLoadingTools.Utilities;

using JetBrains.Annotations;

using SiraUtil.Logging;

using UnityEngine;


namespace CustomFloorPlugin;

/// <summary>
/// Loads AssetBundles containing <see cref="CustomPlatform"/>s
/// </summary>
[UsedImplicitly]
public class PlatformLoader
{
    private readonly SiraLog _siraLog;
    private readonly BloomPrePassRendererSO _bloomPrepassRenderer;
    private readonly BloomPrePassEffectContainerSO _bloomPrePassEffectContainer;
    private readonly Dictionary<string, Task<CustomPlatform?>> _pathTaskPairs;


    public PlatformLoader(SiraLog siraLog, BloomPrePassRendererSO bloomPrepassRenderer, BloomPrePassEffectContainerSO bloomPrePassEffectContainer)
    {
        _siraLog = siraLog;
        _bloomPrepassRenderer = bloomPrepassRenderer;
        _bloomPrePassEffectContainer = bloomPrePassEffectContainer;
        _pathTaskPairs = new Dictionary<string, Task<CustomPlatform?>>();
    }

    /// <summary>
    /// Loads the platform's AssetBundle located at <param name="fullPath"></param><br/>
    /// If the loading process for this path is already started, the corresponding Task is awaited and the result returned
    /// </summary>
    /// <param name="fullPath">The path to the platform's AssetBundle</param>
    /// <returns>The loaded <see cref="CustomPlatform"/>, or null if an error occurs</returns>
    internal async Task<CustomPlatform?> LoadPlatformFromFileAsync(string fullPath)
    {
        if (_pathTaskPairs.TryGetValue(fullPath, out Task<CustomPlatform?> task)) return await task;
        task = LoadPlatformFromFileAsyncCore(fullPath);
        _pathTaskPairs.Add(fullPath, task);
        CustomPlatform? platform = await task;
        _pathTaskPairs.Remove(fullPath);
        return platform;
    }

    /// <summary>
    /// Asynchronously loads a <see cref="CustomPlatform"/> from a specified file path
    /// </summary>
    private async Task<CustomPlatform?> LoadPlatformFromFileAsyncCore(string fullPath)
    {
        byte[] bundleData = await Task.Run(() => File.ReadAllBytes(fullPath));

        AssetBundle? assetBundle = await AssetBundleExtensions.LoadFromMemoryAsync(bundleData);

        if (assetBundle == null)
        {
            _siraLog.Error($"File could not be loaded:{Environment.NewLine}{fullPath}");
            return null;
        }

        GameObject? platformPrefab = await AssetBundleExtensions.LoadAssetAsync<GameObject>(assetBundle, "_CustomPlatform");

        if (platformPrefab == null)
        {
            assetBundle.Unload(true);
            _siraLog.Error($"Platform GameObject could not be loaded:{Environment.NewLine}{fullPath}");
            return null;
        }

        assetBundle.Unload(false);

        await RepairPlatformShadersAsync(platformPrefab);

        CustomPlatform? customPlatform = platformPrefab.GetComponent<CustomPlatform>();

        if (customPlatform == null)
        {
            // Check for old platform
            global::CustomFloorPlugin.Legacy_Behaviours.CustomPlatform? legacyPlatform = platformPrefab.GetComponent<global::CustomFloorPlugin.Legacy_Behaviours.CustomPlatform>();
            if (legacyPlatform != null)
            {
                // Replace legacy platform component with up to date one
                customPlatform = platformPrefab.AddComponent<CustomPlatform>();
                customPlatform.platName = legacyPlatform.platName;
                customPlatform.platAuthor = legacyPlatform.platAuthor;
                customPlatform.hideDefaultPlatform = true;
                // Remove old platform data
                UnityEngine.Object.Destroy(legacyPlatform);
            }
            else
            {
                // No CustomPlatform component, abort
                UnityEngine.Object.Destroy(platformPrefab);
                _siraLog.Error($"AssetBundle does not contain a CustomPlatform:{Environment.NewLine}{fullPath}");
                return null;
            }
        }

        Camera[] cameras = platformPrefab.GetComponentsInChildren<Camera>(true);
        foreach (Camera camera in cameras)
        {
            BloomPrePass bloomPrePass = camera.gameObject.AddComponent<BloomPrePass>();
            bloomPrePass._bloomPrepassRenderer = _bloomPrepassRenderer;
            bloomPrePass._bloomPrePassEffectContainer = _bloomPrePassEffectContainer;
        }

        customPlatform.platHash = await Task.Run(() => ComputeHash(bundleData));
        customPlatform.fullPath = fullPath;
        customPlatform.name = $"{customPlatform.platName} by {customPlatform.platAuthor}";

        return customPlatform;
    }

    /// <summary>
    /// Computes the MD5 hash value for the given <see cref="data"/> and returns it's hexadecimal string representation
    /// </summary>
    private static string ComputeHash(byte[] data)
    {
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }

    /// <summary>
    /// Shader repair so that single pass shaders used prior to BS 1.29.4 show properly in VR
    /// </summary>
    private async Task RepairPlatformShadersAsync(GameObject platformPrefab)
    {
        List<Material> materials = ShaderRepair.GetMaterialsFromGameObjectRenderers(platformPrefab);

        var replacementInfo = await ShaderRepair.FixShadersOnMaterialsAsync(materials);

        if (!replacementInfo.AllShadersReplaced)
        {
            _siraLog.Warn($"Missing shader replacement data:");
            foreach (var shaderName in replacementInfo.MissingShaderNames)
            {
                _siraLog.Warn($"\t- {shaderName}");
            }
        }
        else _siraLog.Debug("All shaders replaced!");
    }
}