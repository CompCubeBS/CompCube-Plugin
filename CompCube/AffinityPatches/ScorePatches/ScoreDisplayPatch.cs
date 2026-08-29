using System;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace CompCube.AffinityPatches.ScorePatches
{
    public class ScoreDisplayPatch : IAffinity
    {
        [Inject] private readonly ScoreController _scoreController = null!;
        [Inject] private readonly IGameEnergyCounter _gameEnergyCounter = null!;
        
        [AffinityPatch(typeof(ScoreUIController), nameof(ScoreUIController.UpdateScore), argumentTypes: new Type[] {typeof(int)})]
        [AffinityPrefix]
        private void Prefix(ref int displayScore)
        {
            if (!_gameEnergyCounter.noFail)
                return;
            
            displayScore = _scoreController._multipliedScore;
        }
    }
}