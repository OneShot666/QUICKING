using UnityEngine.InputSystem;
using UnityEngine;
using Utensils;
using Food;
using UI;

// ! Logic of distance for interact texts are reverse between Slice & Pick up (line 65)
// ReSharper disable Unity.PerformanceCriticalCodeInvocation
// ReSharper disable Unity.PreferNonAllocApi
namespace Player {
    public class PlayerInteraction : MonoBehaviour {
        [Header("References")]
        public Transform leftHand;
        public Transform rightHand;
        public InteractionPrompt pickUpPrompt;

        [Header("Settings")]
        [Tooltip("Layer for items and surfaces")]
        public LayerMask interactLayer;
        [Tooltip("Radius around the player to detect items")]
        public float interactionRadius = 4.0f;
        [Tooltip("Offset from the player pivot (feet) to the center of detection sphere")]
        public Vector3 interactionOffset = new(0, 1f, 0);
        [Tooltip("Maximum distance to cutting boards to become priority on items placed on")]
        public float workStationDistance = 1.5f;

        private MonoBehaviour _currentInteractable;                             // ItemBase or ItemSurface
        private PlayerInput _playerInput;
        private InputAction _interactAction;
        private InputAction _dropAction;
        private ItemBase _rightHeldItem;
        private ItemBase _leftHeldItem;

        private void Awake() {
            _playerInput = GetComponent<PlayerInput>();
            _interactAction = _playerInput.actions["Interact"];
            _dropAction = _playerInput.actions["Drop"];
        }

        private void Update() {
            DetectNearbyObjects();
            HandleInteraction();
            HandleDrop();
        }

        private void DetectNearbyObjects() {                                    // Scan around player
            Vector3 detectionCenter = transform.TransformPoint(interactionOffset);
            Collider[] colliders = Physics.OverlapSphere(detectionCenter, interactionRadius, interactLayer);

            MonoBehaviour closestInteractable = null;
            float minDistance = float.MaxValue;
            
            bool handsFull = _rightHeldItem && _leftHeldItem;

            foreach (var col in colliders) {                // If hold item, look for Surfaces. Else, look for Items
                MonoBehaviour candidate = null;

                if (!handsFull) {
                    ItemBase item = col.GetComponentInParent<ItemBase>();       // Look for item
                    if (item && item != _rightHeldItem || item != _leftHeldItem) candidate = item;
                }

                if (!candidate) candidate = col.GetComponentInParent<ItemSurface>();    // Look for surface

                if (candidate) {                                                // Check if it's closest one
                    float rawDist = Vector3.Distance(detectionCenter, candidate.transform.position);
                    float finalScore = rawDist;

                    if (candidate is ItemBase) finalScore -= 0.5f;              // Bonus distance for items
                    else if (candidate is CuttingBoard) {                       // If surface
                        if (rawDist <= workStationDistance) finalScore -= 2.0f; // Big bonus : score became low (close)
                    }

                    if (finalScore < minDistance) {
                        minDistance = finalScore;
                        closestInteractable = candidate;
                    }
                }
            }

            _currentInteractable = closestInteractable;                         // Update current target and UI
            UpdateUI();
        }

        private void UpdateUI() {
            if (!_currentInteractable) {
                pickUpPrompt.Hide();
                return;
            }

            string message;
            Vector3 targetPos = _currentInteractable.transform.position;

            if (_currentInteractable is ItemBase item) {                        // Pick up item
                message = $"'E' to pick up {item.GetName()}";
            } else if (_currentInteractable is ItemSurface surface) {           // Interact with surface
                if ((_rightHeldItem || _leftHeldItem) && surface.IsAvailable()) {   // Place item
                    ItemBase itemToPlace = _rightHeldItem ? _rightHeldItem : _leftHeldItem;
                    message = $"'E' to place {itemToPlace.GetName()}";
                } else if (!_rightHeldItem && !_leftHeldItem && surface is CuttingBoard board) {    // Use surface object
                    ItemBase itemOnBoard = board.GetHeldItem();                 // Check if is something to cut
                    if (itemOnBoard is FoodItem food && food.CanBeSliced()) {
                        message = $"'E' to Slice {food.GetName()}";
                    } else { pickUpPrompt.Hide(); return; }                     // Nothing to do (already sliced or not food)
                } else { pickUpPrompt.Hide(); return; }                         // Safe fallback
            } else { pickUpPrompt.Hide(); return; }

            pickUpPrompt.Show(message, targetPos);
        }

        private void HandleInteraction() {
            if (!_interactAction.WasPerformedThisFrame()) return;
            if (!_currentInteractable) return;

            if (_currentInteractable is ItemBase itemToPick) {
                if (!_rightHeldItem) PickUpItem(itemToPick, true);              // Right Hand
                else if (!_leftHeldItem) PickUpItem(itemToPick, false);         // Left Hand
            } else if (_currentInteractable is ItemSurface surface) {
                if (surface.IsAvailable()) {                                    // Place right item first. If empty place left item
                    if (_rightHeldItem) PlaceItem(surface, true);
                    else if (_leftHeldItem) PlaceItem(surface, false);
                } else if (!_rightHeldItem && !_leftHeldItem) {                 // Interact with surface
                    surface.OnInteract();
                }
            }
        }

        private void HandleDrop() {
            if (_dropAction.WasPerformedThisFrame()) {                          // Drop Right first, then Left
                if (_rightHeldItem) DropItem(true);
                else if (_leftHeldItem) DropItem(false);
            }
        }

        private void PickUpItem(ItemBase item, bool isRightHand) {
            if (isRightHand) _rightHeldItem = item;
            else _leftHeldItem = item;

            Transform targetHand = isRightHand ? rightHand : leftHand;
            item.OnEquip(targetHand);
        }

        private void DropItem(bool isRightHand) {
            ItemBase item = isRightHand ? _rightHeldItem : _leftHeldItem;
            if (!item) return;

            item.OnDrop();                                                      // Handles parenting to "Foods"

            if (isRightHand) _rightHeldItem = null;
            else _leftHeldItem = null;
        }

        private void PlaceItem(ItemSurface surface, bool isRightHand) {
            ItemBase item = isRightHand ? _rightHeldItem : _leftHeldItem;
            if (!item) return;

            item.OnPlace(surface.GetPlacementTransform());

            if (isRightHand) _rightHeldItem = null;
            else _leftHeldItem = null;
        }

        private void OnDrawGizmosSelected() {                                   // Draw radius in Scene View
            Gizmos.color = Color.yellow;
            Vector3 detectionCenter = transform.TransformPoint(interactionOffset);
            Gizmos.DrawWireSphere(detectionCenter, interactionRadius);
        }
    }
}
