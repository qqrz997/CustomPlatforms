using System.Collections.Generic;
using UnityEngine;

namespace CustomFloorPlugin.Helpers;

internal static class UnityExtensions
{
    public static GameObject? FindObject(this Transform transform, string name)
    {
        var found = transform.Find(name);
        return found != null && found.gameObject != null ? found.gameObject : null;
    }

    public static GameObject? AppendToName(this GameObject gameObject, string name)
    {
        if (gameObject == null) return null;
        gameObject.name += name;
        return gameObject;
    }

    public static void SetActive(this IEnumerable<GameObject> gameObjects, bool active)
    {
        foreach (var gameObject in gameObjects) gameObject.SetActive(active);
    }
}