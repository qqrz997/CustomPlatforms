using System.Linq;
using BGLib.UnityExtension;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace CustomFloorPlugin;

internal class GameAssets : IInitializable
{
    private readonly SiraLog _log;
    private readonly Material internalErrorShader = new(Shader.Find("Hidden/InternalErrorShader"));

    public GameAssets(SiraLog log)
    {
        _log = log;
    }
    
    private Material? _mirrorMaterial;
    public Material MirrorMaterial => _mirrorMaterial ??= internalErrorShader;

    private Material? _noMirrorMaterial;
    public Material NoMirrorMaterial => _noMirrorMaterial ??= internalErrorShader;

    public void Initialize()
    {
        _log.Debug("Loading shaders");
        _mirrorMaterial = LoadAsset<Material>("Assets/Visuals/Materials/Mirror/PlayersPlaceMirror.mat");
        _noMirrorMaterial = LoadAsset<Material>("Assets/Visuals/Materials/Mirror/PlayersPlaceNoMirror.mat");
    }

    private T? LoadAsset<T>(string addressableKey) where T : Object
    {
        var asset = AddressablesExtensions.LoadContent<T>(addressableKey).FirstOrDefault();
        if (asset) _log.Debug($"Asset<{typeof(T).Name}> loaded: {asset.name}");
        else _log.Error($"Unable to load Asset{typeof(T).Name}>: {addressableKey}");
        return asset;
    }
}