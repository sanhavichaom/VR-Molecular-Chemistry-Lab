using System.Collections.Generic;
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
        [SerializeField] private Transform _inspectorCanvasTransform;

        [Tooltip("Assign the BackgroundPanel containing the Canvas Group here.")]
        [SerializeField] private CanvasGroup _inspectorPanelGroup;

        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _formulaText;
        [SerializeField] private TextMeshProUGUI _bondInfoText;

        [Header("Inspector Placement Settings")]
        [Tooltip("Distance from the player's headset to spawn the UI.")]
        [SerializeField] private float _distanceFromPlayer = 1.2f;

        [Tooltip("Vertical offset to place the UI slightly below eye level.")]
        [SerializeField] private float _verticalOffset = -0.1f;

        [Tooltip("How smoothly the UI follows the player's gaze. Higher value = faster follow.")]
        [SerializeField] private float _followSpeed = 8.0f;

        [Header("Molecule Library Panel")]
        [Tooltip("The Text component on the wall/board to display the list of discovered molecules.")]
        [SerializeField] private TextMeshProUGUI _libraryListText;

        [Tooltip("The text showing the total count (e.g., 'Discovered: 1 / 7').")]
        [SerializeField] private TextMeshProUGUI _discoveryCountText;

        [Tooltip("Total number of required molecules to find (at least 7 based on the assessment).")]
        [SerializeField] private int _totalMoleculesToFind = 7;

        private bool _isShowingInfo = false;

        private HashSet<string> _discoveredMolecules = new HashSet<string>();

        private void OnEnable()
        {
            // Subscribe to events
            MoleculeController.OnMoleculeExamined += ShowMoleculeInfo;
            MoleculeController.OnMoleculeReleased += HideMoleculeInfo;

            // Listen for newly formed molecules
            BondManager.OnMoleculeFormed += HandleMoleculeFormed;
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            MoleculeController.OnMoleculeExamined -= ShowMoleculeInfo;
            MoleculeController.OnMoleculeReleased -= HideMoleculeInfo;
            BondManager.OnMoleculeFormed -= HandleMoleculeFormed;
        }

        private void Start()
        {
            HideMoleculeInfo();
            UpdateLibraryUI();
        }

        private void LateUpdate()
        {
            if (_isShowingInfo)
            {
                UpdatePanelPosition();
            }
        }

        private void HandleMoleculeFormed(MoleculeDefinition definition, Vector3 spawnPosition)
        {
            if (!_discoveredMolecules.Contains(definition.MoleculeName))
            {
                _discoveredMolecules.Add(definition.MoleculeName);
                Debug.Log($"<color=green>[UIManager]</color> New Discovery: {definition.MoleculeName}");

                UpdateLibraryUI();
            }
        }

        private void UpdateLibraryUI()
        {
            if (_libraryListText == null || _discoveryCountText == null) return;

            _discoveryCountText.text = $"Discovered: {_discoveredMolecules.Count} / {_totalMoleculesToFind}";

            if (_discoveredMolecules.Count == 0)
            {
                _libraryListText.text = "No molecules discovered yet.\n\nTry combining atoms!";
                return;
            }

            string listContent = "";
            foreach (string moleculeName in _discoveredMolecules)
            {
                listContent += $"• {moleculeName}\n";
            }

            _libraryListText.text = listContent;
        }

        private void ShowMoleculeInfo(MoleculeDefinition definition)
        {
            if (_inspectorPanelGroup == null) return;

            _nameText.text = $"Name: {definition.MoleculeName}";
            _formulaText.text = $"Formula: {definition.Formula}";
            _bondInfoText.text = $"Bond: {definition.BondType}";

            _isShowingInfo = true;
            SnapPanelPosition();

            _inspectorPanelGroup.alpha = 1f;
            _inspectorPanelGroup.interactable = true;
            _inspectorPanelGroup.blocksRaycasts = true;
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
            if (_cameraTransform == null || _inspectorCanvasTransform == null) return;

            Vector3 targetPosition = _cameraTransform.position + (_cameraTransform.forward * _distanceFromPlayer);
            targetPosition.y = _cameraTransform.position.y + _verticalOffset;

            _inspectorCanvasTransform.position = Vector3.Lerp(_inspectorCanvasTransform.position, targetPosition, Time.deltaTime * _followSpeed);

            Quaternion targetRotation = Quaternion.LookRotation(_inspectorCanvasTransform.position - _cameraTransform.position);
            _inspectorCanvasTransform.rotation = Quaternion.Slerp(_inspectorCanvasTransform.rotation, targetRotation, Time.deltaTime * _followSpeed);
        }

        private void SnapPanelPosition()
        {
            if (_cameraTransform == null || _inspectorCanvasTransform == null) return;

            Vector3 targetPosition = _cameraTransform.position + (_cameraTransform.forward * _distanceFromPlayer);
            targetPosition.y = _cameraTransform.position.y + _verticalOffset;

            _inspectorCanvasTransform.position = targetPosition;
            _inspectorCanvasTransform.rotation = Quaternion.LookRotation(_inspectorCanvasTransform.position - _cameraTransform.position);
        }
    }
}