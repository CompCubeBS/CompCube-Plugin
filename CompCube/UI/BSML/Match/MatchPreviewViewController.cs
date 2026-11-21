using System.Collections;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube.Extensions;
using CompCube.Interfaces;
using CompCube.UI.BSML.Components.CustomLevelBar;
using CompCube.UI.BSML.Components.EnergyBar;
using HMUI;
using Reactive;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CompCube.UI.BSML.Match;

public class MatchPreviewViewController : ViewController, IInitializable, IDisposable
{
    [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;

    [Inject] private readonly IServerListener _serverListener = null!;
    
    private MatchPreviewView _view = null!;
    
    void Awake()
    {
        new MatchPreviewView().Bind(ref _view).Use(transform);
    }

    private void HandleMatchCreated(MatchCreatedPacket packet) => PopulateData(packet.Red, packet.Blue);
    

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

    public void Initialize()
    {
        _serverListener.OnMatchCreated += HandleMatchCreated;
    }

    public void Dispose()
    {
        _serverListener.OnMatchCreated -= HandleMatchCreated;
    }
}