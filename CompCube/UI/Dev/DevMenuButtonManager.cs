using BeatSaberMarkupLanguage.MenuButtons;
using CompCube.Extensions;
using Zenject;

namespace CompCube.UI.Dev;

public class DevMenuButtonManager : IInitializable
{
    [Inject] private readonly MenuButtons _menuButtons = null!;
    [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
    [Inject] private readonly DevFlowCoordinator _devFlowCoordinator = null!;
    
    public void Initialize()
    {
        #if DEBUG
        _menuButtons.RegisterButton(new MenuButton("CompCube Dev", () =>
        {
            _mainFlowCoordinator.PresentFlowCoordinatorSynchronously(_devFlowCoordinator);
        }));
        
        #endif
    }
}