using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace SpritesInDetail
{
    public class ModEntry : Mod
    {
        private List<HDTextureInfo> hdTextures = new();
        private HashSet<string> spritesToInvalidateDaily = new();
        private List<Tuple<string, IManifest>> tokensToRegister = new();
        public static Dictionary<IManifest, Dictionary<string, string>> settings = new();

        private int tick = 0;

        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(ModManifest.UniqueID);

            // Patch SpriteBatch.Draw specific overload
            MethodInfo drawMethod = typeof(SpriteBatch).GetMethod(
                "Draw",
                new Type[] { typeof(Texture2D), typeof(Rectangle?), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(SpriteEffects), typeof(float) }
            );
            harmony.Patch(drawMethod, new HarmonyMethod(typeof(ModEntry).GetMethod(nameof(DrawReplacedTexture), BindingFlags.Public | BindingFlags.Static)));

            // Game events
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Content.AssetRequested += OnAssetRequested;

            Monitor.Log("SpritesInDetail loaded.", LogLevel.Info);
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (spritesToInvalidateDaily.Count > 0)
            {
                Helper.GameContent.InvalidateCache(asset => spritesToInvalidateDaily.Contains(asset.Name.BaseName));
            }
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var cpApi = Helper.ModRegistry.GetApi<Pathoschild.Stardew.Common.ContentPatcher.IContentPatcherAPI>("Pathoschild.ContentPatcher");
            var configMenuApi = Helper.ModRegistry.GetApi<GenericModConfigMenu.IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            // Register Content Patcher tokens
            if (cpApi != null)
            {
                foreach (var token in tokensToRegister)
                {
                    cpApi.RegisterToken(token.Item2, token.Item1, () => new[] { settings[token.Item2][token.Item1] });
                }
            }

            // Register config menu
            if (configMenuApi != null)
            {
                foreach (var setting in settings)
                {
                    if (!setting.Value.ContainsKey("Enabled")) continue;

                    configMenuApi.Register(
                        mod: setting.Key,
                        reset: () => settings[setting.Key]["Enabled"] = "true",
                        save: () =>
                        {
                            Helper.GameContent.InvalidateCache(asset => hdTextures.Any(h => h.Target == asset.Name.BaseName));
                        }
                    );

                    configMenuApi.AddBoolOption(
                        mod: setting.Key,
                        name: () => "Enabled",
                        getValue: () => settings[setting.Key]["Enabled"].ToLower() == "true",
                        setValue: value => settings[setting.Key]["Enabled"] = value.ToString()
                    );
                }
            }
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            tick++;
            if (tick >= 2)
            {
                // Clean-up
                Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            }
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            foreach (var info in hdTextures)
            {
                if (!e.Name.IsEquivalentTo(info.Target)) continue;

                e.Edit(asset =>
                {
                    bool enabled = true;

                    if (settings.ContainsKey(info.ContentPackManifest) &&
                        settings[info.ContentPackManifest].TryGetValue("Enabled", out string val) &&
                        val.ToLower() == "false")
                    {
                        enabled = false;
                    }

                    var cpApi = Helper.ModRegistry.GetApi<Pathoschild.Stardew.Common.ContentPatcher.IContentPatcherAPI>("Pathoschild.ContentPatcher");
                    if (enabled && info.Conditionals.Count > 0 && cpApi != null && cpApi.IsConditionsApiReady)
                    {
                        var managedConditions = cpApi.ParseConditions(info.ContentPackManifest, info.Conditionals, new SemanticVersion("1.28.0"));
                        enabled = managedConditions.IsMatch;
                    }

                    if (!enabled) return;

                    ReplacedTexture replacement;
                    IAssetDataForImage assetImage = asset.AsImage();

                    if (info.Target.Contains("farmer_") && info.HDTexture != null)
                    {
                        replacement = new ReplacedTexture(assetImage.Data, info.HDTexture, info, info.HDTexture.Width, info.HDTexture.Height);
                        Color[] data = new Color[info.HDTexture.Width * info.HDTexture.Height];
                        info.HDTexture.GetData(data);
                        replacement.SetData(data);
                    }
                    else if (info.PixelReplacements.Count > 0)
                    {
                        replacement = new ReplacedTexture(assetImage.Data, info.HDTexture, info);
                        Color[] data = new Color[assetImage.Data.Width * assetImage.Data.Height];
                        assetImage.Data.GetData(data);
                        replacement.SetData(data);
                    }
                    else
                    {
                        replacement = new ReplacedTexture(assetImage.Data, info.HDTexture, info);
                    }

                    Monitor.Log($"Replacing Texture for {info.Target}", LogLevel.Trace);
                    asset.AsImage().ReplaceWith(replacement);
                });
            }
        }

        // Harmony patch for SpriteBatch.Draw
        public static bool DrawReplacedTexture(SpriteBatch __instance, Texture2D texture, Rectangle? destination, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            if (texture is ReplacedTexture rt && rt.NewTexture != null)
            {
                texture = rt.NewTexture;
            }
            return true; // continue original method
        }
    }
}
