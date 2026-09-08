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
    internal static class WeaponAuthoringWizard
    {
        private static readonly string[] WizardSteps =
        {
            "Identity",
            "Stats",
            "Attack",
            "Presentation",
            "Balance",
            "Review"
        };

        internal static void DrawCreateWizard(GameContentAuthoringSurfaceContext context, WeaponAuthoringState draft, WeaponProviderV2State state)
        {
            GameContentAuthoringValidationResult validation = WeaponAuthoringSession.ValidateDraft(draft);

            WeaponAuthoringFields.DrawHeader("New Weapon", draft.WeaponId, WeaponAuthoringSummary.BuildWeaponChips(draft, validation));
            GameContentAuthoringCommand command = GameContentAuthoringCommandBar.Draw(GameContentAuthoringWorkbenchMode.Create, validation.IsValid, true, "Create");
            if (command == GameContentAuthoringCommand.Create)
            {
                WeaponAuthoringSession.Create(context, draft, state);
            }

            state.WizardStep = DeucarianEditorWizardHeader.Draw(state.WizardStep, WizardSteps);
            GUILayout.Space(DeucarianEditorSpacing.Small);
            switch (Mathf.Clamp(state.WizardStep, 0, WizardSteps.Length - 1))
            {
                case 0:
                    WeaponAuthoringFields.DrawOverview(context, draft, null, true);
                    break;
                case 1:
                    WeaponAuthoringFields.DrawStats(context, draft);
                    break;
                case 2:
                    WeaponAuthoringFields.DrawAttack(context, draft);
                    break;
                case 3:
                    WeaponAuthoringFields.DrawPresentation(context, draft);
                    break;
                case 4:
                    WeaponAuthoringFields.DrawBalance(context, draft);
                    break;
                default:
                    DrawReview(context, draft, validation);
                    break;
            }

            GameContentAuthoringProviderGUI.DrawValidationIssues(
                validation,
                GameContentAuthoringValidationSummaryStyle.Counts);
            context.Authoring.DrawCreationResult();
        }

        private static void DrawReview(GameContentAuthoringSurfaceContext context, WeaponAuthoringState state, GameContentAuthoringValidationResult validation)
        {
            WeaponAuthoringFields.DrawSummaryRows(
                WeaponAuthoringSummary.Row("Folder", state.OutputRoot.TrimEnd('/') + "/" + WeaponAuthoringSummary.Sanitize(state.WeaponId)),
                WeaponAuthoringSummary.Row("Root Asset", WeaponAuthoringSummary.Sanitize(state.WeaponId) + "_WeaponDefinition.asset"),
                WeaponAuthoringSummary.Row("Sections", "Stats, Presentation"),
                WeaponAuthoringSummary.Row("Validation", validation.ErrorCount.ToString(CultureInfo.InvariantCulture) + " blocker(s), " + validation.WarningCount.ToString(CultureInfo.InvariantCulture) + " warning(s)"));
        }
    }
}
