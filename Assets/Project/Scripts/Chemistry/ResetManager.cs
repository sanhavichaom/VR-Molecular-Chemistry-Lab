using System;
using System.Collections.Generic;
using UnityEngine;
using VRChemistryLab.Data;

namespace VRChemistryLab.Chemistry
{
    [Serializable]
    public struct AtomPrefabMapping
    {
        public ElementType Element;
        public GameObject Prefab;
    }

    public class ResetManager : MonoBehaviour
    {
        [Header("Atom Prefab References")]
        [Tooltip("Map each ElementType to its corresponding Atom Prefab so the manager knows what to spawn.")]
        [SerializeField] private List<AtomPrefabMapping> _atomPrefabs = new List<AtomPrefabMapping>();

        [Header("Spawn Settings")]
        [Tooltip("Slight random offset so spawned atoms don't overlap perfectly and explode due to physics.")]
        [SerializeField] private float _spawnOffsetRadius = 0.15f;

        // Cache for quick lookup
        private Dictionary<ElementType, GameObject> _prefabDictionary;

        private void Awake()
        {
            // Convert list to dictionary for faster lookups when spawning
            _prefabDictionary = new Dictionary<ElementType, GameObject>();
            foreach (var mapping in _atomPrefabs)
            {
                if (!_prefabDictionary.ContainsKey(mapping.Element))
                {
                    _prefabDictionary.Add(mapping.Element, mapping.Prefab);
                }
            }
        }

        public void BreakApartMolecule(MoleculeController molecule)
        {
            if (molecule == null || molecule.Definition.RequiredAtoms == null) return;

            Vector3 breakPosition = molecule.transform.position;
            string moleculeName = molecule.Definition.MoleculeName;

            // 1. Spawn the required atoms back into the world
            foreach (var requirement in molecule.Definition.RequiredAtoms)
            {
                if (_prefabDictionary.TryGetValue(requirement.Element, out GameObject atomPrefab))
                {
                    for (int i = 0; i < requirement.RequiredCount; i++)
                    {
                        // Add a small random offset so they don't spawn inside each other
                        Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * _spawnOffsetRadius;
                        Instantiate(atomPrefab, breakPosition + randomOffset, Quaternion.identity);
                    }
                }
                else
                {
                    Debug.LogWarning($"<color=orange>[ResetManager]</color> Missing prefab mapping for element: {requirement.Element}");
                }
            }

            // 2. Destroy the molecule
            Destroy(molecule.gameObject);
            Debug.Log($"<color=green>[ResetManager]</color> Successfully broke apart: {moleculeName}");
        }
    }
}