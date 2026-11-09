using CompCube.Configuration;
using CompCube.Interfaces;
using CompCube.Server;
using CompCube.UI.BSML.EarlyLeaveWarning;
using HMUI;
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
    
    private void Awake()
    {
        _view = new MatchmakingMenuView(_config);

        _view.Use(transform);
    }

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        _view.OnFindMatchButtonPressed += HandleFindMatchButtonPressed;
        _view.CancelButtonPressed += HandleCancelButtonPressed;
        _view.AboutButtonPressed += HandleAboutButtonPressed;
    }

    private void HandleFindMatchButtonPressed()
    {
        var modal = new YesOrNoDialogModal(
            "Competitive matches may take a long time. Only queue when you can finish the match and protect competitive integrity. Are you sure you want to continue?",
            true,
            async void () =>
            {
                try
                {
                    _view.ChangeState(true);
                    await HandleShouldConnectToServer();
                }
                catch (Exception e)
                {
                    _siraLog.Error(e);
                }
            },
            () => { });
        modal.Use(transform);
        
        modal.Show();
    }

    private void HandleAboutButtonPressed()
    {
        OnAboutButtonPressedCallback?.Invoke();
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        _view.OnFindMatchButtonPressed -= HandleFindMatchButtonPressed;
        _view.CancelButtonPressed -= HandleCancelButtonPressed;
        _view.AboutButtonPressed -= HandleAboutButtonPressed;
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

    private async Task HandleShouldConnectToServer()
    {
        var canConnectToServer = await _serverChecker.CanConnectToServer();

        if (!canConnectToServer.CanConnect)
        {
            var modal = new OkDialogModal($"Failed to connect to server!\nReason: {canConnectToServer.Reason}", true);
            modal.Use(transform);
            modal.Show();
            return;
        }

        await _serverListener.ConnectAsync(_view.GetActiveQueueOptionState());
    }
}