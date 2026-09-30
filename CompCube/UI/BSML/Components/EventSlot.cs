using BeatSaberMarkupLanguage.Attributes;
using CompCube_Models.Models.Events;

namespace CompCube.UI.BSML.Components;

public class EventSlot(EventData eventData)
{
    [UIValue("displayName")] private string _displayName = eventData.DisplayName;
    [UIValue("description")] private string _description = eventData.Description;
    
    public event Action<EventData> OnJoinButtonClicked;

    [UIAction("joinButtonOnClick")]
    private void JoinButtonOnClick()
    {
        OnJoinButtonClicked?.Invoke(EventData);
    }
    
    public readonly EventData EventData = eventData;
}