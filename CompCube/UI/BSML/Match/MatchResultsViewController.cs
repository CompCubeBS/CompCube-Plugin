using System.Collections;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube.Extensions;
using CompCube.Game;
using CompCube.Game.MatchState;
using CompCube.Interfaces;
using CompCube.UI.FlowCoordinators;
using HMUI;
using Reactive;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace CompCube.UI.BSML.Match;

public class MatchResultsViewController : ViewController, IInitializable, IDisposable
{
    private MatchResultsView _view = null!;

    [Inject] private readonly IServerListener _serverListener = null!;
    [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;

    public Action? OnContinueButtonPressedCallback { get; set; }
    
    private void Awake()
    {
        new MatchResultsView().Bind(ref _view).Use(transform);
        
        _view.OnContinueButtonPressed += () => OnContinueButtonPressedCallback?.Invoke();
    }

    public void Initialize()
    {
        _serverListener.OnMatchFinished += HandleMatchFinished;
    }

    private void HandleMatchFinished(MatchFinishedPacket packet)
    {
        _sharedCoroutineStarter.Run(HandleMatchFinishedCoroutine());
        return;

        IEnumerator HandleMatchFinishedCoroutine()
        {
            yield return new WaitUntil(() => _view != null);
            
            _view.EloTransfer = packet.EloChange;
            _view.Won = packet.Won;
        }
    }

    public void Dispose()
    {
        _serverListener.OnMatchFinished -= HandleMatchFinished;
    }
}
