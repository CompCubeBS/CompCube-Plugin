using System.Collections;
using System.Reflection;
using System.Windows.Forms;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube.Game;
using HMUI;
using SiraUtil.Logging;
using SongCore;
using UnityEngine;
using Zenject;

namespace CompCube.UI.BSML.EarlyLeaveWarning;

[ViewDefinition("CompCube.UI.BSML.Modal.DownloadingBeatmapsModalView.bsml")]
public class DownloadingBeatmapsModal : BSMLAutomaticViewController
{
    [Inject] private readonly BeatmapDownloader _beatmapDownloader = null!;
    [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
    
    [UIParams] private readonly BSMLParserParams _parserParams = null!;

    [UIValue("modalText")] private string ModalText { get; set; } = "";
    
    public static void DownloadLevelsAndParseModalOntoGameObject(ViewController viewController, string[] hashes, Action? callback = null)
    {
        var hashesToDownload = hashes.Distinct().ToList();

        foreach (var hash in hashes)
            if (Collections.songWithHashPresent(hash))
                hashesToDownload.Remove(hash);

        if (hashesToDownload.Count == 0)
        {
            callback?.Invoke();
            return;
        }
        
        DownloadLevelsAndParseModalOntoGameObjectInternal(viewController, hashesToDownload.ToArray(), callback);
    }

    private static async Task DownloadLevelsAndParseModalOntoGameObjectInternal(ViewController viewController,
        string[] hashesToDownload, Action? callback = null)
    {
        var controller = BeatSaberUI.CreateViewController<DownloadingBeatmapsModal>();
        
        Parse(viewController, controller);
        
        controller._sharedCoroutineStarter.Run(controller.UpdateModalText(0, hashesToDownload.Length));

        controller._beatmapDownloader.OnMapDownloaded += controller.HandleMapDownloaded;
        await controller._beatmapDownloader.DownloadMaps(hashesToDownload);
        controller._beatmapDownloader.OnMapDownloaded -= controller.HandleMapDownloaded;
        
        controller.Hide();
        
        callback?.Invoke();
    }

    private IEnumerator UpdateModalText(int downloaded, int lengthOfBatch)
    {
        yield return new WaitForEndOfFrame();
        
        ModalText = $"Downloading beatmaps... ({downloaded}/{lengthOfBatch})";
        NotifyPropertyChanged(nameof(ModalText));
    }

    private void HandleMapDownloaded(int downloaded, int lengthOfBatch) => _sharedCoroutineStarter.Run(UpdateModalText(downloaded, lengthOfBatch));
    
    private static void Parse(ViewController viewControllerToParseOnto, DownloadingBeatmapsModal controller)
    {
        BSMLParser.Instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "CompCube.UI.BSML.Modal.DownloadingBeatmapsModalView.bsml"), viewControllerToParseOnto.gameObject, controller);
        controller.Show();
    }

    private void Hide()
    {
        _parserParams.EmitEvent("hideModal");
    }

    private void Show()
    {
        _parserParams.EmitEvent("showModal");
    }
}