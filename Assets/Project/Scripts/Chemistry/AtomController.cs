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
        [SerializeField] private ElementType elementType;

        [Header("Visual Feedback")]
        [Tooltip("Material to apply when the user hovers over or grabs the atom.")]
        [SerializeField] private Material highlightMaterial;

        private Material originalMaterial;
        private MeshRenderer meshRenderer;
        private XRGrabInteractable grabInteractable;

        public ElementType ElementType => elementType;
        public bool IsGrabbed => grabInteractable != null && grabInteractable.isSelected;

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
            if (grabInteractable == null)
                return;

            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
            grabInteractable.hoverEntered.AddListener(OnHovered);
            grabInteractable.hoverExited.AddListener(OnUnhovered);
        }

        private void OnDisable()
        {
            if (grabInteractable == null)
                return;

            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
            grabInteractable.hoverEntered.RemoveListener(OnHovered);
            grabInteractable.hoverExited.RemoveListener(OnUnhovered);
        }

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            Debug.Log($"<color=cyan>[XR]</color> Grabbed: {gameObject.name} ({elementType})");
            SetHighlight(true);
        }

        private void OnReleased(SelectExitEventArgs args)
        {
            Debug.Log($"<color=yellow>[XR]</color> Released: {gameObject.name} ({elementType})");
            SetHighlight(false);
        }

        private void OnHovered(HoverEnterEventArgs args)
        {
            if (!IsGrabbed)
                SetHighlight(true);
        }

        private void OnUnhovered(HoverExitEventArgs args)
        {
            if (!IsGrabbed)
                SetHighlight(false);
        }

        private void SetHighlight(bool isHighlighted)
        {
            if (meshRenderer == null || highlightMaterial == null || originalMaterial == null)
                return;

            meshRenderer.material = isHighlighted ? highlightMaterial : originalMaterial;
        }
    }
}