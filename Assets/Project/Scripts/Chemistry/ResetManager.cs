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
        [Tooltip("The radius of the circle to arrange spawned atoms, preventing physics overlap.")]
        [SerializeField] private float _spawnRadius = 0.2f;

        [Tooltip("How high above the molecule's original position the atoms should spawn to drop gently.")]
        [SerializeField] private float _spawnHeightOffset = 0.1f;

        // Cache for quick lookup
        private Dictionary<ElementType, GameObject> _prefabDictionary;

        private void Awake()
        {
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

            int totalAtoms = 0;
            foreach (var requirement in molecule.Definition.RequiredAtoms)
            {
                totalAtoms += requirement.RequiredCount;
            }

            if (totalAtoms == 0) return;

            float angleStep = (Mathf.PI * 2f) / totalAtoms;
            int currentSpawnIndex = 0;

            foreach (var requirement in molecule.Definition.RequiredAtoms)
            {
                if (_prefabDictionary.TryGetValue(requirement.Element, out GameObject atomPrefab))
                {
                    for (int i = 0; i < requirement.RequiredCount; i++)
                    {
                        float angle = currentSpawnIndex * angleStep;
                        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * _spawnRadius;

                        Vector3 spawnPos = breakPosition + offset + (Vector3.up * _spawnHeightOffset);

                        GameObject newAtom = Instantiate(atomPrefab, spawnPos, Quaternion.identity);

                        if (newAtom.TryGetComponent<Rigidbody>(out Rigidbody rb))
                        {
                            rb.linearVelocity = Vector3.zero;
                            rb.angularVelocity = Vector3.zero;
                        }

                        currentSpawnIndex++;
                    }
                }
                else
                {
                    Debug.LogWarning($"<color=orange>[ResetManager]</color> Missing prefab mapping for element: {requirement.Element}");
                }
            }

            // 3. Destroy the molecule
            Destroy(molecule.gameObject);
            Debug.Log($"<color=green>[ResetManager]</color> Successfully broke apart: {moleculeName}");
        }
    }
}