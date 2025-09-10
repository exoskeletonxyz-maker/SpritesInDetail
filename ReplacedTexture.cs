using Microsoft.Xna.Framework.Graphics;

namespace SpritesInDetail
{
    internal class ReplacedTexture : Texture2D
    {
        public Texture2D OriginalTexture { get; private set; }
        public Texture2D? NewTexture { get; private set; }

        public HDTextureInfo HDTextureInfo { get; private set; }

        public ReplacedTexture(Texture2D originalTexture, Texture2D? newTexture, HDTextureInfo hdTextureInfo, int? width = null, int? height = null)
            : base(originalTexture.GraphicsDevice, width ?? originalTexture.Width, height ?? originalTexture.Height)
        {
            OriginalTexture = originalTexture;
            NewTexture = newTexture;
            HDTextureInfo = hdTextureInfo;
        }

        // Helper method untuk Android-safe replacement
        public void ApplyNewTexture()
        {
            if (NewTexture != null)
            {
                // Copy pixel data untuk Android
                Color[] data = new Color[NewTexture.Width * NewTexture.Height];
                NewTexture.GetData(data);
                this.SetData(data);
            }
        }
    }
}
