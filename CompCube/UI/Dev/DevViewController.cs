using HMUI;
using Reactive;
using Zenject;

namespace CompCube.UI.Dev;

public class DevViewController : ViewController
{
    public Action<ViewController>? OnViewControllerPresentedCallback
    {
        get => _view.OnButtonPressedCallback;
        set => _view.OnButtonPressedCallback = value;
    }
    
    private DevView _view = null!;
    
    void Awake()
    {
        new DevView().Bind(ref _view).Use(transform);
    }
}