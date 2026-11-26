using System.Reflection;
using BeatSaberMarkupLanguage;
using UnityEngine;

namespace CompCube.Utils;

public static class TextureHelpers
{
    public static Sprite LoadSpriteFromDisk(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var texture = new Texture2D(1, 1);
        texture.LoadImage(bytes);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }

    public static Sprite LoadSpriteFromBeatmapFileLocationFresh(BeatmapLevel level)
    {
        var customLevelLoader = BeatSaberUI.DiContainer.Resolve<CustomLevelLoader>();

        var coverImageFilename = customLevelLoader._loadedBeatmapSaveData[level.levelID].standardLevelInfoSaveData
            ?.coverImageFilename;

        var folderPath = customLevelLoader._loadedBeatmapSaveData[level.levelID].customLevelFolderInfo.folderPath;

        var fullCoverImagePath = Path.Combine(folderPath, coverImageFilename!);
        
        return LoadSpriteFromDisk(fullCoverImagePath);
    }
    
    public static byte[] GetResourceData(string resourceName)
    {
        using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        
        if (resourceStream == null)
            throw new Exception($"Could not find resource {resourceName}!");

        using var memoryStream = new MemoryStream(new byte[resourceStream.Length], true);
        
        resourceStream.CopyTo(memoryStream);
        
        return memoryStream.ToArray();
    }

    public static Texture GetTextureFromResource(string resourceName)
    {
        var data = GetResourceData(resourceName);
        
        var texture = new Texture2D(1, 1);
        texture.LoadImage(data);
        texture.Apply();
        
        return texture;
    }
}