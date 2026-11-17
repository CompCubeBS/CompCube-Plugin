using CompCube_Models.Models.Map;
using JetBrains.Annotations;
using SongCore;

namespace CompCube.Extensions;

public static class VotingMapExtensions
{
    public static BeatmapLevel? GetBeatmapLevel(this CCBeatmapData votingMap)
    {
        return Loader.GetLevelByHash(votingMap.Hash);
    }

    public static BeatmapDifficulty GetBaseGameDifficultyType(this CCBeatmapData votingMap) => votingMap.Difficulty switch
    {
        CCBeatmapData.DifficultyType.Easy => BeatmapDifficulty.Easy,
        CCBeatmapData.DifficultyType.Normal => BeatmapDifficulty.Normal,
        CCBeatmapData.DifficultyType.Hard => BeatmapDifficulty.Hard,
        CCBeatmapData.DifficultyType.Expert => BeatmapDifficulty.Expert,
        CCBeatmapData.DifficultyType.ExpertPlus => BeatmapDifficulty.ExpertPlus,
        _ => throw new ArgumentOutOfRangeException()
    };
    
    public static BeatmapDifficultyMask GetBaseGameDifficultyTypeMask(this CCBeatmapData votingMap) => votingMap.GetBaseGameDifficultyType() switch
    {
        BeatmapDifficulty.Easy => BeatmapDifficultyMask.Easy,
        BeatmapDifficulty.Normal => BeatmapDifficultyMask.Normal,
        BeatmapDifficulty.Hard => BeatmapDifficultyMask.Hard,
        BeatmapDifficulty.Expert => BeatmapDifficultyMask.Expert,
        BeatmapDifficulty.ExpertPlus => BeatmapDifficultyMask.ExpertPlus,
        _ => BeatmapDifficultyMask.All
    };
    
    public static BeatmapKey GetBeatmapKey(this CCBeatmapData votingMap) => votingMap.GetBeatmapLevel()?.GetBeatmapKeys().First(i =>
        i.beatmapCharacteristic.serializedName == "Standard" && i.difficulty == votingMap.GetBaseGameDifficultyType()) ?? throw new Exception("Could not find beatmap key!");
}