using CustomFloorPlugin.Models;
using UnityEngine;

using Zenject;


// ReSharper disable once CheckNamespace
namespace CustomFloorPlugin;

[RequireComponent(typeof(MeshRenderer))]
public class TrackMirror : MonoBehaviour, INotifyPlatformEnabled
{
    [Inject] private MirrorRendererSO _mirrorRenderer = null!;
    [Inject] private GameAssets _gameAssets = null!;
    
    private Mirror? _mirror;

    public void PlatformEnabled(DiContainer container)
    {
        if (_mirror != null)
            return;
        container.Inject(this);
        _mirror = gameObject.AddComponent<Mirror>();
        _mirror._reflectionPlaneTransform = _mirror.transform;
        _mirror._renderer = GetComponent<MeshRenderer>();
        _mirror._mirrorRenderer = Instantiate(_mirrorRenderer);
        _mirror._mirrorMaterial = _gameAssets.MirrorMaterial;
        _mirror._noMirrorMaterial = _gameAssets.NoMirrorMaterial;
    }
}