using System.Collections;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.ClientData;
using CompCube.Extensions;
using CompCube.UI.BSML.Components.CustomLevelBar;
using CompCube.UI.BSML.Components.EnergyBar;
using HMUI;
using Reactive;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CompCube.UI.BSML.Match;

public class MatchPreviewViewController : ViewController
{
    [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
    
    private MatchPreviewView _view = null!;
    
    void Awake()
    {
        new MatchPreviewView().Bind(ref _view).Use(transform);
    }
    
    public void PopulateData(UserProfile red, UserProfile blue)
    {
        _sharedCoroutineStarter.Run(PopulateDataCoroutine());
        return;
        
        IEnumerator PopulateDataCoroutine()
        {
            yield return new WaitUntil(() => _view != null);
            
            _view.RedPlayer = red;
            _view.BluePlayer = blue;
            
            _view.StartAnimation();
        }
    }
}