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
            if (Time.time - lastCheckTime < CHECK_COOLDOWN) return;
            lastCheckTime = Time.time;

            if (moleculeDatabase == null)
            {
                Debug.LogError("BondManager: Molecule Database is not assigned!");
                return;
            }

            Vector3 centerPoint = (atomA.transform.position + atomB.transform.position) / 2f;

            List<AtomController> nearbyAtoms = GetAtomsInRadius(centerPoint, bondingCheckRadius);

            // Tally up the elements found in the cluster
            Dictionary<ElementType, int> elementCounts = CountElements(nearbyAtoms);

            // Check if this cluster matches any molecule recipe in the database
            if (TryFindMatchingMolecule(elementCounts, out MoleculeDefinition matchedMolecule))
            {
                FormMolecule(matchedMolecule, nearbyAtoms, centerPoint);
            }
        }

        private List<AtomController> GetAtomsInRadius(Vector3 center, float radius)
        {
            List<AtomController> foundAtoms = new List<AtomController>();
            Collider[] colliders = Physics.OverlapSphere(center, radius, atomLayerMask);

            foreach (var col in colliders)
            {
                if (col.TryGetComponent<AtomController>(out var atom))
                {
                    foundAtoms.Add(atom);
                }
            }
            return foundAtoms;
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

        private bool TryFindMatchingMolecule(Dictionary<ElementType, int> currentElements, out MoleculeDefinition matchedMolecule)
        {
            matchedMolecule = default;

            foreach (var recipe in moleculeDatabase.ValidMolecules)
            {
                // Create a temporary dictionary for the recipe requirements
                Dictionary<ElementType, int> requiredElements = new Dictionary<ElementType, int>();
                foreach (var req in recipe.RequiredAtoms)
                {
                    requiredElements[req.Element] = req.RequiredCount;
                }

                // Check if current elements exactly match the required elements (both type and quantity)
                bool isMatch = requiredElements.Count == currentElements.Count && !requiredElements.Except(currentElements).Any();

                if (isMatch)
                {
                    matchedMolecule = recipe;
                    return true;
                }
            }
            return false;
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