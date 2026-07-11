using System;
using System.Globalization;
using Deucarian.Editor;
using Deucarian.GameContentAuthoring.Editor;
using UnityEditor;
using UnityEngine;

namespace Deucarian.WeaponSystems.Editor
{
    public sealed class WeaponContentRecordProjection
    {
        public WeaponContentRecordProjection(
            GameContentRecordDescriptor record,
            bool isTower,
            string fireMode,
            float damage,
            float cooldownSeconds,
            float range,
            string targetingMode,
            string payloadRecordId,
            float areaRadius,
            string rankPathSummary,
            string mutationSummary,
            string evolutionSummary,
            string presentationSummary)
        {
            Record = record;
            IsTower = isTower;
            FireMode = fireMode ?? string.Empty;
            Damage = damage;
            CooldownSeconds = cooldownSeconds;
            Range = range;
            TargetingMode = targetingMode ?? string.Empty;
            PayloadRecordId = payloadRecordId ?? string.Empty;
            AreaRadius = areaRadius;
            RankPathSummary = rankPathSummary ?? string.Empty;
            MutationSummary = mutationSummary ?? string.Empty;
            EvolutionSummary = evolutionSummary ?? string.Empty;
            PresentationSummary = presentationSummary ?? string.Empty;
        }

        public GameContentRecordDescriptor Record { get; }
        public bool IsTower { get; }
        public string FireMode { get; }
        public float Damage { get; }
        public float CooldownSeconds { get; }
        public float Range { get; }
        public string TargetingMode { get; }
        public string PayloadRecordId { get; }
        public float AreaRadius { get; }
        public string RankPathSummary { get; }
        public string MutationSummary { get; }
        public string EvolutionSummary { get; }
        public string PresentationSummary { get; }
    }

    internal sealed class WeaponPackAwareLensState
    {
        public readonly GameContentRecordLensBrowserState Browser = new GameContentRecordLensBrowserState();
    }

    internal static class WeaponPackAwareLensView
    {
        public static void Draw(
            GameContentAuthoringSurfaceContext context,
            GameContentLensDescriptor lens,
            WeaponPackAwareLensState state)
        {
            GameContentRecordLensBrowser.Draw(
                context,
                lens,
                state.Browser,
                DrawDetails,
                DrawPreview);
        }

        private static void DrawDetails(GameContentRecordDescriptor record)
        {
            if (!GameContentRecordProjectionRegistry<WeaponContentRecordProjection>.TryProject(record, out WeaponContentRecordProjection projection))
            {
                EditorGUILayout.HelpBox("No installed adapter exposes common Weapon / Tower fields for this record.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField(projection.IsTower ? "Tower" : "Weapon", DeucarianEditorStyles.SectionTitle);
            GameContentRecordLensBrowser.DrawRow("Fire Mode", projection.FireMode);
            Row("Damage", projection.Damage);
            Row("Cooldown", projection.CooldownSeconds, "s");
            Row("Range", projection.Range);
            GameContentRecordLensBrowser.DrawRow("Targeting", Empty(projection.TargetingMode));
            GameContentRecordLensBrowser.DrawRow("Projectile / Payload", Empty(projection.PayloadRecordId));
            Row("Area", projection.AreaRadius);
            GameContentRecordLensBrowser.DrawRow("Rank Path", Empty(projection.RankPathSummary));
            GameContentRecordLensBrowser.DrawRow("Mutation", Empty(projection.MutationSummary));
            GameContentRecordLensBrowser.DrawRow("Evolution", Empty(projection.EvolutionSummary));
            GameContentRecordLensBrowser.DrawRow("Presentation", Empty(projection.PresentationSummary));
        }

        private static void DrawPreview(GameContentRecordDescriptor record)
        {
            EditorGUILayout.LabelField(record.DisplayName, DeucarianEditorStyles.SectionTitle);
            if (!GameContentRecordProjectionRegistry<WeaponContentRecordProjection>.TryProject(record, out WeaponContentRecordProjection projection))
            {
                EditorGUILayout.HelpBox("Preview adapter unavailable.", MessageType.Warning);
                return;
            }

            DeucarianEditorStatusBadge.Draw("Read-only pack record", DeucarianEditorStatus.Info, GUILayout.MinWidth(138f));
            GameContentRecordLensBrowser.DrawRow("Kind", projection.IsTower ? "Tower" : "Weapon");
            Row("Damage", projection.Damage);
            Row("Interval", projection.CooldownSeconds, "s");
            Row("Range", projection.Range);
            GameContentRecordLensBrowser.DrawRow("Payload", Empty(projection.PayloadRecordId));
            EditorGUILayout.HelpBox(
                string.IsNullOrWhiteSpace(projection.PresentationSummary)
                    ? "No prefab is assigned by this read-only source. The preview uses authored weapon values."
                    : projection.PresentationSummary,
                MessageType.Info);
        }

        private static void Row(string label, float value, string suffix = null)
        {
            GameContentRecordLensBrowser.DrawRow(label, value.ToString("0.###", CultureInfo.InvariantCulture) + (suffix ?? string.Empty));
        }

        private static string Empty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "None" : value;
        }
    }
}
