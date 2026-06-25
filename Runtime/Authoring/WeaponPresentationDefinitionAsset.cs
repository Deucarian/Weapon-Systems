using UnityEngine;

namespace Deucarian.WeaponSystems.Authoring
{
    public sealed class WeaponPresentationDefinitionAsset : ScriptableObject
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private AudioClip _placementAudio;
        [SerializeField] private GameObject _placementVfxPrefab;

        public GameObject Prefab => _prefab;
        public AudioClip PlacementAudio => _placementAudio;
        public GameObject PlacementVfxPrefab => _placementVfxPrefab;

        public void Configure(GameObject prefab, AudioClip placementAudio, GameObject placementVfxPrefab)
        {
            _prefab = prefab;
            _placementAudio = placementAudio;
            _placementVfxPrefab = placementVfxPrefab;
        }
    }
}
