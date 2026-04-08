using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRChemistryLab.Chemistry
{
    public class AtomSpawner : MonoBehaviour
    {
        [Header("Spawner Settings")]
        [Tooltip("The Atom Prefab to spawn (e.g., Hydrogen, Oxygen)")]
        [SerializeField] private GameObject _atomPrefab;

        [Tooltip("Time in seconds to wait before spawning a new atom after one is grabbed.")]
        [SerializeField] private float _spawnDelay = 5.0f;

        private GameObject _currentAtom;

        private void Start()
        {
            SpawnNewAtom();
        }

        private void SpawnNewAtom()
        {
            if (_atomPrefab == null)
            {
                Debug.LogWarning("<color=orange>[AtomSpawner]</color> Atom Prefab is missing!");
                return;
            }

            _currentAtom = Instantiate(_atomPrefab, transform.position, transform.rotation);

            var grabInteractable = _currentAtom.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
                grabInteractable.selectEntered.AddListener(OnAtomGrabbed);
        }

        private void OnAtomGrabbed(SelectEnterEventArgs args)
        {
            var grabInteractable = _currentAtom.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.selectEntered.RemoveListener(OnAtomGrabbed);
            }

            StartCoroutine(SpawnWithDelay());
        }

        private IEnumerator SpawnWithDelay()
        {
            yield return new WaitForSeconds(_spawnDelay);

            SpawnNewAtom();
        }
    }
}