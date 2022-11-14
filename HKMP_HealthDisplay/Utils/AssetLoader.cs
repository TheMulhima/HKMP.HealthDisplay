namespace HKMP_HealthDisplay.Utils;
public static class AssetLoader
{
    public static Sprite Mask;
    public static Sprite MaskBlue;
    public static Sprite MaskEmpty;
    public static Sprite Plus;

    static AssetLoader()
    {
        Mask = CreateSprite($"{nameof(Mask)}.png");
        MaskBlue = CreateSprite($"{nameof(MaskBlue)}.png");
        MaskEmpty = CreateSprite($"{nameof(MaskEmpty)}.png");
        Plus = CreateSprite($"{nameof(Plus)}.png");
    }
    
    private static Sprite CreateSprite(string assetPath)
    {
        try
        {
            var asm = Assembly.GetCallingAssembly();
            using (var stream = asm.GetManifestResourceStream($"HKMP_HealthDisplay.Images.{assetPath}"))
            {
                    
                byte[] data = new byte[stream!.Length];
                stream.Read(data, 0, data.Length);

                Texture2D texture2D = new Texture2D(2, 2);
                bool success = texture2D.LoadImage(data);
                if (!success)
                    throw new Exception("ImageConversion.LoadImage() failed.");

                return Sprite.Create(texture2D, GetRectForTexture(texture2D), new UnityEngine.Vector2(0.5f, 0.5f), 64);
            }
        }
        catch (Exception ex)
        {
            HKMP_HealthDisplay.Instance.Log("Couldn't load embedded resource: " + ex.Message);
            throw;
        }
    }
    private static Rect GetRectForTexture(Texture2D texture)
    {
        return new(0f, 0f, texture.width, texture.height);
    }
}