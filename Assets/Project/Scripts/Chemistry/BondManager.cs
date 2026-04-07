using System;
using System.Collections.Generic;
using UnityEngine;
using VRChemistryLab.Data;

namespace VRChemistryLab.Chemistry
{
    public class BondManager : MonoBehaviour
    {
        [Header("Database Reference")]
        [Tooltip("Reference to the Molecule Database ScriptableObject.")]
        [SerializeField] private MoleculeDatabase _moleculeDatabase;

        [Header("Bonding Settings")]
        [Tooltip("Maximum distance between atoms to consider them part of the same bonding group.")]
        [SerializeField] private float _bondingCheckRadius = 0.2f;

        [Tooltip("Cooldown between bond checks to prevent repeated processing.")]
        [SerializeField] private float _checkCooldown = 0.25f;

        [Header("Debug")]
        [Tooltip("If true, valid molecule matches will only log results and keep atoms in the scene.")]
        [SerializeField] private bool _debugModeWithoutPrefab = true;
        [SerializeField] private bool _destroyAtomsEvenWithoutPrefab = true;

        public static event Action<MoleculeDefinition, Vector3> OnMoleculeFormed;

        private float _lastCheckTime = -999f;
        private bool _isProcessingBond = false;

        public void TryBondFromAtoms(List<AtomController> candidateAtoms)
        {
            if (_isProcessingBond)
                return;

            if (Time.time - _lastCheckTime < _checkCooldown)
                return;

            if (_moleculeDatabase == null)
            {
                Debug.LogError("BondManager: Molecule Database is not assigned.");
                return;
            }

            if (candidateAtoms == null || candidateAtoms.Count < 2)
                return;

            _isProcessingBond = true;
            _lastCheckTime = Time.time;

            try
            {
                List<AtomController> validAtoms = GetValidAtoms(candidateAtoms);

                if (validAtoms.Count < 2)
                    return;

                List<AtomController> closeGroup = GetLargestCloseGroup(validAtoms);

                if (closeGroup.Count < 2)
                    return;

                Dictionary<ElementType, int> elementCounts = CountElements(closeGroup);

                if (!TryFindBestMatchingMolecule(elementCounts, out MoleculeDefinition matchedMolecule))
                {
                    Debug.Log($"<color=orange>[BondManager]</color> No valid molecule for group of {closeGroup.Count} atoms.");
                    return;
                }

                Vector3 spawnPosition = GetCenterPoint(closeGroup);
                FormMolecule(matchedMolecule, closeGroup, spawnPosition);
            }
            finally
            {
                _isProcessingBond = false;
            }
        }

        private List<AtomController> GetValidAtoms(List<AtomController> candidateAtoms)
        {
            HashSet<AtomController> uniqueAtoms = new HashSet<AtomController>();

            foreach (var atom in candidateAtoms)
            {
                if (atom == null)
                    continue;

                if (!atom.gameObject.activeInHierarchy)
                    continue;

                uniqueAtoms.Add(atom);
            }

            return new List<AtomController>(uniqueAtoms);
        }

        private List<AtomController> GetLargestCloseGroup(List<AtomController> atoms)
        {
            List<AtomController> bestGroup = new List<AtomController>();

            foreach (var seedAtom in atoms)
            {
                List<AtomController> group = new List<AtomController>();

                foreach (var otherAtom in atoms)
                {
                    float distance = Vector3.Distance(seedAtom.transform.position, otherAtom.transform.position);

                    if (distance <= _bondingCheckRadius)
                    {
                        group.Add(otherAtom);
                    }
                }

                if (group.Count > bestGroup.Count)
                {
                    bestGroup = group;
                }
            }

            return bestGroup;
        }

        private Dictionary<ElementType, int> CountElements(List<AtomController> atoms)
        {
            Dictionary<ElementType, int> counts = new Dictionary<ElementType, int>();

            foreach (var atom in atoms)
            {
                if (counts.ContainsKey(atom.ElementType))
                    counts[atom.ElementType]++;
                else
                    counts[atom.ElementType] = 1;
            }

            return counts;
        }

        private bool TryFindBestMatchingMolecule(
            Dictionary<ElementType, int> currentElements,
            out MoleculeDefinition matchedMolecule)
        {
            matchedMolecule = default;

            bool foundMatch = false;
            int bestMatchAtomCount = 0;

            foreach (var recipe in _moleculeDatabase.ValidMolecules)
            {
                Dictionary<ElementType, int> requiredElements = BuildRequiredElements(recipe);
                int totalRequiredAtoms = GetTotalRequiredAtomCount(recipe);

                if (!IsExactRecipeMatch(currentElements, requiredElements))
                    continue;

                if (totalRequiredAtoms > bestMatchAtomCount)
                {
                    bestMatchAtomCount = totalRequiredAtoms;
                    matchedMolecule = recipe;
                    foundMatch = true;
                }
            }

            return foundMatch;
        }

        private Dictionary<ElementType, int> BuildRequiredElements(MoleculeDefinition recipe)
        {
            Dictionary<ElementType, int> requiredElements = new Dictionary<ElementType, int>();

            foreach (var req in recipe.RequiredAtoms)
            {
                requiredElements[req.Element] = req.RequiredCount;
            }

            return requiredElements;
        }

        private int GetTotalRequiredAtomCount(MoleculeDefinition recipe)
        {
            int total = 0;

            foreach (var req in recipe.RequiredAtoms)
            {
                total += req.RequiredCount;
            }

            return total;
        }

        private bool IsExactRecipeMatch(
            Dictionary<ElementType, int> currentElements,
            Dictionary<ElementType, int> requiredElements)
        {
            if (currentElements.Count != requiredElements.Count)
                return false;

            foreach (var pair in requiredElements)
            {
                if (!currentElements.TryGetValue(pair.Key, out int currentCount))
                    return false;

                if (currentCount != pair.Value)
                    return false;
            }

            return true;
        }

        private Vector3 GetCenterPoint(List<AtomController> atoms)
        {
            Vector3 sum = Vector3.zero;

            foreach (var atom in atoms)
            {
                sum += atom.transform.position;
            }

            return sum / atoms.Count;
        }

        private void FormMolecule(MoleculeDefinition molecule, List<AtomController> usedAtoms, Vector3 spawnPosition)
        {
            Debug.Log($"<color=green>[BondManager]</color> Valid molecule found: {molecule.MoleculeName} ({molecule.Formula})");

            bool hasPrefab = molecule.MoleculePrefab != null;
            bool shouldDestroyAtoms = hasPrefab || _destroyAtomsEvenWithoutPrefab;

            if (!hasPrefab)
            {
                Debug.LogWarning($"BondManager: No prefab assigned for {molecule.MoleculeName}");

                if (_debugModeWithoutPrefab)
                {
                    Debug.Log("<color=cyan>[BondManager]</color> Debug mode active.");
                }
            }

            if (shouldDestroyAtoms)
            {
                foreach (var atom in usedAtoms)
                {
                    if (atom != null)
                        Destroy(atom.gameObject);
                }
            }

            if (hasPrefab)
            {
                GameObject spawnedMolecule = Instantiate(molecule.MoleculePrefab, spawnPosition, Quaternion.identity);

                if (spawnedMolecule.TryGetComponent<MoleculeController>(out var molController))
                {
                    molController.Initialize(molecule);
                }
                else
                {
                    Debug.LogWarning($"BondManager: {molecule.MoleculeName} prefab is missing a MoleculeController component!");
                }
            }

            OnMoleculeFormed?.Invoke(molecule, spawnPosition);
        }
    }
}