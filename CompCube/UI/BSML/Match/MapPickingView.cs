using CompCube.UI.BSML.Components.Card;
using Reactive;
using UnityEngine;

namespace CompCube.UI.BSML.Match;

public class MapPickingView : ReactiveComponent
{
    private State<string> _pickButtonText = null!;
    
    protected override GameObject Construct()
    {
        _pickButtonText = Remember("");

        return new Layout()
        {
            Children =
            {
                new Card()
                {
                    
                }
            }
        }.Use();
    }
}