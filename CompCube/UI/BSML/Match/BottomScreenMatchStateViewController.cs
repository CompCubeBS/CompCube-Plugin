using System.Collections;
using System.Globalization;
using System.Net.Http;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube.Extensions;
using CompCube.UI.BSML.Components.EnergyBar;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CompCube.UI.BSML.Match;

[ViewDefinition("CompCube.UI.BSML.Match.BottomScreenMatchStateView.bsml")]
public class BottomScreenMatchStateViewController : BSMLAutomaticViewController
{
    [Inject] private readonly SiraLog _siraLog = null!;
    [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
    
    private CustomEnergyBar _redEnergyBar;
    private CustomEnergyBar _blueEnergyBar;

    [UIValue("roundText")] private string RoundText { get; set; } = "";
    
    [UIValue("redHealthText")] private string RedHealthText { get; set; } = "";
    [UIValue("blueHealthText")] private string BlueHealthText { get; set; } = "";

    [UIValue("multiplierText")] private string MultiplierText { get; set; } = "";

    [UIValue("redPlayerText")] private string RedPlayerText { get; set; } = "";
    [UIValue("bluePlayerText")] private string BluePlayerText { get; set; } = "";

    [UIComponent("redImage")] private readonly Image _redImage = null!;
    [UIComponent("blueImage")] private readonly Image _blueImage = null!;
    
    private readonly HttpClient _client = new();

    [UIAction("#post-parse")]
    void PostParse()
    {
        _redEnergyBar = CustomEnergyBar.InstantiateOntoViewController(this, new Vector3(-60f, -25f), false, new Color(1f, .4549f, .4235f));
        _blueEnergyBar = CustomEnergyBar.InstantiateOntoViewController(this, new Vector3(60f, -25f), true, new Color(.565f, .835f, 1f));
        
        var material = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "UINoGlowRoundEdge");
        
        _redImage.material = material;
        _blueImage.material = material;
    }
    
    public void PopulateData(CompCube_Models.Models.ClientData.UserInfo red, CompCube_Models.Models.ClientData.UserInfo blue)
    {
        RedPlayerText = red.GetFormattedUserName();
        BluePlayerText = blue.GetFormattedUserName();
        
        NotifyPropertyChanged(null);

        _sharedCoroutineStarter.Run(PopulateImagesCoroutine());
        return;

        IEnumerator PopulateImagesCoroutine()
        {
            yield return new WaitUntil(() => _redImage && _blueImage);
            
            SetSpriteImageFromUrl(red.ProfilePictureLink, _redImage);
            SetSpriteImageFromUrl(blue.ProfilePictureLink, _blueImage);
        }
    }

    private async Task SetSpriteImageFromUrl(string url, Image image)
    {
        image.sprite = Sprite.Create(new Texture2D(0, 0), new Rect(Vector2.zero, Vector2.zero), Vector2.zero);

        var response = await _client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            _siraLog.Warn("Failed to fetch user profile picture!");
            return;
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        
        var tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        
        image.sprite = sprite;
    }

    public void UpdateHealth(float redHealth, float blueHealth)
    {
        _sharedCoroutineStarter.Run(UpdateHealthCoroutine());
        return;
        
        IEnumerator UpdateHealthCoroutine()
        {
            yield return new WaitUntil(() => _redEnergyBar != null);
            
            RedHealthText = redHealth.ToString("P", CultureInfo.InvariantCulture);
            BlueHealthText = blueHealth.ToString("P", CultureInfo.InvariantCulture);
        
            _redEnergyBar.SetEnergy(redHealth);
            _blueEnergyBar.SetEnergy(blueHealth);
        
            NotifyPropertyChanged(null);
        }
    }

    public void UpdateRound(int round)
    {
        RoundText = $"Round {round}";
        NotifyPropertyChanged(nameof(RoundText));
    }

    public void UpdateMultiplier(float multiplier)
    {
        MultiplierText = $"{multiplier:F1}x";
        NotifyPropertyChanged(nameof(MultiplierText));
    }

    public void SetStatus(string status)
    {
        RoundText = status;
        NotifyPropertyChanged(nameof(RoundText));
    }
}