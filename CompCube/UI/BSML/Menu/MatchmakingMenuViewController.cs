using CompCube.Configuration;
using CompCube.Interfaces;
using CompCube.Server;
using CompCube.UI.BSML.EarlyLeaveWarning;
using HMUI;
using Reactive;
using SiraUtil.Logging;
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
    
    private CancellationTokenSource _cts = new();
    
    private void Awake()
    {
        new MatchmakingMenuView(_config).Bind(ref _view).Use(transform);
    }

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        _view.OnShouldConnectToServer += HandleShouldConnectToServer;
        _view.CancelButtonPressed += HandleCancelButtonPressed;
        _view.AboutButtonPressed += HandleAboutButtonPressed;
    }

    private void HandleAboutButtonPressed()
    {
        OnAboutButtonPressedCallback?.Invoke();
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        _view.OnShouldConnectToServer -= HandleShouldConnectToServer;
        _view.CancelButtonPressed -= HandleCancelButtonPressed;
        _view.AboutButtonPressed -= HandleAboutButtonPressed;
    }

    private async void HandleCancelButtonPressed()
    {
        try
        {
            _cts.Cancel();
            
            _view.IsConnected = false;
            
            await _serverListener.DisconnectAsync();
        }
        catch (Exception e)
        {
            _siraLog.Error(e);
            
            _view.IsConnected = false;
        }
    }

    private async void HandleShouldConnectToServer(string queue)
    {
        try
        {
            _cts = new CancellationTokenSource();
            
            _view.IsConnected = true;
            
            var canConnectToServer = await _serverChecker.CanConnectToServer();

            if (!canConnectToServer.CanConnect)
            {
                OkDialogModal.CreateOnTransform(transform, $"Failed to connect to server!\nReason: {canConnectToServer.Reason}", true);
                return;
            }

            await _serverListener.ConnectAsync($"queue/{queue}", _cts.Token);
        }
        catch (Exception e)
        {
            _siraLog.Error(e);
            _view.IsConnected = false;
            OkDialogModal.CreateOnTransform(transform, "Failed to connect to server:\n" + e.Message, true);
        }
    }
}