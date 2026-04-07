using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRChemistryLab.Data;

namespace VRChemistryLab.Chemistry
{
    public class BondManager : MonoBehaviour
    {
        [Header("Database Reference")]
        [Tooltip("Reference to the Molecule Database ScriptableObject.")]
        [SerializeField] private MoleculeDatabase moleculeDatabase;

        [Header("Bonding Settings")]
        [Tooltip("The radius to search for nearby atoms when a collision occurs.")]
        [SerializeField] private float bondingCheckRadius = 0.3f;

        [Tooltip("Layer mask specifically for Atom objects to optimize Physics.OverlapSphere.")]
        [SerializeField] private LayerMask atomLayerMask;

        public static event Action<MoleculeDefinition, Vector3> OnMoleculeFormed;

        private float lastCheckTime = 0f;
        private const float CHECK_COOLDOWN = 0.5f;
        private bool isProcessingBond = false;

        private void OnEnable()
        {
            AtomController.OnProximityTriggered += HandleProximityTriggered;
        }

        private void OnDisable()
        {
            AtomController.OnProximityTriggered -= HandleProximityTriggered;
        }

        private void HandleProximityTriggered(AtomController atomA, AtomController atomB)
        {
            if (isProcessingBond)
                return;

            if (Time.time - lastCheckTime < CHECK_COOLDOWN)
                return;

            isProcessingBond = true;
            lastCheckTime = Time.time;

            try
            {
                if (moleculeDatabase == null)
                {
                    Debug.LogError("BondManager: Molecule Database is not assigned!");
                    return;
                }

                if (atomA == null || atomB == null)
                    return;

                Vector3 centerPoint = (atomA.transform.position + atomB.transform.position) / 2f;
                List<AtomController> nearbyAtoms = GetAtomsInRadius(centerPoint, bondingCheckRadius);
                Dictionary<ElementType, int> elementCounts = CountElements(nearbyAtoms);

                if (TryFindMatchingMolecule(elementCounts, out MoleculeDefinition matchedMolecule))
                {
                    FormMolecule(matchedMolecule, nearbyAtoms, centerPoint);
                }
            }
            finally
            {
                isProcessingBond = false;
            }
        }

        private List<AtomController> GetAtomsInRadius(Vector3 center, float radius)
        {
            HashSet<AtomController> foundAtoms = new HashSet<AtomController>();
            Collider[] colliders = Physics.OverlapSphere(center, radius, atomLayerMask);

            foreach (var col in colliders)
            {
                if (col.TryGetComponent<AtomController>(out var atom))
                {
                    foundAtoms.Add(atom);
                }
            }

            return foundAtoms.ToList();
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

        private bool TryFindMatchingMolecule(Dictionary<ElementType, int> currentElements,out MoleculeDefinition matchedMolecule)
        {
            matchedMolecule = default;

            bool foundMatch = false;
            int bestMatchAtomCount = 0;

            foreach (var recipe in moleculeDatabase.ValidMolecules)
            {
                Dictionary<ElementType, int> requiredElements = BuildRequiredElements(recipe);
                int totalRequiredAtoms = GetTotalRequiredAtomCount(recipe);

                if (!IsRecipeSatisfied(currentElements, requiredElements))
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

        private bool IsRecipeSatisfied(Dictionary<ElementType, int> currentElements,Dictionary<ElementType, int> requiredElements)
        {
            foreach (var pair in requiredElements)
            {
                if (!currentElements.TryGetValue(pair.Key, out int currentCount))
                    return false;

                if (currentCount < pair.Value)
                    return false;
            }

            return true;
        }

        private void FormMolecule(MoleculeDefinition molecule, List<AtomController> usedAtoms, Vector3 spawnPosition)
        {
            foreach (var atom in usedAtoms)
                Destroy(atom.gameObject);

            if (molecule.MoleculePrefab != null)
                Instantiate(molecule.MoleculePrefab, spawnPosition, Quaternion.identity);
            else
                Debug.LogWarning($"BondManager: No prefab assigned for {molecule.MoleculeName}");

            OnMoleculeFormed?.Invoke(molecule, spawnPosition);

            Debug.Log($"Successfully formed: {molecule.MoleculeName} ({molecule.Formula})");
        }
    }
}