using UnityEngine;

/// <summary>Creates a few crisp, reusable sprites without requiring external art assets.</summary>
public static class RuntimeSprites
{
    private static Sprite square;
    private static Sprite circle;
    private static Sprite softCircle;

    public static Sprite Square => square != null ? square : square = CreateSprite(false, false);
    public static Sprite Circle => circle != null ? circle : circle = CreateSprite(true, false);
    public static Sprite SoftCircle => softCircle != null ? softCircle : softCircle = CreateSprite(true, true);

    private static Sprite CreateSprite(bool round, bool soft)
    {
        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = round ? (soft ? "Soft Glow Sprite" : "Circle Sprite") : "Square Sprite";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float nx = (x + .5f) / size * 2f - 1f;
            float ny = (y + .5f) / size * 2f - 1f;
            float distance = Mathf.Sqrt(nx * nx + ny * ny);
            float alpha = round ? (soft ? Mathf.Clamp01(1f - distance) : 1f - Mathf.SmoothStep(.88f, 1f, distance)) : 1f;
            pixels[y * size + x] = new Color(1, 1, 1, alpha);
        }
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 64f);
    }
}
