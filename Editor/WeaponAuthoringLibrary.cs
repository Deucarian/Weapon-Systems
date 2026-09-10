using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Deucarian.Attacks.Authoring;
using Deucarian.Editor;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.WeaponSystems.Authoring;
using UnityEditor;
using UnityEngine;

namespace Deucarian.WeaponSystems.Editor
{
    internal static class WeaponAuthoringLibrary
    {
        internal static void DrawWeaponList(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state, IReadOnlyList<WeaponProviderV2ListItem> items)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DeucarianEditorTextGUI.LabelField("Weapons", DeucarianEditorStyles.SectionTitle);
                GUILayout.FlexibleSpace();
                if (DeucarianEditorMiniToolbar.Button("Refresh", true, GUILayout.Width(62f), GUILayout.Height(22f)))
                    context.RefreshLibrary();
            }

            state.SearchText = DeucarianEditorSearchField.Draw(state.SearchText, "Search weapons", GUILayout.ExpandWidth(true));
            if (DeucarianEditorButtons.Secondary("Create New", true, GUILayout.Height(24f)))
            {
                state.BeginCreate();
                context.ClearSelection();
                context.RequestRepaint();
            }

            GUILayout.Space(DeucarianEditorSpacing.Small);
            state.ListScroll = EditorGUILayout.BeginScrollView(state.ListScroll);
            int shown = 0;
            for (int i = 0; i < items.Count; i++)
            {
                WeaponProviderV2ListItem item = items[i];
                if (!item.Matches(state.SearchText))
                    continue;

                shown++;
                DrawWeaponCard(context, state, item);
            }

            if (shown == 0)
                DeucarianEditorTextGUI.LabelField(items.Count == 0 ? "No authored weapons found." : "No weapons match the current search.", DeucarianEditorStyles.MutedLabel);
            EditorGUILayout.EndScrollView();
        }

        private static void DrawWeaponCard(GameContentAuthoringSurfaceContext context, WeaponProviderV2State state, WeaponProviderV2ListItem item)
        {
            bool selected = !state.Creating && context.IsSelected(item.Source);
            var chips = new[]
            {
                new DeucarianEditorStatusChip(item.TypeLabel, DeucarianEditorStatus.Info),
                new DeucarianEditorStatusChip(item.ReadinessLabel, item.ReadinessStatus),
                new DeucarianEditorStatusChip(item.AttackLabel, item.HasAttack ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Error, item.AttackTooltip),
                new DeucarianEditorStatusChip(item.HasPrefab ? "Model" : "NoModel", item.HasPrefab ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Warning),
                new DeucarianEditorStatusChip(item.HasPresentation ? "VFX/Aud" : "Quiet", item.HasPresentation ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Disabled)
            };

            bool clicked = DeucarianEditorCompactObjectCard.Draw(
                item.DisplayName,
                item.StableId,
                selected,
                chips,
                () =>
                {
                    if (DeucarianEditorMiniToolbar.PingButton(item.Source.Asset))
                        GUI.FocusControl(null);
                },
                null,
                GUILayout.ExpandWidth(true));

            if (clicked && item.Source != null)
            {
                state.Creating = false;
                state.DetailScroll = Vector2.zero;
                context.SelectItem(item.Source);
                Event.current.Use();
            }
        }
    }
}
