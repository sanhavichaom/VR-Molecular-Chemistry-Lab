using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using VRChemistryLab.Data;

namespace VRChemistryLab.Chemistry
{
    [RequireComponent(typeof(XRGrabInteractable))]
    [RequireComponent(typeof(Rigidbody))]
    public class MoleculeController : MonoBehaviour
    {
        [Header("Visual Feedback")]
        [Tooltip("Material to apply when the user hovers over or grabs the molecule.")]
        [SerializeField] private Material _highlightMaterial;

        public MoleculeDefinition Definition { get; private set; }

        private Material[] _originalMaterials;
        private MeshRenderer[] _meshRenderers;
        private XRGrabInteractable _grabInteractable;

        public static event Action<MoleculeDefinition> OnMoleculeExamined;
        public static event Action OnMoleculeReleased;

        public bool IsGrabbed => _grabInteractable != null && _grabInteractable.isSelected;

        private void Awake()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();

            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
            _originalMaterials = new Material[_meshRenderers.Length];

            for (int i = 0; i < _meshRenderers.Length; i++)
            {
                _originalMaterials[i] = _meshRenderers[i].material;
            }
        }

        private void OnEnable()
        {
            if (_grabInteractable == null) return;

            _grabInteractable.selectEntered.AddListener(OnGrabbed);
            _grabInteractable.selectExited.AddListener(OnReleased);
            _grabInteractable.hoverEntered.AddListener(OnHovered);
            _grabInteractable.hoverExited.AddListener(OnUnhovered);
        }

        private void OnDisable()
        {
            if (_grabInteractable == null) return;

            _grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            _grabInteractable.selectExited.RemoveListener(OnReleased);
            _grabInteractable.hoverEntered.RemoveListener(OnHovered);
            _grabInteractable.hoverExited.RemoveListener(OnUnhovered);
        }

        public void Initialize(MoleculeDefinition definition)
        {
            Definition = definition;
            Debug.Log($"<color=green>[MoleculeController]</color> Initialized: {Definition.MoleculeName} ({Definition.Formula})");
        }

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            SetHighlight(true);
            OnMoleculeExamined?.Invoke(Definition);
        }

        private void OnReleased(SelectExitEventArgs args)
        {
            SetHighlight(false);
            OnMoleculeReleased?.Invoke();
        }

        private void OnHovered(HoverEnterEventArgs args)
        {
            if (!IsGrabbed) SetHighlight(true);
        }

        private void OnUnhovered(HoverExitEventArgs args)
        {
            if (!IsGrabbed) SetHighlight(false);
        }

        private void SetHighlight(bool isHighlighted)
        {
            if (_highlightMaterial == null || _meshRenderers.Length == 0) return;

            for (int i = 0; i < _meshRenderers.Length; i++)
            {
                _meshRenderers[i].material = isHighlighted ? _highlightMaterial : _originalMaterials[i];
            }
        }
    }
}