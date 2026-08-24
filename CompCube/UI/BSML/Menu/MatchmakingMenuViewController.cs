using System.Diagnostics;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube.Configuration;
using CompCube.Interfaces;
using CompCube.Server;
using CompCube.UI.BSML.Components;
using CompCube.UI.BSML.EarlyLeaveWarning;
using SiraUtil.Logging;
using UnityEngine.UI;
using Zenject;

namespace CompCube.UI.BSML.Menu
{
    [ViewDefinition("CompCube.UI.BSML.Menu.MatchmakingMenuView.bsml")]
    public class MatchmakingMenuViewController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        [Inject] private readonly PluginConfig _config = null!;
        [Inject] private readonly IServerListener _serverListener = null!;
        [Inject] private readonly ServerChecker _serverChecker = null!;
        [Inject] private readonly WarningModalViewController _warningModalViewController = null!;
        [Inject] private readonly SiraLog _siraLog = null!;

        [UIParams] private readonly BSMLParserParams _parserParams = null!;

        private Action? _aboutButtonClickedCallback;

        public void SetButtonCallbacks(Action aboutButtonClickedCallback)
        {
            _aboutButtonClickedCallback = aboutButtonClickedCallback;
        }
        
        [UIAction("aboutButtonOnClick")]
        private void AboutButtonClicked() => _aboutButtonClickedCallback?.Invoke();
        
        [UIValue("queueOptions")] 
        private readonly List<object> _queueOptions = [new QueueOptionTab("Casual 1v1", "standard_casual_1v1"), new QueueOptionTab("Competitive 1v1", "standard_competitive_1v1")];

        [UIComponent("queueTabSelector")] private readonly TabSelector _queueTabSelector = null!;

        [UIValue("failedToConnectReason")] private string FailedToConnectReason { get; set; } = "";

        [UIAction("joinMatchmakingPoolButtonOnClick")]
        private async void HandleJoinMatchmakingPoolClicked()
        {
            try
            {
                SetState(true);

                var canConnectToServer = await _serverChecker.CanConnectToServer();

                if (!canConnectToServer.CanConnect)
                {
                    ShowFailedToConnectModal(canConnectToServer.Reason);
                    return;
                }
            
                await _serverListener.ConnectAsync(((QueueOptionTab) _queueOptions[_queueTabSelector.TextSegmentedControl.selectedCellNumber]).QueueEndpoint, () =>
                {
                    
                });
            }
            catch (Exception e)
            {
                SetState(false);
                ShowFailedToConnectModal();
                _siraLog.Error(e);
            }
        }

        private void ShowFailedToConnectModal(string reason = "")
        {
            SetState(false);

            var modalText = "Failed to connect to server";
            
            if (reason != "")
                modalText += "\nReason: " + reason;
            
            _warningModalViewController.ParseOntoGameObject(this, modalText, _warningModalViewController.Hide);
        }

        [UIComponent("join-pool-button")] private readonly Button _joinPoolButton = null!;

        [UIComponent("leave-pool-button")] private readonly Button _leavePoolButton = null!;

        [UIComponent("about-button")] private readonly Button _aboutButton = null!;

        [UIComponent("events-button")] private readonly Button _eventsButton = null!;
        private void SetState(bool connected)
        {
            _joinPoolButton.interactable = !connected;
            _leavePoolButton.gameObject.SetActive(connected);
            _aboutButton.interactable = !connected;
            _eventsButton.interactable = !connected;
        }
        
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            
            SetState(false);
        }

        [UIAction("leaveMatchmakingPoolButtonOnClick")]
        private void HandleLeaveMatchmakingPoolButtonClicked() => _parserParams.EmitEvent("disconnectModalShowEvent");
        
        [UIAction("leaveMatchmakingPoolDenyButtonOnClick")] 
        private void HandleLeaveMatchmakingPoolDenied() => _parserParams.EmitEvent("disconnectModalHideEvent");

        [UIAction("leaveMatchmakingPoolAllowButtonOnClick")]
        private void LeaveMatchmakingPoolAllowButton()
        {
            _parserParams.EmitEvent("disconnectModalHideEvent");
            _serverListener.DisconnectAsync();
            SetState(false);
        }

        [UIAction("failedToConnectModalOkButtonOnClick")]
        private void HandleFailedToConnectModalOkButtonClicked() => _parserParams.EmitEvent("failedToConnectModalHide");

        public void Initialize()
        {
            _serverListener.OnAbruptDisconnect += HandleAbruptDisconnect;
            
            if (!_config.ConnectToDebugQueue)
                return;
            
            if (_queueOptions.Any(i => ((QueueOptionTab) i).Queue == "debug"))
                return;
            
            _queueOptions.Add(new QueueOptionTab("Debug", "debug"));
        }

        private void HandleAbruptDisconnect(string reason)
        {
            SetState(false);
        }

        public void Dispose()
        {
            _serverListener.OnAbruptDisconnect -= HandleAbruptDisconnect;
        }
    }
}