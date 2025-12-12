using UnityEngine;
using Food;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace Utensils {
    [RequireComponent(typeof(Collider))]
    public class ItemSurface : MonoBehaviour {                                  // Places to put food items
        [Header("Settings")]
        [Tooltip("[Optional] Where to place item")]
        public Transform placementPoint;

        private void Awake() {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }

        public bool IsAvailable() {                                             // Check if place is free or not
            Transform holder = GetPlacementTransform();
            if (holder.childCount == 0) return true;                            // Logic based on children in object
            return !holder.GetComponentInChildren<ItemBase>();
        }

        public virtual void OnInteract() {}

        public Transform GetPlacementTransform() => placementPoint ? placementPoint : transform;

        public ItemBase GetHeldItem() {                                         // Give current item placed
            Transform holder = GetPlacementTransform();
            if (holder.childCount > 0) return holder.GetComponentInChildren<ItemBase>();
            return null;
        }
    }
}
