using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("Slots UI pour l'aperçu (preview)")]
    public RawImage[] previewSlotImages; // Un RawImage par slot, assigné à la RenderTexture de la caméra du slot
    [Header("Slots UI pour le contenu complet (full)")]
    public RawImage[] fullSlotImages;

    [Header("Système d'aperçu 3D partagé")]
    public Camera previewCamera;
    public RenderTexture previewRenderTexture;
    public Transform previewSlotTransform;
    public float previewDelay = 0.05f; // Délai pour forcer le rendu (peut être réduit)

    [Header("Containers UI")]
    public GameObject previewContainer;
    public GameObject fullContainer;

    private bool isPreviewActive = false;
    private bool isFullInventoryOpen = false;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (isPreviewActive && previewContainer.activeSelf)
        {
            Vector2 mousePos = Input.mousePosition;
            RectTransform rect = previewContainer.GetComponent<RectTransform>();
            float width = rect.rect.width;
            float height = rect.rect.height;
            // Offset pour placer le coin inférieur gauche sur la souris
            Vector2 uiPos = mousePos + new Vector2(-width / 2f, height / 2f);
            // Clamp pour rester dans le viewport
            uiPos.x = Mathf.Clamp(uiPos.x, 0, Screen.width - width);
            uiPos.y = Mathf.Clamp(uiPos.y, height, Screen.height);
            previewContainer.transform.position = uiPos;
        }
    }

    public void ShowPreview(Item[] items)
    {
        if (isFullInventoryOpen) return;
        previewContainer.SetActive(true);
        isPreviewActive = true;
        StopAllCoroutines();
        StartCoroutine(RenderInventoryPreviews(items, previewSlotImages));
    }

    public void HidePreview()
    {
        previewContainer.SetActive(false);
        isPreviewActive = false;
    }

    public void ShowFullContent(Item[] items)
    {
        isFullInventoryOpen = true;
        fullContainer.SetActive(true);
        HidePreview();
        StopAllCoroutines();
        StartCoroutine(RenderInventoryPreviews(items, fullSlotImages));
    }

    public void HideFullContent()
    {
        isFullInventoryOpen = false;
        fullContainer.SetActive(false);
        HidePreview();
    }

    private IEnumerator RenderInventoryPreviews(Item[] items, RawImage[] uiSlots)
    {
        Inventory inventory = null;
        if (items.Length > 0 && items[0] != null)
        {
            // On suppose que tous les items viennent du même inventaire
            var slot = FindFirstObjectByType<Inventory>();
            if (slot != null) inventory = slot;
        }
        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (i < items.Length && items[i] != null && inventory != null && inventory.slots[i].itemObject != null)
            {
                var obj = inventory.slots[i].itemObject;
                // Sauvegarder la position/orientation/parent d'origine
                var originalParent = obj.transform.parent;
                var originalPos = obj.transform.position;
                var originalRot = obj.transform.rotation;
                var originalActive = obj.activeSelf;

                // Déplacer l'objet dans le PreviewSlot
                obj.SetActive(true);
                obj.transform.SetParent(previewSlotTransform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;

                // Forcer le rendu
                previewCamera.targetTexture = previewRenderTexture;
                yield return new WaitForSeconds(previewDelay);
                uiSlots[i].texture = previewRenderTexture;
                uiSlots[i].gameObject.SetActive(true);

                // Remettre l'objet à sa place d'origine
                obj.transform.SetParent(originalParent);
                obj.transform.position = originalPos;
                obj.transform.rotation = originalRot;
                obj.SetActive(originalActive);
            }
            else
            {
                uiSlots[i].gameObject.SetActive(false);
            }
        }
    }
}
