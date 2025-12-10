using UnityEngine;

public class CameraManager : MonoBehaviour {
    [Header("Target (player)")]
    [Tooltip("Object to follow")]
    public Transform target;

    [Header("Settings")]
    [Tooltip("If true: Camera stays at fixed position but rotates to look at player." +
             "\nIf false: Camera follows player position but keeps its own rotation.")]
    public bool rotateWithPlayer;
    [Tooltip("Smooth speed of movement")]
    public float smoothTime = 0.2f;

    private Vector3 _currentVelocity;
    private Vector3 _offset;

    private void Start() {
        if (!target) return;

        _offset = transform.position - target.position;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.C)) ToggleCameraMode();                    // Change mode with 'C'
    }

    private void LateUpdate() {
        if (!target) return;

        if (rotateWithPlayer) transform.LookAt(target.position + Vector3.up * 1.5f);    // Rotate camera
        else {
            Vector3 targetPosition = target.position + _offset;                 // Follow player
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, smoothTime);
        }
    }

    private void ToggleCameraMode() {                                           // Toggle helper
        ToggleCameraMode(!rotateWithPlayer);
    }

    private void ToggleCameraMode(bool enableRotationMode) {                    // Change camera mode
        rotateWithPlayer = enableRotationMode;

        if (!rotateWithPlayer) _offset = transform.position - target.position;
    }
}
