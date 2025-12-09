using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [Header("Settings")]
    public float moveSpeed = 10f;
    [Tooltip("Time it takes to rotate towards movement direction")]
    public float turnSmoothTime = 0.2f;

    [Header("References")]
    public Transform cam;

    private float _turnSmoothVelocity;

    void Start() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!cam && Camera.main) cam = Camera.main.transform;
    }

    void Update() {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");                  // Inputs
        float moveVertical = Input.GetAxisRaw("Vertical");                      // GetAxisRaw avoids Unity's auto-smoothing
        Vector3 direction = new Vector3(moveHorizontal, 0f, moveVertical).normalized;

        if (direction.magnitude >= 0.1f) {                                      // Rotate towards direction when move
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);               // Apply smooth movement

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            transform.position += moveDir.normalized * (moveSpeed * Time.deltaTime);
        }
    }
}
