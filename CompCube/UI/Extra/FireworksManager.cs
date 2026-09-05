using Zenject;

namespace CompCube.UI.Sound;

public class FireworksManager
{
    [Inject] private readonly FireworksController _fireworksController = null!;
    
    public void StartFireworks()
    {
        _fireworksController.enabled = true;
    }

    public void StopFireworks()
    {
        _fireworksController.enabled = false;
    }
}