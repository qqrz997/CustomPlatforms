using System.Threading.Tasks;

namespace CustomFloorPlugin.Models;

public interface IEnvironmentHider
{
    /// <summary>
    /// Uses the configuration of a CustomPlatform to hide parts of the current Beat Saber environment.
    /// </summary>
    /// <param name="customPlatform">The CustomPlatform whom to hide objects for</param>
    /// <returns>A task representing the async operation</returns>
    Task HideObjectsForPlatform(CustomPlatform customPlatform);
}