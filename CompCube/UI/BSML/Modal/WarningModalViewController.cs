using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using UnityEngine;

namespace CompCube.UI.BSML.EarlyLeaveWarning;

[ViewDefinition("CompCube.UI.BSML.Modal.WarningModalView.bsml")]
public class WarningModalViewController : BSMLAutomaticViewController
{
    private Action? _onYesButtonClickedCallback = null;
    private Action? _onNoButtonClickedCallback = null;
    
    [UIParams] private readonly BSMLParserParams _parserParams = null!;
    
    [UIValue("shouldShowYesOrNo")] private bool ShouldShowYesOrNo { get; set; }

    [UIValue("shouldShowOk")] private bool ShouldShowOk => !ShouldShowYesOrNo;
    
    [UIValue("modalText")] private string ModalText { get; set; } = "";

    public void ParseOntoViewController(ViewController viewController, string text, Action? onOkButtonClicked)
    {
        _onYesButtonClickedCallback = onOkButtonClicked;

        ModalText = text;
        
        ShouldShowYesOrNo = false;
        
        NotifyPropertyChanged(null);
        
        Parse(viewController);
    }

    public void ParseOntoViewController(ViewController viewController, string text, Action? onYesButtonClicked,
        Action? onNoButtonClicked)
    {
        _onYesButtonClickedCallback = onYesButtonClicked;
        _onNoButtonClickedCallback = onNoButtonClicked;
        
        ModalText = text;
        
        ShouldShowYesOrNo = true;
        
        NotifyPropertyChanged(null);
        
        Parse(viewController);
    }

    private void Parse(ViewController viewController)
    {
        BSMLParser.Instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "CompCube.UI.BSML.Modal.WarningModalView.bsml"), viewController.gameObject, this);
        Show();
    }

    private void Show()
    {
        _parserParams.EmitEvent("showModal");
    }

    public void Hide()
    {
        _parserParams.EmitEvent("hideModal");
    }

    [UIAction("onOkButtonClicked")]
    private void OnOkButtonClicked() => _onYesButtonClickedCallback?.Invoke();
    
    [UIAction("onYesButtonClicked")]
    private void OnYesButtonClicked() => _onYesButtonClickedCallback?.Invoke();
    
    [UIAction("onNoButtonClicked")]
    private void OnNoButtonClicked() => _onNoButtonClickedCallback?.Invoke();
}