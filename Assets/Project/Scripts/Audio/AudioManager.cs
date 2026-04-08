using UnityEngine;
using VRChemistryLab.Chemistry;
using VRChemistryLab.Data;

namespace VRChemistryLab.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Clips")]
        [Tooltip("The sound to play when atoms successfully combine into a molecule.")]
        [SerializeField] private AudioClip _bondSuccessClip;

        // The AudioSource component attached to this GameObject
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();

            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;
        }

        private void OnEnable()
        {
            // Subscribe to the molecule formation event
            BondManager.OnMoleculeFormed += PlayBondSuccessSound;
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            BondManager.OnMoleculeFormed -= PlayBondSuccessSound;
        }

        private void PlayBondSuccessSound(MoleculeDefinition definition, Vector3 spawnPosition)
        {
            if (_bondSuccessClip == null || _audioSource == null)
                return;

            _audioSource.PlayOneShot(_bondSuccessClip);
        }
    }
}