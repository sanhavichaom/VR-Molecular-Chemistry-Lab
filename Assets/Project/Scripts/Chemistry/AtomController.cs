using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using VRChemistryLab.Data;

namespace VRChemistryLab.Chemistry
{
    [RequireComponent(typeof(XRGrabInteractable))]
    [RequireComponent(typeof(Rigidbody))]
    public class AtomController : MonoBehaviour
    {
        [Header("Atom Properties")]
        [Tooltip("The specific chemical element this object represents.")]
        public ElementType ElementType;

        [Header("Visual Feedback")]
        [Tooltip("Material to apply when the user hovers over or grabs the atom.")]
        [SerializeField] private Material highlightMaterial;

        private Material originalMaterial;
        private MeshRenderer meshRenderer;
        private XRGrabInteractable grabInteractable;

        public static event Action<AtomController, AtomController> OnProximityTriggered;

        private void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
            meshRenderer = GetComponentInChildren<MeshRenderer>();

            if (meshRenderer != null)
            {
                originalMaterial = meshRenderer.material;
            }
        }

        private void OnEnable()
        {
            // Subscribe to XR Interaction Toolkit events
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
            grabInteractable.hoverEntered.AddListener(OnHovered);
            grabInteractable.hoverExited.AddListener(OnUnhovered);
        }

        private void OnDisable()
        {
            // Always unsubscribe to prevent memory leaks
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
            grabInteractable.hoverEntered.RemoveListener(OnHovered);
            grabInteractable.hoverExited.RemoveListener(OnUnhovered);
        }

        #region XR Interaction Callbacks

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            SetHighlight(true);
        }

        private void OnReleased(SelectExitEventArgs args)
        {
            SetHighlight(false);
        }

        private void OnHovered(HoverEnterEventArgs args)
        {
            if (!grabInteractable.isSelected)
                SetHighlight(true);
        }

        private void OnUnhovered(HoverExitEventArgs args)
        {
            if (!grabInteractable.isSelected)
                SetHighlight(false);
        }

        private void SetHighlight(bool isHighlighted)
        {
            if (meshRenderer != null && highlightMaterial != null)
            {
                meshRenderer.material = isHighlighted ? highlightMaterial : originalMaterial;
            }
        }

        #endregion

        #region Bonding Logic

        private void OnTriggerEnter(Collider other)
        {
            // Check if we collided with another atom
            if (other.TryGetComponent<AtomController>(out var otherAtom))
            {
                // Only trigger the bonding check if THIS atom is currently being held by the player.
                // This prevents physics jitter from triggering random bonds across the table.
                if (grabInteractable.isSelected)
                {
                    OnProximityTriggered?.Invoke(this, otherAtom);
                }
            }
        }

        #endregion
    }
}