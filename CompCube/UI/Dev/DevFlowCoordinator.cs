using System.Collections;
using CompCube.Extensions;
using HMUI;
using UnityEngine;
using Zenject;

namespace CompCube.UI.Dev;

public class DevFlowCoordinator : FlowCoordinator
{
    [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
    [Inject] private readonly DevViewController _devViewController = null!;
    
    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        ProvideInitialViewControllers(_devViewController);
        showBackButton = true;
        SetTitle("CompCube Dev");

        StartCoroutine(Coroutine());
        return;

        IEnumerator Coroutine()
        {
            yield return new WaitUntil(() => _devViewController.isActivated);

            _devViewController.OnViewControllerPresentedCallback =
                (controller) => this.ReplaceViewControllerSynchronously(controller);
        }
    }

    protected override void BackButtonWasPressed(ViewController topViewController)
    {
        if (topViewController is DevViewController)
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
            return;
        }
        
        this.ReplaceViewControllerSynchronously(_devViewController);
    }
}