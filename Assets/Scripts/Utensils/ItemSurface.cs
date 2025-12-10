using UnityEngine;
using Food;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace Utensils {
    public class ItemSurface : MonoBehaviour {                                  // Places to put food items
        [Header("Settings")]
        [Tooltip("[Optional] Where to place item")]
        public Transform placementPoint;

        public bool IsAvailable() {                                             // Check if place is free or not
            Transform holder = GetPlacementTransform();
            return holder.childCount == 0;                                      // Logic based on children in object
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
