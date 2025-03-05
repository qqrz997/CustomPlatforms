using System.Threading.Tasks;

namespace CustomFloorPlugin.Models;

public interface IPlatformSpawner
{
    /// <summary>
    /// Replaces the current environment or platform with a new CustomPlatform.
    /// </summary>
    /// <param name="customPlatform">The CustomPlatform to replace the current one with</param>
    /// <returns>A task representing the async operation</returns>
    Task SpawnPlatform(CustomPlatform customPlatform);
}