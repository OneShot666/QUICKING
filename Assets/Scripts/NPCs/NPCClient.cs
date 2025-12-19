namespace NPCs {
    using Food;
    using TMPro;
    using UnityEngine;
    using Utensils;

    public class NPCClient : MonoBehaviour
    {
        public string requestedItemName;
        public ItemBase requestedItem; // Optional: reference to the actual item
        public GameObject requestUIPrefab;
        public Transform requestUIAnchor;
        public TMP_Text requestedDishText; // Dedicated TMP_Text for dish name
        public ItemSurface assignedPlate; // The plate this NPC is waiting at
        private GameObject requestUIInstance;
        private System.Action<NPCClient> onClientLeave;

        // Movement
        public Transform waitingSpot;
        public Transform exitSpot;
        public float moveSpeed = 2f;
        private bool isLeaving = false;
        private bool atWaitingSpot = false;

        public void Initialize(string itemName, ItemSurface plate, System.Action<NPCClient> onLeave, Transform waitSpot, Transform exit, ItemBase itemRef = null, TMP_Text dishText = null)
        {
            requestedItemName = itemName;
            requestedItem = itemRef;
            assignedPlate = plate;
            onClientLeave = onLeave;
            waitingSpot = waitSpot;
            exitSpot = exit;
            requestedDishText = dishText;
            ShowRequestUI();
        }

        private void Update()
        {
            if (!atWaitingSpot && waitingSpot)
            {
                MoveTo(waitingSpot.position, () => atWaitingSpot = true);
            }
            else if (isLeaving && exitSpot)
            {
                MoveTo(exitSpot.position, () => FulfillRequestAndLeave());
            }
        }

        private void MoveTo(Vector3 target, System.Action onArrive)
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, step);
            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                onArrive?.Invoke();
            }
        }

        private void ShowRequestUI()
        {
            if (requestUIPrefab && requestUIAnchor)
            {
                requestUIInstance = Instantiate(requestUIPrefab, requestUIAnchor);
                var text = requestUIInstance.GetComponentInChildren<TMP_Text>();
                if (text) text.text = requestedItem ? requestedItem.GetName() : requestedItemName;
            }
            // Set dedicated TMP_Text if assigned
            if (requestedDishText)
                requestedDishText.text = requestedItem ? requestedItem.GetName() : requestedItemName;
        }

        public bool TryDeliverItem(GameObject item)
        {
            var foodItem = item.GetComponent<FoodItem>();
            if (foodItem && foodItem.GetName() == requestedItemName)
            {
                Destroy(item);
                StartLeaving();
                return true;
            }
            return false;
        }

        public bool IsRequestFulfilled()
        {
            var item = assignedPlate?.GetHeldItem();
            if (item && item.GetName() == requestedItemName)
            {
                Destroy(item.gameObject);
                StartLeaving();
                return true;
            }
            return false;
        }

        private void StartLeaving()
        {
            isLeaving = true;
            if (requestUIInstance) Destroy(requestUIInstance);
        }

        public void FulfillRequestAndLeave()
        {
            onClientLeave?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
