using System;
using System.Linq;
using Deucarian.Editor.Definitions;
using Deucarian.WeaponSystems.Authoring;
using Deucarian.Attacks.Authoring;
using UnityEditor;
using UnityEngine;

namespace Deucarian.WeaponSystems.Editor.Definitions
{
    [Serializable]
    public sealed class WeaponPresentationDefinitionSpec
    {
        [DefinitionField("_prefab")] public GameObject Prefab;
        [DefinitionField("_placementAudio")] public AudioClip PlacementAudio;
        [DefinitionField("_placementVfxPrefab")] public GameObject PlacementVfxPrefab;
    }
}
