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
using HarmonyLib;
using SiraUtil.Logging;
using UnityEngine;
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
        [Inject] private readonly IApi _api = null!;

        [UIParams] private readonly BSMLParserParams _parserParams = null!;

        private Action? _aboutButtonClickedCallback;

        public void SetButtonCallbacks(Action aboutButtonClickedCallback)
        {
            _aboutButtonClickedCallback = aboutButtonClickedCallback;
        }
        
        [UIAction("aboutButtonOnClick")]
        private void AboutButtonClicked() => _aboutButtonClickedCallback?.Invoke();
        
        [UIValue("queueOptions")] 
        private List<QueueOptionTab> _queueOptions = [];

        [UIComponent("queueTabSelector")] private readonly TabSelector _queueTabSelector = null!;


        [UIAction("joinMatchmakingPoolButtonOnClick")]
        private async void HandleJoinMatchmakingPoolClicked()
        {
            try
            {
                SetState(State.Connected);

                var canConnectToServer = await _serverChecker.CanConnectToServer();

                if (!canConnectToServer.CanConnect)
                {
                    ShowFailedToConnectModal(canConnectToServer.Reason);
                    return;
                }
            
                await _serverListener.ConnectAsync((_queueOptions[_queueTabSelector.TextSegmentedControl.selectedCellNumber]).QueueEndpoint);
            }
            catch (Exception e)
            {
                SetState(State.Disconnected);
                ShowFailedToConnectModal();
                _siraLog.Error(e);
            }
        }

        private void ShowFailedToConnectModal(string reason = "")
        {
            SetState(State.Disconnected);

            var modalText = "Failed to connect to server";
            
            if (reason != "")
                modalText += "\nReason: " + reason;
            
            _warningModalViewController.ParseOntoViewController(this, modalText, _warningModalViewController.Hide);
        }

        [UIComponent("join-pool-button")] private readonly Button _joinPoolButton = null!;

        [UIComponent("leave-pool-button")] private readonly Button _leavePoolButton = null!;

        [UIComponent("about-button")] private readonly Button _aboutButton = null!;

        [UIComponent("events-button")] private readonly Button _eventsButton = null!;

        [UIObject("loadingIndicator")] private readonly GameObject _loadingIndicatorGo = null!;
        
        private void SetState(State state)
        {
            _joinPoolButton.interactable = state == State.Disconnected;
            _leavePoolButton.gameObject.SetActive(state == State.Connected);
            _aboutButton.interactable = state is State.Disconnected or State.FetchingAvailableQueues;
            _eventsButton.interactable = state is State.Disconnected or State.FetchingAvailableQueues;
            _loadingIndicatorGo.SetActive(state == State.FetchingAvailableQueues);
            _queueTabSelector.gameObject.SetActive(state is State.Connected or State.Disconnected);
        }
        
        protected override async void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            try
            {
                base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
                
                await FetchQueues();
            }
            catch (Exception e)
            {
                ShowFailedToConnectModal("An unhandled exception occured while fetching queue data from server!");
                
                _siraLog.Error(e);
            }
        }

        private async Task FetchQueues()
        {
            SetState(State.FetchingAvailableQueues);
                
            var queues = await _api.GetQueues();

            if (queues == null)
            {
                _warningModalViewController.ParseOntoViewController(this, "Failed to fetch available queues!", _warningModalViewController.Hide);
                return;
            }
                
            queues.Do(i => _siraLog.Info(i.Name));
                
            _queueOptions = queues.Select(i => i.ToQueueOptionTab()).ToList();
            
            
            SetState(State.Disconnected);
                
            _queueTabSelector.TextSegmentedControl.SetTexts(_queueOptions.Select(i => i.TabName).ToArray());
            _queueTabSelector.TextSegmentedControl.SetTexts(_queueOptions.Select(i => i.TabName).ToArray());
        }

        [UIAction("leaveMatchmakingPoolButtonOnClick")]
        private void HandleLeaveMatchmakingPoolButtonClicked() => _warningModalViewController.ParseOntoViewController(this, "Are you sure you want to leave the matchmaking pool?",
            () =>
            {
                _serverListener.DisconnectAsync();
                SetState(State.Disconnected);
            },
            _warningModalViewController.Hide);

        public void Initialize()
        {
            _serverListener.OnAbruptDisconnect += HandleAbruptDisconnect;
        }

        private void HandleAbruptDisconnect(string reason)
        {
            SetState(State.Disconnected);
        }

        public void Dispose()
        {
            _serverListener.OnAbruptDisconnect -= HandleAbruptDisconnect;
        }

        private enum State
        {
            Connected,
            FetchingAvailableQueues,
            Disconnected
        }
    }
}