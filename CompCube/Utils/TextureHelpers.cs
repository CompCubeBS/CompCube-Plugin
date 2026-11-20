using System.Reflection;
using UnityEngine;

namespace CompCube.Utils;

public static class TextureHelpers
{
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