using HMUI;
using Reactive;

namespace CompCube.UI.BSML.Match;

public class MapPickingViewController : ViewController
{
    private MapPickingView _mapPickingView = null!;
    
    private void Awake()
    {
        new MapPickingView().Bind(ref _mapPickingView).Use(transform);
    }
}