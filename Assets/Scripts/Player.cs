using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // La souris reste libre pour l'UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Déplacement du joueur en vue du dessus
        float moveHorizontal = Input.GetAxis("Horizontal"); // Q/D ou A/D
        float moveVertical = Input.GetAxis("Vertical"); // Z/S ou W/S
        Vector3 move = new Vector3(moveHorizontal, 0f, moveVertical);
        transform.position += move * (moveSpeed * Time.deltaTime);
    }
}
