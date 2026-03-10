using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
public class DebugUI : MonoBehaviour {
    void Update() {
        // Si on clique gauche
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame) {
            // On demande à l'EventSystem : "Qu'est-ce qu'il y a sous la souris ?"
            if (EventSystem.current.IsPointerOverGameObject()) {
                Debug.Log($"[UI BLOCAGE] La souris touche l'objet UI : {GetHoveredObjectName()}");
            } else {
                Debug.Log("[UI LIBRE] Clic dans le vide (World 3D)");
            }
        }
    }

    string GetHoveredObjectName() {
        PointerEventData pointerData = new PointerEventData(EventSystem.current) {
            position = UnityEngine.InputSystem.Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0) {
            return results[0].gameObject.name; // Retourne le nom du coupable
        }
        return "Inconnu";
    }
}
