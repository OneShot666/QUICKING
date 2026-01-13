using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Inventory;
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

        public float InteractionRadius => interactionRadius;

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
            
            bool handsFull = _rightHeldItem && _leftHeldItem;
            
            foreach (var col in colliders) {                // If hold item, look for Surfaces. Else, look for Items
                MonoBehaviour candidate = null;

                ItemBase item = col.GetComponentInParent<ItemBase>();           // Look for item first
                
                if (item) {                                                     // Check if inside a kitchen facility
                    KitchenFacility container = item.GetComponentInParent<KitchenFacility>();
                    
                    if (container && !container.isDoorOpen) item = null;        // If facility closed, can't pick item up
                }

                if (item && !handsFull && item != _rightHeldItem && item != _leftHeldItem)  // If at least a hand free
                    candidate = item;                                           // If item isn't already in hands

                if (!candidate) candidate = col.GetComponentInParent<ItemSurface>();                // Then for surface
                if (!candidate) candidate = col.GetComponentInParent<BaseFacilityInteraction>();    // And finally for facilities

                if (candidate) {                                                // Check if it's closest one
                    Vector3 closestPoint = col.ClosestPoint(detectionCenter);
                    float dist = Vector3.Distance(detectionCenter, closestPoint);   // Closest distance from item to player
                    float finalScore = dist;

                    if (candidate is ItemBase) finalScore -= 0.5f;              // Bonus for item on the ground
                    else if (candidate is BaseFacilityInteraction) finalScore -= 1.0f;  // Bonus for facility
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

        private void UpdateActionsUI() {                                        // Create action buttons
            if (!_currentInteractable) {
                buttonsManager.Hide();
                return;
            }

            _currentOptions.Clear();
            string targetName = _currentInteractable.gameObject.name
                .Replace("(Clone)", "").Replace("_", " ").Trim();               // Get proper name

            if (_currentInteractable is ItemBase item) {                        // Pick up item option
                if (CanEquipInHand(true) || CanEquipInHand(false)) {
                    string handText = CanEquipInHand(true) ? "(Right)" : "(Left)";
                    _currentOptions.Add(new InteractionOption($"Pick up {item.GetName()} {handText}", () => PickUpItem(item)));
                }

                CuttingBoard parentBoard = item.GetComponentInParent<CuttingBoard>();
                if (parentBoard && item is FoodItem food && food.CanBeSliced() &&
                !parentBoard.IsCutting) {                                       // If item on cutting board and board isn't busy
                    _currentOptions.Add(new InteractionOption($"Slice {food.GetName()}", 
                        () => { SliceFood(parentBoard); UpdateActionsUI(); }));
                }

                CookingStation parentStove = item.GetComponentInParent<CookingStation>();
                if (parentStove && item is FoodItem rawFood && rawFood.CanBeCooked() &&
                !parentStove.IsCooking) {                                       // If item in cooking station and station isn't busy
                    _currentOptions.Add(new InteractionOption($"Cook {rawFood.GetName()}", 
                        () => { CookFood(parentStove); UpdateActionsUI(); }));
                }
            } else if (_currentInteractable is ItemSurface surface) {           // Interact with surface
                bool isAvailable = surface.IsAvailable();

                if (isAvailable) {                                              // Place item on surface
                    if (surface is TrashBin bin) {
                        if (_rightHeldItem) {
                            string txt = $"Throw {_rightHeldItem.GetName()}";
                            _currentOptions.Add(new InteractionOption(txt, () => ThrowItemInBin(bin, _rightHeldItem, true)));
                        }
                        if (_leftHeldItem) {
                            string txt = $"Throw {_leftHeldItem.GetName()}";
                            _currentOptions.Add(new InteractionOption(txt, () => ThrowItemInBin(bin, _leftHeldItem, false)));
                        }
                    } else {
                        if (_rightHeldItem) {
                            string txt = $"Place {_rightHeldItem.GetName()} on {targetName}";
                            _currentOptions.Add(new InteractionOption(txt, () => PlaceChooseItem(surface, _rightHeldItem, true)));
                        }
                        if (_leftHeldItem) {
                            string txt = $"Place {_leftHeldItem.GetName()} on {targetName}";
                            _currentOptions.Add(new InteractionOption(txt, () => PlaceChooseItem(surface, _leftHeldItem, false)));
                        }
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

                    if (CanEquipInHand(true) || CanEquipInHand(false)) {        // If at least a hand is free
                        _currentOptions.Add(new InteractionOption($"Pick up {food.GetName()}", () => PickUpItem(food)));
                    }
                }
            } else if (_currentInteractable is BaseFacilityInteraction facility) {  // Interact with facility
                if (facility is ItemSpawner spawner) {
                    if (spawner.CanSpawn()) {                                   // If not empty
                        if (CanEquipInHand(true))
                            _currentOptions.Add(new InteractionOption(spawner.GetInteractionPrompt() + " (Right)", 
                                () => TakeFromSpawner(spawner, true)));

                        if (CanEquipInHand(false))
                            _currentOptions.Add(new InteractionOption(spawner.GetInteractionPrompt() + " (Left)", 
                                () => TakeFromSpawner(spawner, false)));
                    }
                } else {
                    _currentOptions.Add(new InteractionOption(facility.GetInteractionPrompt(), 
                        () => { facility.Interact(); UpdateActionsUI(); }));

                    if (facility is KitchenFacility { isDoorOpen: false } kitchen) {
                        FacilityInventory inv = kitchen.facilityInventory;

                        if (inv && inv.HasSpace()) {
                            if (_rightHeldItem)
                                _currentOptions.Add(new InteractionOption($"Put {_rightHeldItem.GetName()} in {targetName}", 
                                    () => StoreItemInFacility(inv, _rightHeldItem, true)));

                            if (_leftHeldItem)
                                _currentOptions.Add(new InteractionOption($"Put {_leftHeldItem.GetName()} in {targetName}", 
                                    () => StoreItemInFacility(inv, _leftHeldItem, false)));
                        }
                    }

                    if (facility is CookingFacility { isDoorOpen: false, isLightOn: true } oven ) // To turn off facilities
                        _currentOptions.Add(new InteractionOption($"Turn off {targetName}", 
                            () => { oven.TurnOff(); UpdateActionsUI(); }));
                }
            } else if (_currentInteractable is NPCs.NPCClient npcClient) {
                if (_rightHeldItem && _rightHeldItem is FoodItem foodRight) {
                    _currentOptions.Add(new InteractionOption($"Give {foodRight.GetName()} (Right)", () => GiveItemToClient(npcClient, _rightHeldItem, true)));
                }
                if (_leftHeldItem && _leftHeldItem is FoodItem foodLeft) {
                    _currentOptions.Add(new InteractionOption($"Give {foodLeft.GetName()} (Left)", () => GiveItemToClient(npcClient, _leftHeldItem, false)));
                }
            }

            if (_currentOptions.Count > 0) buttonsManager.Show(_currentInteractable.transform, _currentOptions);
            else buttonsManager.Hide();                                         // Update buttons menu
        }

        public void PickUpItem(ItemBase item) {
            if (CanEquipInHand(true)) {
                if (pickUpSound?.clip) pickUpSound.Play();
                _rightHeldItem = item;
                handUIManager?.SetRightHandItem(item);
                item.OnEquip(rightHand);
            } else if (CanEquipInHand(false)) {
                if (pickUpSound?.clip) pickUpSound.Play();
                _leftHeldItem = item;
                handUIManager?.SetLeftHandItem(item);
                item.OnEquip(leftHand);
            }

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

        private void ThrowItemInBin(TrashBin bin, ItemBase item, bool isRightHand) {
            if (!item) return;

            if (isRightHand) {                                                  // Empty hand
                _rightHeldItem = null;
                handUIManager?.ClearRightHandItem();
            } else {
                _leftHeldItem = null;
                handUIManager?.ClearLeftHandItem();
            }

            bin.DisposeItem(item);                                              // Place item in bin
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

        private void TakeFromSpawner(ItemSpawner spawner, bool isRightHand) {
            ItemBase newItem = spawner.SpawnItem();                             // Get instance of item

            if (newItem) {                                                      // Equip item
                if (isRightHand) {
                    _rightHeldItem = newItem;
                    handUIManager?.SetRightHandItem(newItem);
                    newItem.OnEquip(rightHand);
                } else {
                    _leftHeldItem = newItem;
                    handUIManager?.SetLeftHandItem(newItem);
                    newItem.OnEquip(leftHand);
                }
                
                if (pickUpSound) pickUpSound.Play();
            }
            
            UpdateActionsUI();
        }
        
        private void StoreItemInFacility(FacilityInventory inventory, ItemBase item, bool isRightHand) {
            if (!item) return;

            if (inventory.AddItem(item)) {                                      // Try to add item
                if (isRightHand) {                                              // If success, update hands
                    _rightHeldItem = null;
                    handUIManager?.ClearRightHandItem();
                } else {
                    _leftHeldItem = null;
                    handUIManager?.ClearLeftHandItem();
                }

                UpdateActionsUI();                                              // Update action button
                
                InventoryUIManager.Instance.RefreshContentIfOpen(inventory);    // Update UI inventory
            }
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

<<<<<<< Updated upstream
            if (isRightHand)
            {
                _rightHeldItem = null;
                handUIManager?.ClearRightHandItem();
            }

            else
            {
=======
            if (isRightHand) {
                _rightHeldItem = null;
                handUIManager?.ClearRightHandItem();
            } else {
>>>>>>> Stashed changes
                _leftHeldItem = null;
                handUIManager?.ClearLeftHandItem();
            }
        }

        private void OnDrawGizmosSelected() {                                   // Draw radius in Scene View
            Gizmos.color = Color.yellow;
            Vector3 detectionCenter = transform.TransformPoint(interactionOffset);
            Gizmos.DrawWireSphere(detectionCenter, interactionRadius);
        }

        private void GiveItemToClient(NPCs.NPCClient client, ItemBase item, bool isRightHand) {
            if (client.TryDeliverItem(item.gameObject)) {
                if (isRightHand) {
                    _rightHeldItem = null;
                    handUIManager?.ClearRightHandItem();
                } else {
                    _leftHeldItem = null;
                    handUIManager?.ClearLeftHandItem();
                }
                UpdateActionsUI();
            }
        }
    }
}
