using HMUI;

using Zenject;

namespace CustomFloorPlugin.UI;

internal class PlatformsFlowCoordinator : FlowCoordinator
{
    [Inject] private readonly PlatformListsViewController _platformsListViewController = null!;
    [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        if (!firstActivation)
            return;
        showBackButton = true;
        SetTitle("Custom Platforms");
        ProvideInitialViewControllers(_platformsListViewController);
    }

    protected override void BackButtonWasPressed(ViewController viewController) => _mainFlowCoordinator.DismissFlowCoordinator(this);
}