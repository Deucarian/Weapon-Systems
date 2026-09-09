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
    internal sealed class WeaponProviderV2State : GameContentAuthoringProviderSessionState<WeaponAuthoringState>
    {
        public void BeginCreate()
        {
            Creating = true;
            DetailScroll = Vector2.zero;
            WizardStep = 0;
            ClearEditingState();
            PreviewStatus = "Previewing draft weapon";
        }
    }
}
