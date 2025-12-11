using UnityEngine;

public class KitchenFacilityReceiver : InteractionReceiver
{
    public enum RotationAxis { X, Y, Z }

    public bool isCookingSpot = false;
    public bool isOpen = false;
    public bool isOn = false;
    public float cookingTimer = 0f;
    public Light facilityLight;

    [Header("Animation de la porte")]
    public GameObject doorObject; // L'objet à faire tourner
    public RotationAxis rotationAxis = RotationAxis.Y;
    public float closedAngle = 0f;
    public float openedAngle = 90f;
    public float doorAnimationSpeed = 180f; // degrés/seconde
    private float targetAngle;

    [Header("Inventaire de l'équipement")]
    public Inventory inventory;
    public int previewSlotCount = 4;

    public GameObject previewContent;
    public GameObject fullContent;

    private bool wasFullInventoryJustClosed = false;
    private bool isInteractable = true;
    private bool isPlayerInTrigger = false;
    private bool isMouseOver = false;
    private bool previewShouldBeHidden = false;

    public override void Preview()
    {
        if (inventory != null && !wasFullInventoryJustClosed && !previewShouldBeHidden)
        {
            Item[] previewItems = GetPreviewItems();
            InventoryUIManager.Instance.ShowPreview(previewItems);
        }
    }

    public override void Interact()
    {
        if (!isInteractable) return;
        // Interaction possible uniquement si le joueur est dans le trigger
        if (!isPlayerInTrigger) return;
        if (inventory != null)
        {
            Item[] allItems = inventory.GetAllItems();
            InventoryUIManager.Instance.ShowFullContent(allItems);
        }
        // Si déjà ouvert, on ferme et on cache l'inventaire complet
        if (isOpen)
        {
            CloseFacility();
            return;
        }
        ToggleOpen();
        ToggleOnOff();
        if (isCookingSpot)
        {
            StartCooking();
        }
    }

    public Item[] GetPreviewItems()
    {
        if (inventory == null) return new Item[0];
        int count = Mathf.Min(previewSlotCount, inventory.slots.Length);
        Item[] preview = new Item[count];
        for (int i = 0; i < count; i++)
            preview[i] = inventory.slots[i].item;
        return preview;
    }

    private void ToggleOpen()
    {
        isOpen = !isOpen;
        targetAngle = isOpen ? openedAngle : closedAngle;
    }

    private void ToggleOnOff()
    {
        isOn = !isOn;
        if (facilityLight != null)
            facilityLight.enabled = isOn;
    }

    private void StartCooking()
    {
        if (isOn)
        {
            cookingTimer = 10f; // exemple de durée
            // Lancer la logique de cuisson ici
        }
    }

    void Update()
    {
        // Animation de la porte
        if (doorObject != null)
        {
            Vector3 currentRotation = doorObject.transform.localEulerAngles;
            float currentAngle = 0f;
            switch (rotationAxis)
            {
                case RotationAxis.X: currentAngle = currentRotation.x; break;
                case RotationAxis.Y: currentAngle = currentRotation.y; break;
                case RotationAxis.Z: currentAngle = currentRotation.z; break;
            }
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, doorAnimationSpeed * Time.deltaTime);
            switch (rotationAxis)
            {
                case RotationAxis.X:
                    doorObject.transform.localEulerAngles = new Vector3(newAngle, currentRotation.y, currentRotation.z);
                    break;
                case RotationAxis.Y:
                    doorObject.transform.localEulerAngles = new Vector3(currentRotation.x, newAngle, currentRotation.z);
                    break;
                case RotationAxis.Z:
                    doorObject.transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, newAngle);
                    break;
            }
        }
    }

    void Awake()
    {
        // Initialisation de l'angle de la porte
        targetAngle = isOpen ? openedAngle : closedAngle;
        if (doorObject != null)
        {
            Vector3 rot = doorObject.transform.localEulerAngles;
            switch (rotationAxis)
            {
                case RotationAxis.X:
                    doorObject.transform.localEulerAngles = new Vector3(targetAngle, rot.y, rot.z);
                    break;
                case RotationAxis.Y:
                    doorObject.transform.localEulerAngles = new Vector3(rot.x, targetAngle, rot.z);
                    break;
                case RotationAxis.Z:
                    doorObject.transform.localEulerAngles = new Vector3(rot.x, rot.y, targetAngle);
                    break;
            }
        }
        // La lumière doit être éteinte au démarrage
        if (facilityLight != null)
            facilityLight.enabled = false;
        isOn = false;
    }

    private void EndPreview()
    {
        InventoryUIManager.Instance.HidePreview();
    }

    public void CloseFacility()
    {
        InventoryUIManager.Instance.HideFullContent();
        isOpen = false;
        targetAngle = closedAngle;
        wasFullInventoryJustClosed = true;
        isInteractable = false;
    }

    void OnMouseEnter()
    {
        isMouseOver = true;
        previewShouldBeHidden = false;
        isInteractable = true; // Correction : réactivation systématique
        if (inventory != null && !wasFullInventoryJustClosed)
        {
            Item[] previewItems = GetPreviewItems();
            InventoryUIManager.Instance.ShowPreview(previewItems);
        }
    }

    void OnMouseExit()
    {
        isMouseOver = false;
        InventoryUIManager.Instance.HidePreview();
        wasFullInventoryJustClosed = false;
        previewShouldBeHidden = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            previewShouldBeHidden = false;
            isInteractable = true; // Correction : réactivation systématique
            if (inventory != null && !InventoryUIManager.Instance.previewContainer.activeSelf && !wasFullInventoryJustClosed)
            {
                Item[] previewItems = GetPreviewItems();
                InventoryUIManager.Instance.ShowPreview(previewItems);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            EndPreview();
            CloseFacility(); // Fermeture automatique de la facility
            wasFullInventoryJustClosed = false;
            previewShouldBeHidden = true;
        }
    }

    private void HideFullContentIfOpen()
    {
        if (isOpen)
        {
            InventoryUIManager.Instance.HideFullContent();
        }
    }
}
