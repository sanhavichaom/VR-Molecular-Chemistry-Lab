using System.Collections.Generic;
using UnityEngine;

namespace VRChemistryLab.Chemistry
{
    public class ResetZone : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ResetManager _resetManager;

        [Header("Settings")]
        [Tooltip("How long the molecule must remain unchanged in the zone before breaking apart.")]
        [SerializeField] private float _stabilityTime = 1.0f;

        private readonly List<MoleculeController> _moleculesInZone = new List<MoleculeController>();
        private float _nextCheckTime = -1f;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<MoleculeController>(out var molecule))
                return;

            if (_moleculesInZone.Contains(molecule))
                return;

            _moleculesInZone.Add(molecule);
            Debug.Log($"<color=magenta>[ResetZone]</color> Enter: {molecule.name}. Total in zone: {_moleculesInZone.Count}");

            ResetStabilityTimer();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<MoleculeController>(out var molecule))
                return;

            if (_moleculesInZone.Remove(molecule))
            {
                Debug.Log($"<color=magenta>[ResetZone]</color> Exit: {molecule.name}. Total in zone: {_moleculesInZone.Count}");
                ResetStabilityTimer();
            }
        }

        private void Update()
        {
            CleanupNullMolecules();

            // If timer is inactive, do nothing
            if (_nextCheckTime < 0f)
                return;

            // Wait until the stability time has passed
            if (Time.time < _nextCheckTime)
                return;

            _nextCheckTime = -1f;
            TryBreakApart();
        }

        private void ResetStabilityTimer()
        {
            _nextCheckTime = Time.time + _stabilityTime;
        }

        private void CleanupNullMolecules()
        {
            bool removedAny = false;
            for (int i = _moleculesInZone.Count - 1; i >= 0; i--)
            {
                if (_moleculesInZone[i] == null)
                {
                    _moleculesInZone.RemoveAt(i);
                    removedAny = true;
                }
            }

            if (removedAny) ResetStabilityTimer();
        }

        private void TryBreakApart()
        {
            if (_resetManager == null)
            {
                Debug.LogError("ResetZone: ResetManager reference is missing.");
                return;
            }

            // Rule: Only process if exactly ONE molecule is in the zone
            if (_moleculesInZone.Count == 0)
                return;

            if (_moleculesInZone.Count > 1)
            {
                Debug.Log($"<color=orange>[ResetZone]</color> Too many molecules in zone ({_moleculesInZone.Count}). Please leave only 1 to break apart.");
                // Reset timer so it keeps checking until the user removes the extra ones
                ResetStabilityTimer();
                return;
            }

            Debug.Log($"<color=cyan>[ResetZone]</color> Zone stable. Breaking apart molecule.");

            // Break apart the single molecule
            MoleculeController targetMolecule = _moleculesInZone[0];
            _moleculesInZone.Clear(); // Clear the list since it's about to be destroyed
            _resetManager.BreakApartMolecule(targetMolecule);
        }
    }
}