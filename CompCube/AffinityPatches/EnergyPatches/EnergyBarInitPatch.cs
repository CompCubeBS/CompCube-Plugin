using SiraUtil.Affinity;
using Zenject;

namespace CompCube.AffinityPatches.EnergyPatches
{
    public class EnergyBarInitPatch : IAffinity
    {
        [AffinityPatch(typeof(GameEnergyUIPanel), nameof(GameEnergyUIPanel.Init))]
        [AffinityPostfix]
        private void Postfix(GameEnergyUIPanel __instance)
        {
            __instance.gameObject.SetActive(false);
        }
    }
}