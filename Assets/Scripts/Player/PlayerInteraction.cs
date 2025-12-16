using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Utensils;
using Food;
using UI;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
// ReSharper disable Unity.PreferNonAllocApi
namespace Player {
    public class PlayerInteraction : MonoBehaviour {
        [Header("References")]
        [Tooltip("Left hand of player")]
        [SerializeField] private Transform leftHand;
        [Tooltip("Right hand of player")]
        [SerializeField] private Transform rightHand;
        [SerializeField] private InteractionButtonMenuManager buttonsManager;
        [SerializeField] private HandUIManager handUIManager;
        [SerializeField] private AudioSource pickUpSound;

        [Header("Settings")]
        [Tooltip("Layer for items and surfaces")]
        [SerializeField] private LayerMask interactLayer;
        [Tooltip("Radius around the player to detect items")]
        [SerializeField] private float interactionRadius = 4.0f;
        [Tooltip("Offset from the player pivot (feet) to the center of detection sphere")]
        [SerializeField] private Vector3 interactionOffset = new(0, 1f, 0);

        private readonly List<InteractionOption> _currentOptions = new();
        private MonoBehaviour _currentInteractable;                             // ItemBase or ItemSurface
        private PlayerInput _playerInput;
        private InputAction _dropAction;
        private ItemBase _rightHeldItem;
        private ItemBase _leftHeldItem;

        private void Awake() {
            _playerInput = GetComponent<PlayerInput>();
            _dropAction = _playerInput.actions["Drop"];
        }

        private void Update() {
            DetectNearbyObjects();
            HandleDrop();
        }

        public bool HasKnifeInHand() {                                          // ! Upgrade function after made proper knife
            return false;
        }

        public bool CanEquipInHand(bool isRightHand) {
            if (isRightHand) return !_rightHeldItem;
            return !_leftHeldItem;
        }

        private void DetectNearbyObjects() {                                    // Scan around player
            Vector3 detectionCenter = transform.TransformPoint(interactionOffset);
            Collider[] colliders = Physics.OverlapSphere(detectionCenter, interactionRadius, interactLayer);

            MonoBehaviour closestInteractable = null;
            float minDistance = float.MaxValue;
            
            foreach (var col in colliders) {                // If hold item, look for Surfaces. Else, look for Items
                MonoBehaviour candidate = col.GetComponentInParent<ItemBase>();                     // Look for item first
                if (!candidate) candidate = col.GetComponentInParent<BaseFacilityInteraction>();    // Then for facilities
                if (!candidate) candidate = col.GetComponentInParent<ItemSurface>();                // And finally for surface

                if (candidate) {                                                // Check if it's closest one
                    if (candidate is ItemBase item && (item == _rightHeldItem || item == _leftHeldItem)) 
                        continue;                                               // If items already in hands

                    float rawDist = Vector3.Distance(detectionCenter, candidate.transform.position);
                    float finalScore = rawDist;

                    if (candidate is ItemBase) finalScore -= 0.5f;                    // Bonus for item on the ground
                    else if (candidate is CuttingBoard board) {
                        ItemBase itemOnBoard = board.GetHeldItem();
                        if (itemOnBoard is FoodItem f && f.CanBeSliced()) finalScore -= 2.0f;
                    } else if (candidate is CookingStation stove) {
                        ItemBase itemOnStove = stove.GetHeldItem();
                        if (itemOnStove is FoodItem f && f.CanBeCooked()) finalScore -= 2.0f;
                    }

                    if (finalScore < minDistance) {
                        minDistance = finalScore;
                        closestInteractable = candidate;
                    }
                }
            }

            if (closestInteractable != _currentInteractable || closestInteractable) {
                _currentInteractable = closestInteractable;                     // Update current target and UI
                UpdateActionsUI();
            }
        }

        private void UpdateActionsUI() {
            if (!_currentInteractable) {
                buttonsManager.Hide();
                return;
            }

            _currentOptions.Clear();
            string targetName = _currentInteractable.gameObject.name
                .Replace("(Clone)", "").Replace("_", " ").Trim();               // Get proper name

            if (_currentInteractable is ItemBase item) {                        // Pick up item option
                if (!_rightHeldItem || !_leftHeldItem) {
                    string handText = !_rightHeldItem ? "(Right)" : "(Left)";
                    _currentOptions.Add(new InteractionOption($"Pick up {item.GetName()} {handText}", () => PickUpItem(item)));
                }

                CuttingBoard parentBoard = item.GetComponentInParent<CuttingBoard>();
                if (parentBoard && item is FoodItem food && food.CanBeSliced() &&
                !parentBoard.IsCutting) {                                       // If item on cutting board and bpard isn't busy
                    _currentOptions.Add(new InteractionOption($"Slice {food.GetName()}", 
                        () => { SliceFood(parentBoard); UpdateActionsUI(); }));
                }

                CookingStation parentStove = item.GetComponentInParent<CookingStation>();
                if (parentStove && item is FoodItem rawFood && rawFood.CanBeCooked() &&
                !parentStove.IsCooking) {                                       // If item in cooking station and station isn't busy
                    _currentOptions.Add(new InteractionOption($"Cook {rawFood.GetName()}", 
                        () => { CookFood(parentStove); UpdateActionsUI(); }));
                }
            } else if (_currentInteractable is BaseFacilityInteraction facility) {  // Interact with facility
                _currentOptions.Add(new InteractionOption(facility.GetInteractionPrompt(), 
                    () => { facility.Interact(); UpdateActionsUI(); }));

                if (facility is KitchenFacility { isDoorOpen: true } kitchen)
                    _currentOptions.Add(new InteractionOption($"Open inventory", () => kitchen.OpenInventory()));

                if (facility is CookingFacility { isDoorOpen: false, isLightOn: true }) // To turn off facilities
                    _currentOptions.Add(new InteractionOption($"Turn off {targetName}", 
                        () => { facility.Interact(); UpdateActionsUI(); }));
            } else if (_currentInteractable is ItemSurface surface) {           // Interact with surface
                bool isAvailable = surface.IsAvailable();

                if (isAvailable) {                                              // Place item on surface
                    if (_rightHeldItem) {
                        string txt = $"Place {_rightHeldItem.GetName()} on {targetName}";
                        _currentOptions.Add(new InteractionOption(txt, () => PlaceChooseItem(surface, _rightHeldItem, true)));
                    }
                    if (_leftHeldItem) {
                        string txt = $"Place {_leftHeldItem.GetName()} on {targetName}";
                        _currentOptions.Add(new InteractionOption(txt, () => PlaceChooseItem(surface, _leftHeldItem, false)));
                    }
                }

                ItemBase itemOnSurface = surface.GetHeldItem();

                if (itemOnSurface is FoodItem food) {
                    if (surface is CuttingBoard board && food.CanBeSliced() && !board.IsCutting) {
                        _currentOptions.Add(new InteractionOption($"Slice {food.GetName()}", 
                            () => { SliceFood(board); UpdateActionsUI(); }));
                    }

                    if (surface is CookingStation stove && food.CanBeCooked() && !stove.IsCooking) {
                        _currentOptions.Add(new InteractionOption($"Cook {food.GetName()}", 
                            () => { CookFood(stove); UpdateActionsUI(); }));
                    }

                    if (!_rightHeldItem || !_leftHeldItem) {                    // If at least a hand is free
                        _currentOptions.Add(new InteractionOption($"Pick up {food.GetName()}", () => PickUpItem(food)));
                    }
                }
            }

            if (_currentOptions.Count > 0) buttonsManager.Show(_currentInteractable.transform, _currentOptions);
            else buttonsManager.Hide();                                         // Update buttons menu
        }

        private void PickUpItem(ItemBase item) {
            if (!_rightHeldItem) {
                if (pickUpSound?.clip) pickUpSound.Play();
                _rightHeldItem = item;
                handUIManager?.SetRightHandItem(item.gameObject);
                item.OnEquip(rightHand);
            } else if (!_leftHeldItem) {
                if (pickUpSound?.clip) pickUpSound.Play();
                _leftHeldItem = item;
                handUIManager?.SetLeftHandItem(item.gameObject);
                item.OnEquip(leftHand);
            }

            UpdateActionsUI();
        }

        private void PlaceChooseItem(ItemSurface surface, ItemBase item, bool isRightHand) {
            if (!item) return;

            if (isRightHand) {
                _rightHeldItem = null;                                          // Empty hand
                handUIManager?.ClearRightHandItem();
            } else {
                _leftHeldItem = null;
                handUIManager?.ClearLeftHandItem();
                
            }

            item.OnPlace(surface.GetPlacementTransform());
            
            UpdateActionsUI();
        }

        private void SliceFood(CuttingBoard board) {
            board.OnInteract();
            UpdateActionsUI();
        }

        private void CookFood(CookingStation stove) {
            stove.OnInteract();
            UpdateActionsUI();
        }

        private void HandleDrop() {
            if (_dropAction.WasPerformedThisFrame()) {                          // Drop Right first, then Left
                if (_rightHeldItem) DropItem(true);
                else if (_leftHeldItem) DropItem(false);
                UpdateActionsUI();
            }
        }

        private void DropItem(bool isRightHand) {
            ItemBase item = isRightHand ? _rightHeldItem : _leftHeldItem;
            if (!item) return;

            item.OnDrop();                                                      // Handles parenting to "Foods"

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
