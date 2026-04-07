using System.Collections.Generic;
using UnityEngine;

namespace VRChemistryLab.Chemistry
{
    public class BondingZone : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BondManager _bondManager;

        [Header("Settings")]
        [Tooltip("How long the bonding zone must remain unchanged before checking for a valid molecule.")]
        [SerializeField] private float _stabilityTime = 0.5f;

        private readonly List<AtomController> _atomsInZone = new List<AtomController>();
        private float _nextCheckTime = -1f;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<AtomController>(out var atom))
                return;

            if (_atomsInZone.Contains(atom))
                return;

            _atomsInZone.Add(atom);
            Debug.Log($"<color=magenta>[BondingZone]</color> Enter: {atom.name}");

            ResetStabilityTimer();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<AtomController>(out var atom))
                return;

            if (_atomsInZone.Remove(atom))
            {
                Debug.Log($"<color=magenta>[BondingZone]</color> Exit: {atom.name}");

                ResetStabilityTimer();
            }
        }

        private void Update()
        {
            CleanupNullAtoms();

            if (_nextCheckTime < 0f)
                return;

            if (Time.time < _nextCheckTime)
                return;

            _nextCheckTime = -1f;
            TryCheckBonding();
        }

        private void ResetStabilityTimer()
        {
            _nextCheckTime = Time.time + _stabilityTime;
        }

        private void CleanupNullAtoms()
        {
            for (int i = _atomsInZone.Count - 1; i >= 0; i--)
            {
                if (_atomsInZone[i] == null)
                {
                    _atomsInZone.RemoveAt(i);
                    ResetStabilityTimer();
                }
            }
        }

        private void TryCheckBonding()
        {
            if (_bondManager == null)
            {
                Debug.LogError("BondingZone: BondManager reference is missing.");
                return;
            }

            if (_atomsInZone.Count < 2)
                return;

            Debug.Log($"<color=cyan>[BondingZone]</color> Zone stable. Checking {_atomsInZone.Count} atoms for bonding.");
            _bondManager.TryBondFromAtoms(_atomsInZone);
        }
    }
}