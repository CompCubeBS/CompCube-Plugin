using System.IO.Compression;
using BeatSaverSharp;
using CompCube.Interfaces;
using IPA.Utilities;
using SiraUtil.Logging;
using SongCore;
using Zenject;

namespace CompCube.Game;

public class BeatmapDownloader
{
    [Inject] private readonly SiraLog _siraLog = null!;
    [Inject] private readonly IApi _api = null!;

    private readonly BeatSaver _beatSaver = new BeatSaver("CompCube", new Version(1, 0, 0));
    
    public event Action<int, int>? OnMapDownloaded;
    
    public bool IsDownloadingBeatmaps { get; private set; } = false;

    public async Task DownloadMaps(string[] mapHashes)
    {
        IsDownloadingBeatmaps = true;
        var mapsDownloaded = 0;

        var hashesWithoutDuplicates = mapHashes.Distinct().ToArray();
        
        foreach (var mapHash in hashesWithoutDuplicates)
        {
            if (Collections.songWithHashPresent(mapHash))
                continue;

            _siraLog.Notice($"Attempting to download {mapHash}...");
            
            var beatmap = await _beatSaver.BeatmapByHash(mapHash);
            
            if (beatmap == null)
                throw new Exception($"Failed to download beatmap " + mapHash);

            var beatmapData = await beatmap.LatestVersion.DownloadZIP();

            if (beatmapData == null)
                throw new Exception($"Failed to download {mapHash}!");
            
            var zippedBeatmap = new ZipArchive(new MemoryStream(beatmapData), ZipArchiveMode.Read);
            zippedBeatmap.ExtractToDirectory(Path.Combine(UnityGame.InstallPath, "Beat Saber_Data", "CustomLevels", mapHash));
            
            mapsDownloaded++;
            OnMapDownloaded?.Invoke(mapsDownloaded, mapHashes.Length);
        }
        
        Loader.Instance.RefreshSongs(false);

        while (Loader.AreSongsLoading)
            await Task.Delay(25);
        
        IsDownloadingBeatmaps = false;
    }
}