using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace SpritesInDetail
{
    public class HDTextureInfo
    {
        public bool Enabled { get; set; } = true;
        public string Target { get; set; } = "";
        public Texture2D? HDTexture { get; set; }

        public int SpriteWidth { get; set; }
        public int SpriteHeight { get; set; }

        public int SpriteOriginX { get; set; }
        public int SpriteOriginY { get; set; }

        public int WidthScale { get; set; }
        public int HeightScale { get; set; }

        public bool DisableBreath { get; set; }
        public int ChestSourceX { get; set; }
        public int ChestSourceY { get; set; }
        public int ChestSourceWidth { get; set; }
        public int ChestSourceHeight { get; set; }
        public int ChestAdjustX { get; set; }
        public int ChestAdjustY { get; set; }

        public bool IsFarmer { get; set; }

        public IManifest ContentPackManifest { get; set; }
        public Dictionary<string, string?> Conditionals { get; set; } = new Dictionary<string, string?>();
        public Dictionary<Vector2, Texture2D> PixelReplacements { get; set; } = new Dictionary<Vector2, Texture2D>();

        public HDTextureInfo(Sprite sprite, IManifest contentPackManifest, Texture2D? hdTexture, Dictionary<string, string?> conditionals, bool isFarmer = false)
        {
            ContentPackManifest = contentPackManifest;
            Target = sprite.Target;
            HDTexture = hdTexture;
            WidthScale = sprite.WidthScale ?? 4;
            HeightScale = sprite.HeightScale ?? 4;
            SpriteWidth = sprite.SpriteWidth ?? 32;
            SpriteHeight = sprite.SpriteHeight ?? 64;

            IsFarmer = isFarmer;
            if (IsFarmer)
            {
                SpriteOriginX = sprite.SpriteOriginX ?? 16;
                SpriteOriginY = sprite.SpriteOriginY ?? 128;
            }
            else
            {
                SpriteOriginX = sprite.SpriteOriginX ?? 32;
                SpriteOriginY = sprite.SpriteOriginY ?? 112;
            }

            // Breath / chest handling
            if (!sprite.BreathType.HasValue || sprite.BreathType == BreathType.Male)
            {
                ChestSourceX = sprite.ChestSourceX ?? 24;
                ChestSourceY = sprite.ChestSourceY ?? 98;
                ChestSourceWidth = sprite.ChestSourceWidth ?? 16;
                ChestSourceHeight = sprite.ChestSourceHeight ?? 16;
                ChestAdjustX = sprite.ChestAdjustX ?? 0;
                ChestAdjustY = sprite.ChestAdjustY ?? 0;
            }
            else if (sprite.BreathType == BreathType.Female)
            {
                ChestSourceX = sprite.ChestSourceX ?? 24;
                ChestSourceY = sprite.ChestSourceY ?? 100;
                ChestSourceWidth = sprite.ChestSourceWidth ?? 16;
                ChestSourceHeight = sprite.ChestSourceHeight ?? 8;
                ChestAdjustX = sprite.ChestAdjustX ?? 0;
                ChestAdjustY = sprite.ChestAdjustY ?? -4;
            }
            else
            {
                DisableBreath = true;
            }

            Conditionals = conditionals;
        }
    }

    // Minimal placeholder classes to avoid compile errors
    public class Sprite
    {
        public string Target { get; set; } = "";
        public int? WidthScale { get; set; }
        public int? HeightScale { get; set; }
        public int? SpriteWidth { get; set; }
        public int? SpriteHeight { get; set; }
        public int? SpriteOriginX { get; set; }
        public int? SpriteOriginY { get; set; }
        public BreathType? BreathType { get; set; }
        public int? ChestSourceX { get; set; }
        public int? ChestSourceY { get; set; }
        public int? ChestSourceWidth { get; set; }
        public int? ChestSourceHeight { get; set; }
        public int? ChestAdjustX { get; set; }
        public int? ChestAdjustY { get; set; }
    }

    public enum BreathType
    {
        Male,
        Female,
        None
    }
}
