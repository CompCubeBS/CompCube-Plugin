using SiraUtil.Affinity;
using Zenject;

namespace CompCube.AffinityPatches.EnergyPatches
{
    public class EnergyBarInitPatch : IAffinity
    {
        [Inject] private readonly IGameEnergyCounter _gameEnergyCounter = null!;
        
        [AffinityPatch(typeof(GameEnergyUIPanel), nameof(GameEnergyUIPanel.Init))]
        [AffinityPostfix]
        private void Postfix(GameEnergyUIPanel __instance)
        {
            if (!_gameEnergyCounter.noFail)
                return;
            
            __instance.gameObject.SetActive(false);
        }
    }
}