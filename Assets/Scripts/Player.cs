using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;

    // Gestion des mains
    public FoodOrUtensilReceiver leftHandItem;
    public FoodOrUtensilReceiver rightHandItem;

    public HandUIManager handUIManager;

    private Rigidbody rb;

    // Vérifie si le joueur a un couteau dans une main
    public bool HasKnifeInHand()
    {
        return (leftHandItem != null && leftHandItem.isKnife) || (rightHandItem != null && rightHandItem.isKnife);
    }

    // Vérifie si la main est libre
    public bool CanEquipInHand(bool leftHand)
    {
        return leftHand ? leftHandItem == null : rightHandItem == null;
    }

    // Méthode pour équiper un objet dans une main
    public void EquipItem(FoodOrUtensilReceiver item, bool leftHand)
    {
        if (leftHand)
        {
            leftHandItem = item;
            if (handUIManager != null)
            {
                if (item != null)
                    handUIManager.SetLeftHandItem(item.gameObject);
                else
                    handUIManager.ClearLeftHandItem();
            }
        }
        else
        {
            rightHandItem = item;
            if (handUIManager != null)
            {
                if (item != null)
                    handUIManager.SetRightHandItem(item.gameObject);
                else
                    handUIManager.ClearRightHandItem();
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
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
        Vector3 move = new Vector3(moveHorizontal, 0f, moveVertical).normalized * (moveSpeed * Time.deltaTime);
        if (move != Vector3.zero)
        {
            rb.MovePosition(rb.position + move);
        }
    }
}
