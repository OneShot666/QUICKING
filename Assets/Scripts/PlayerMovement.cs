using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public float moveSpeed = 5f;

    void Start() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update() {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(moveHorizontal, 0f, moveVertical);
        transform.position += move * (moveSpeed * Time.deltaTime);
    }
}
