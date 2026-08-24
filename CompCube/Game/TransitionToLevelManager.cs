using CompCube_Models.Models.Map;
using CompCube.Configuration;
using CompCube.UI.BSML.PauseMenu;
using CompCube.Extensions;
using CompCube.Game.MatchState;
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
        
    [Inject] private readonly MatchStateManager _matchStateManager = null!;
         
    public bool InLevel { get; private set; } = false;

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
            
        var beatmapLevel = level.GetBeatmapLevel() ?? throw new Exception("Could not get beatmap level!");
            
        // 1.39.1
        _menuTransitionsHelper.StartStandardLevel(
            "Solo",
            level.GetBeatmapKey(),
            beatmapLevel,
            _playerDataModel.playerData.overrideEnvironmentSettings,
            _playerDataModel.playerData.colorSchemesSettings.overrideDefaultColors ? _playerDataModel.playerData.colorSchemesSettings.GetSelectedColorScheme() : null,
            null,
            new GameplayModifiers(GameplayModifiers.EnergyType.Bar, true, false, false, GameplayModifiers.EnabledObstacleType.All, false, false, false, false, GameplayModifiers.SongSpeed.Normal, false, false, proMode, false, false),
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
            
        // 1.40.8
        /*_menuTransitionsHelper.StartStandardLevel(
            "Solo",
            level.GetBeatmapKey(),
            beatmapLevel,
            _playerDataModel.playerData.overrideEnvironmentSettings,
            _playerDataModel.playerData.colorSchemesSettings.overrideDefaultColors ? _playerDataModel.playerData.colorSchemesSettings.GetSelectedColorScheme() : null,
            true,
            beatmapLevel.GetColorScheme(beatmapLevel.GetCharacteristics().First(i => i.serializedName == "Standard"), level.GetBaseGameDifficultyType()),
            new GameplayModifiers(GameplayModifiers.EnergyType.Bar, true, false, false, GameplayModifiers.EnabledObstacleType.All, false, false, false, false, GameplayModifiers.SongSpeed.Normal, false, false, proMode, false, false),
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
        );*/
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