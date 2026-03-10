using UnityEngine;

namespace Audio {
    [RequireComponent(typeof(AudioSource))]
    public class SoundTrigger : MonoBehaviour {
        [Header("Settings")]
        [Tooltip("If true, plays only once per game session. If false, plays every time player enters range.")]
        public bool oneShotOnly;
        [Tooltip("If true, stop sounds if exit trigger area")]
        public bool soundInAreaOnly;
        [Tooltip("Distance at which the sound plays")]
        public float triggerDistance = 3.0f;

        [Header("References")]
        public Transform player;

        private AudioSource _audioSource;
        private bool _hasPlayed;
        private bool _isInRange;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
        }

        private void Start() {
            if (!player) {                                                      // Auto-find player if not assigned
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p) player = p.transform;
            }
        }

        private void Update() {
            if (!player) return;

            float distance = Vector3.Distance(transform.position, player.position);

            // Check if inside zone
            if (distance <= triggerDistance) {
                if (!_isInRange) {                                              // Player enter trigger zone
                    _isInRange = true;
                    
                    if (!_hasPlayed || !oneShotOnly) PlaySound();
                }
            } else if (_isInRange) {                                               // Player exit trigger zone
                _isInRange = false;
                if (soundInAreaOnly) _audioSource.Stop();
            }
        }

        private void PlaySound() {
            if (_audioSource.clip) {
                _audioSource.Play();
                _hasPlayed = true;
            }
        }

        private void OnDrawGizmosSelected() {                   // Visualize range in Editor
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
        }
    }
}
