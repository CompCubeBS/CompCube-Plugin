using CompCube.Models;
using CompCube.Configuration;
using CompCube.UI.BSML.PauseMenu;
using CompCube.Extensions;
using CompCube.Game.MatchState;
using CompCube.Networking;
using CompCube.Networking.ReplayStreaming;
using SiraUtil.Logging;
using SiraUtil.Submissions;
using Zenject;

namespace CompCube.Game;

public class TransitionToLevelManager
{
    [Inject] private readonly MenuTransitionsHelper _menuTransitionsHelper = null!;
    [Inject] private readonly PlayerDataModel _playerDataModel = null!;
    [Inject] private readonly SiraLog _siraLog = null!;
    [Inject] private readonly PluginConfig _config = null!;
	[Inject] private readonly ReplayPublisher _replayPublisher = null!;
         
    public bool InLevel { get; private set; } = false;
	public bool NoFailEnabled { get; private set; } = false;

    private Action<LevelCompletionResults, StandardLevelScenesTransitionSetupDataSO>? _menuSwitchCallback;
        
    public void StartLevel(
        VotingMap level, 
        DateTime unpauseTime, 
        bool proMode,
        Action<LevelCompletionResults, StandardLevelScenesTransitionSetupDataSO> onLevelCompletedCallback)
    {
        if (InLevel) 
            return;
            
        _menuSwitchCallback = onLevelCompletedCallback;
            
        InLevel = true;
		NoFailEnabled = level.Modifiers.Contains("NF");
            
        var beatmapLevel = level.GetBeatmapLevel() ?? throw new Exception("Could not get beatmap level!");
		_ = _replayPublisher.StartAsync(level).ContinueWith(task =>
		{
			if (task.Exception != null) _siraLog.Warn($"Replay streaming could not start: {task.Exception.GetBaseException().Message}");
		}, TaskScheduler.Default);
		var songSpeed = level.Modifiers.Contains("SS")
			? GameplayModifiers.SongSpeed.Slower
			: level.Modifiers.Contains("SFS")
				? GameplayModifiers.SongSpeed.SuperFast
				: level.Modifiers.Contains("FS")
					? GameplayModifiers.SongSpeed.Faster
					: GameplayModifiers.SongSpeed.Normal;
		var gameplayModifiers = new GameplayModifiers(
			level.Modifiers.Contains("4L") ? GameplayModifiers.EnergyType.Battery : GameplayModifiers.EnergyType.Bar,
			NoFailEnabled,
			level.Modifiers.Contains("IF"),
			false,
			level.Modifiers.Contains("NW") ? GameplayModifiers.EnabledObstacleType.NoObstacles : GameplayModifiers.EnabledObstacleType.All,
			level.Modifiers.Contains("NB"),
			false,
			level.Modifiers.Contains("SA"),
			level.Modifiers.Contains("DA"),
			songSpeed,
			level.Modifiers.Contains("NA"),
			level.Modifiers.Contains("GN"),
			level.Modifiers.Contains("PM") || proMode,
			level.Modifiers.Contains("ZM"),
			level.Modifiers.Contains("SN"));
            
#if BS_1_39_1
        _menuTransitionsHelper.StartStandardLevel(
            "Solo",
            level.GetBeatmapKey(),
            beatmapLevel,
            _playerDataModel.playerData.overrideEnvironmentSettings,
            _playerDataModel.playerData.colorSchemesSettings.overrideDefaultColors ? _playerDataModel.playerData.colorSchemesSettings.GetSelectedColorScheme() : null,
            null,
            gameplayModifiers,
            _playerDataModel.playerData.playerSpecificSettings,
            null,
            //TODO: fix this sometimes causing an exception because of creating from addressables
            EnvironmentsListModel.CreateFromAddressables(),
            "Menu",
            false,
            true,
            null,
            diContainer => AfterSceneSwitchToGameplayCallback(diContainer, unpauseTime),
            AfterSceneSwitchToMenuCallback,
            null
        );
#else
        _menuTransitionsHelper.StartStandardLevel(
            "Solo",
            level.GetBeatmapKey(),
            beatmapLevel,
            _playerDataModel.playerData.overrideEnvironmentSettings,
            _playerDataModel.playerData.colorSchemesSettings.overrideDefaultColors ? _playerDataModel.playerData.colorSchemesSettings.GetSelectedColorScheme() : null,
            true,
            beatmapLevel.GetColorScheme(beatmapLevel.GetCharacteristics().First(i => i.serializedName == "Standard"), level.GetBaseGameDifficultyType()),
            gameplayModifiers,
            _playerDataModel.playerData.playerSpecificSettings,
            null,
            EnvironmentsListModel.CreateFromAddressables(),
            "Menu",
            false,
            true,
            null,
            diContainer => AfterSceneSwitchToGameplayCallback(diContainer, unpauseTime),
            AfterSceneSwitchToMenuCallback,
            null
        );
#endif
    }

    public void StopLevel(Action<LevelCompletionResults, StandardLevelScenesTransitionSetupDataSO>? menuSwitchCallback = null)
    {
        _menuSwitchCallback = menuSwitchCallback;
            
        if (InLevel)
            _menuTransitionsHelper.StopStandardLevel();
    }

    private void AfterSceneSwitchToMenuCallback(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSo, LevelCompletionResults levelCompletionResults)
    {
        InLevel = false;
		NoFailEnabled = false;
		_replayPublisher.Complete(levelCompletionResults);
            
        _menuSwitchCallback?.Invoke(levelCompletionResults, standardLevelScenesTransitionSetupDataSo);
        _menuSwitchCallback = null;
    }

    private async void AfterSceneSwitchToGameplayCallback(DiContainer diContainer, DateTime unpauseTime)
    {
        try
        {
            if (!_config.ScoreSubmission)
                diContainer.Resolve<Submission>().DisableScoreSubmission("CompCube");
                
            diContainer.Resolve<PauseMenuViewController>().PopulateData(unpauseTime);
                
            var startingMenuController = diContainer.TryResolve<LevelStartUnpauseController>() ?? throw new Exception("Could not resolve StartingPauseMenuController");
                
            await startingMenuController.UnpauseLevelAtTime(unpauseTime);
        }
        catch (Exception e)
        {
            _siraLog.Error(e);
        }
    }
}
