using CompCube.Game;
using SiraUtil.Affinity;
using Zenject;

namespace CompCube.AffinityPatches.ScorePatches
{
    public class ImmediateRankDisplayPatch : IAffinity
    {
        [Inject] private readonly IGameEnergyCounter _gameEnergyCounter = null!;
        
        [AffinityPatch(typeof(RelativeScoreAndImmediateRankCounter),
            nameof(RelativeScoreAndImmediateRankCounter.UpdateRelativeScoreAndImmediateRank))]
        [AffinityPrefix]
        private void Prefix(ref int score, ref int modifiedScore, ref int maxPossibleScore,
            ref int maxPossibleModifiedScore)
        {
            if (!_gameEnergyCounter.noFail)
                return;
            
            modifiedScore = score;
            maxPossibleModifiedScore = maxPossibleScore;
        }
    }
}