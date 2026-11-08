using System.Diagnostics;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube.Configuration;
using CompCube.Interfaces;
using CompCube.Server;
using CompCube.UI.BSML.Components;
using CompCube.UI.BSML.EarlyLeaveWarning;
using HarmonyLib;
using HMUI;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CompCube.UI.BSML.Menu;

public class MatchmakingMenuViewController : ViewController
{
    [Inject] private readonly ServerChecker _serverChecker = null!;
    [Inject] private readonly IServerListener _serverListener = null!;
    [Inject] private readonly SiraLog _siraLog = null!;
    [Inject] private readonly PluginConfig _config = null!;
    
    private MatchmakingMenuView _view = null!;
    
    public Action? OnAboutButtonPressedCallback { get; set; }
    
    private void Awake()
    {
        _view = new MatchmakingMenuView(_config);

        _view.Use(transform);
    }

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        _view.ShouldFindMatch += HandleShouldConnectToServer;
        _view.CancelButtonPressed += HandleCancelButtonPressed;
        _view.AboutButtonPressed += HandleAboutButtonPressed;
    }

    private void HandleAboutButtonPressed()
    {
        OnAboutButtonPressedCallback?.Invoke();
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        _view.ShouldFindMatch -= HandleShouldConnectToServer;
        _view.CancelButtonPressed -= HandleCancelButtonPressed;
    }

    private async void HandleCancelButtonPressed()
    {
        try
        {
            await _serverListener.DisconnectAsync();
        
            _view.ChangeState(false);
        }
        catch (Exception e)
        {
            _siraLog.Error(e);
        }
    }

    private async void HandleShouldConnectToServer()
    {
        try
        {
            var canConnectToServer = await _serverChecker.CanConnectToServer();
            
            if (!canConnectToServer.CanConnect)
            {
                new OkDialogModal($"Failed to connect to server!\nReason: {canConnectToServer.Reason}", true).Use(transform);
                return;
            }
            
            await _serverListener.ConnectAsync(_view.GetActiveQueueOptionState().QueueEndpoint);
        }
        catch (Exception e)
        {
            _siraLog.Error(e);
        }
    }
}