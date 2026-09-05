using HarmonyLib;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace CompCube.UI.Sound;

public class SoundEffectManager : MonoBehaviour, IInitializable
{
    private readonly AudioClip _levelClearedAudioClip = Resources.FindObjectsOfTypeAll<AudioClip>().FirstOrDefault(x => x.name == "LevelCleared");
    private readonly AudioClip _gongAudioClip = Resources.FindObjectsOfTypeAll<AudioClip>().FirstOrDefault(x => x.name == "MultiplayerLobbyGong");
    
    private AudioSource _audioSource;
    
    [Inject] private readonly AudioClipAsyncLoader _audioClipAsyncLoader = null!;
    [Inject] private readonly PerceivedLoudnessPerLevelModel _perceivedLoudnessPerLevelModel = null!;
    [Inject] private readonly SongPreviewPlayer _songPreviewPlayer = null!;
    
    public void PlayWinningMusic()
    {
        _songPreviewPlayer.CrossfadeTo(_levelClearedAudioClip, -4f, 0f, _levelClearedAudioClip.length, null);
    }

    public void CrossfadeToDefault()
    {
        _songPreviewPlayer.CrossfadeToDefault();
    }

    public void PlayGongSoundEffect()
    {
        _audioSource.PlayOneShot(_gongAudioClip);
    }

    public void PlayBeatmapLevelPreview(BeatmapLevel level) => PlayBeatmapLevelPreviewAsync(level);

    private async Task PlayBeatmapLevelPreviewAsync(BeatmapLevel level)
    {
        var clip = await _audioClipAsyncLoader.LoadPreview(level);

        var perceivedLoudness = _perceivedLoudnessPerLevelModel.GetLoudnessCorrectionByLevelId(level.levelID);
        
        _songPreviewPlayer.CrossfadeTo(clip, perceivedLoudness, level.previewStartTime, level.previewDuration, () =>
        {
            _audioClipAsyncLoader.Unload(clip);
        });
    }

    public void Initialize()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
    }
}