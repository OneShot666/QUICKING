using UnityEngine;

public class ItemSurface : MonoBehaviour {                                      // Places to put food items
    [Header("Settings")]
    [Tooltip("[Optional] Where to place item")]
    public Transform placementPoint;

    public bool IsAvailable() {                                                 // Check if place is free or not
        if (placementPoint && placementPoint.childCount > 0) return false;      // Logic based on children in object
        return true;
    }

    public Transform GetPlacementTransform() => placementPoint ? placementPoint : transform;
}
