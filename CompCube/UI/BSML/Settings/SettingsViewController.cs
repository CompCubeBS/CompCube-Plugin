using System.Net;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube.Configuration;
using Zenject;

namespace CompCube.UI.BSML.Settings;

[ViewDefinition("CompCube.UI.BSML.Settings.SettingsView.bsml")]
public class SettingsViewController : BSMLAutomaticViewController, IInitializable, IDisposable
{
    [Inject] private readonly BSMLSettings _bsmlSettings = null!;
    [Inject] private readonly PluginConfig _config = null!;

    [UIParams] private readonly BSMLParserParams _parserParams = null!;

    [UIValue("serverIp")]
    private string ServerIp
    {
        get => _config.WebsocketIp;
        set => _config.WebsocketIp = value;
    }

    [UIValue("apiIp")]
    private string ApiIp
    {
        get => _config.ApiIP;
        set => _config.ApiIP = value;
    }

    [UIValue("scoreSubmission")]
    private bool ScoreSubmission
    {
        get => _config.ScoreSubmission;
        set => _config.ScoreSubmission = value;
    }

    public void Initialize() => _bsmlSettings.AddSettingsMenu("CompCube", "CompCube.UI.BSML.Settings.SettingsView.bsml", this);

    public void Dispose() => _bsmlSettings.RemoveSettingsMenu(this);
}