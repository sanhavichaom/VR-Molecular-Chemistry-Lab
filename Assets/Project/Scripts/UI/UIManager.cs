using TMPro;
using UnityEngine;
using VRChemistryLab.Chemistry;
using VRChemistryLab.Data;

namespace VRChemistryLab.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Assign the Main Camera (from under XR Origin) to track where the player is looking.")]
        [SerializeField] private Transform _cameraTransform;

        [Header("Molecule Inspector Panel")]
        [Tooltip("Assign the MoleculeInspectorCanvas (root canvas) here.")]
        [SerializeField] private Transform _canvasTransform;

        [Tooltip("Assign the BackgroundPanel containing the Canvas Group here.")]
        [SerializeField] private CanvasGroup _inspectorPanelGroup;

        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _formulaText;
        [SerializeField] private TextMeshProUGUI _bondInfoText;

        [Header("Placement Settings")]
        [Tooltip("Distance from the player's headset to spawn the UI.")]
        [SerializeField] private float _distanceFromPlayer = 1.2f;

        [Tooltip("Vertical offset to place the UI slightly below eye level.")]
        [SerializeField] private float _verticalOffset = -0.1f;

        [Tooltip("How smoothly the UI follows the player's gaze. Higher value = faster follow.")]
        [SerializeField] private float _followSpeed = 8.0f;

        private bool _isShowingInfo = false;

        private void OnEnable()
        {
            // Subscribe to the molecule events
            MoleculeController.OnMoleculeExamined += ShowMoleculeInfo;
            MoleculeController.OnMoleculeReleased += HideMoleculeInfo;
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            MoleculeController.OnMoleculeExamined -= ShowMoleculeInfo;
            MoleculeController.OnMoleculeReleased -= HideMoleculeInfo;
        }

        private void Start()
        {
            HideMoleculeInfo();
        }

        private void LateUpdate()
        {
            if (_isShowingInfo)
                UpdatePanelPosition();
        }

        private void ShowMoleculeInfo(MoleculeDefinition definition)
        {
            if (_inspectorPanelGroup == null) return;

            if (_nameText != null)
                _nameText.text = $"Name: {definition.MoleculeName}";
            if (_formulaText != null)
                _formulaText.text = $"Formula: {definition.Formula}";
            if (_bondInfoText != null)
                _bondInfoText.text = $"Bond: {definition.BondType}";

            _isShowingInfo = true;

            SnapPanelPosition();

            if (_inspectorPanelGroup != null)
            {
                _inspectorPanelGroup.alpha = 1f;
                _inspectorPanelGroup.interactable = true;
                _inspectorPanelGroup.blocksRaycasts = true;
            }
        }

        private void HideMoleculeInfo()
        {
            if (_inspectorPanelGroup == null) return;

            _isShowingInfo = false;

            _inspectorPanelGroup.alpha = 0f;
            _inspectorPanelGroup.interactable = false;
            _inspectorPanelGroup.blocksRaycasts = false;
        }

        private void UpdatePanelPosition()
        {
            if (_cameraTransform == null || _canvasTransform == null) return;

            Vector3 targetPosition = _cameraTransform.position + (_cameraTransform.forward * _distanceFromPlayer);

            targetPosition.y = _cameraTransform.position.y + _verticalOffset;

            _canvasTransform.position = Vector3.Lerp(_canvasTransform.position, targetPosition, Time.deltaTime * _followSpeed);

            Quaternion targetRotation = Quaternion.LookRotation(_canvasTransform.position - _cameraTransform.position);
            _canvasTransform.rotation = Quaternion.Slerp(_canvasTransform.rotation, targetRotation, Time.deltaTime * _followSpeed);
        }

        private void SnapPanelPosition()
        {
            if (_cameraTransform == null || _canvasTransform == null) return;

            Vector3 targetPosition = _cameraTransform.position + (_cameraTransform.forward * _distanceFromPlayer);
            targetPosition.y = _cameraTransform.position.y + _verticalOffset;

            _canvasTransform.position = targetPosition;
            _canvasTransform.rotation = Quaternion.LookRotation(_canvasTransform.position - _cameraTransform.position);
        }
    }
}