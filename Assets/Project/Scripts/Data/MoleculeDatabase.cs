using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRChemistryLab.Data
{
    public enum ElementType
    {
        Hydrogen,
        Oxygen,
        Carbon,
        Nitrogen
    }

    [Serializable]
    public struct AtomRequirement
    {
        public ElementType Element;
        [Min(1)] public int RequiredCount;
    }

    [Serializable]
    public struct MoleculeDefinition
    {
        public string MoleculeName;      // e.g., "Water"
        public string Formula;           // e.g., "H2O"
        public string BondType;          // e.g., "Single Covalent"

        [Tooltip("List of required atoms to form this molecule (e.g., H=2, O=1).")]
        public List<AtomRequirement> RequiredAtoms;

        [Tooltip("The 3D prefab spawned upon successful bonding. Should include appropriate bond visuals.")]
        public GameObject MoleculePrefab;
    }

    [CreateAssetMenu(fileName = "MoleculeDatabase", menuName = "Chemistry Lab/Molecule Database")]
    public class MoleculeDatabase : ScriptableObject
    {
        [Tooltip("List of all valid molecules that can be formed in the VR lab.")]
        public List<MoleculeDefinition> ValidMolecules = new List<MoleculeDefinition>();
    }
}