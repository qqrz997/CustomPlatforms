using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomFloorPlugin.Models;

/// <summary>
/// The Environment Object Source is responsible for providing references to all relevant GameObjects of the current
/// Beat Saber environment. Each collection corresponds to one configuration requirement of a CustomPlatform.
/// </summary>
public interface IEnvironmentObjectSource
{
    /// <summary>
    /// A task to represent when all environment objects have been found. This must be awaited before accessing any
    /// collection of objects, because some parts of environments do not get created, and thus found, immediately on
    /// scene initialization.
    /// </summary>
    Task InitializationTask { get; } 
    
    /// <summary>
    /// GameObjects provided by this property should always be hidden, regardless of the current CustomPlatform
    /// </summary>
    IEnumerable<GameObject> AlwaysHide { get; }
    
    IEnumerable<GameObject> PlayersPlace { get; }
    IEnumerable<GameObject> SmallRings { get; }
    IEnumerable<GameObject> BigRings { get; }
    IEnumerable<GameObject> Visualizer { get; }
    IEnumerable<GameObject> Towers { get; }
    IEnumerable<GameObject> Highway { get; }
    IEnumerable<GameObject> BackColumns { get; }
    IEnumerable<GameObject> DoubleColorLasers { get; }
    IEnumerable<GameObject> BackLasers { get; }
    IEnumerable<GameObject> RotatingLasers { get; }
    IEnumerable<GameObject> TrackLights { get; }
}