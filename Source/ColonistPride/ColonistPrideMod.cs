using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonistPride
{
    public class ColonistPrideMod : Mod
    {
        public static ColonistPrideSettings Settings;

        private Vector2 scrollPos;
        private float viewHeight = 700f;
        private readonly Dictionary<string, string> buffers = new Dictionary<string, string>();

        public ColonistPrideMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ColonistPrideSettings>();
            Settings.Validate();

            new Harmony("yourname.colonistpride").PatchAll();
        }

        public override string SettingsCategory() => "Colonist Pride";

        public override void WriteSettings()
        {
            base.WriteSettings();
            ColonistPrideUtility.ApplyStackLimits();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var viewRect = new Rect(0f, 0f, inRect.width - 20f, viewHeight);
            Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);

            float y = 0f;
            float w = viewRect.width;

            Widgets.CheckboxLabeled(new Rect(0f, y, w, 24f), "Count constructed items (buildings)", ref Settings.trackConstructedItems);
            y += 36f;

            y = DrawReactionGrid(ReactionType.Pride, "Pride (item created)", y, w);
            y = DrawReactionGrid(ReactionType.Heartbreak, "Heartbreak (item destroyed)", y, w);
            y = DrawReactionGrid(ReactionType.Relief, "Relief (low-quality item destroyed)", y, w);

            if (Widgets.ButtonText(new Rect(0f, y, 200f, 30f), "Reset to defaults"))
            {
                Settings.ResetToDefaults();
                buffers.Clear();
            }
            y += 40f;

            viewHeight = y;
            Widgets.EndScrollView();
        }

        private float DrawReactionGrid(ReactionType type, string title, float y, float width)
        {
            ReactionSettings rs = Settings.Get(type);
            QualityCategory[] qualities = ColonistPrideUtility.TiersFor(type);

            const float colTier = 0f, colImpact = 200f, colDuration = 340f;
            const float fieldW = 100f, rowH = 28f;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, y, width, 32f), title);
            Text.Font = GameFont.Small;
            y += 38f;

            Widgets.Label(new Rect(colTier, y, 190f, rowH), "Quality tier");
            Widgets.Label(new Rect(colImpact, y, 130f, rowH), "Mood impact");
            Widgets.Label(new Rect(colDuration, y, 130f, rowH), "Duration (days)");
            y += rowH;

            float magMin = type == ReactionType.Heartbreak ? -50f : 0f;
            float magMax = type == ReactionType.Heartbreak ? 0f : 50f;

            for (int i = 0; i < qualities.Length; i++)
            {
                TierSetting tier = rs.tiers[i];
                Widgets.Label(new Rect(colTier, y, 190f, rowH), qualities[i].GetLabel().CapitalizeFirst());
                NumField(new Rect(colImpact, y, fieldW, rowH - 2f), ref tier.magnitude, type + "_mag_" + i, magMin, magMax);
                NumField(new Rect(colDuration, y, fieldW, rowH - 2f), ref tier.durationDays, type + "_dur_" + i, 0.1f, 60f);
                y += rowH;
            }

            y += 4f;
            Widgets.Label(new Rect(colTier, y, 190f, rowH), "Max stack");
            NumField(new Rect(colImpact, y, fieldW, rowH - 2f), ref rs.maxStack, type + "_stack", 1f, 20f);
            y += rowH + 20f;

            return y;
        }

        private void NumField<T>(Rect rect, ref T value, string key, float min, float max) where T : struct
        {
            buffers.TryGetValue(key, out string buffer);
            Widgets.TextFieldNumeric(rect, ref value, ref buffer, min, max);
            buffers[key] = buffer;
        }
    }
}