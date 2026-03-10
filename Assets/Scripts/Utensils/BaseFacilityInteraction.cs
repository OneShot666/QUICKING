using UnityEngine;

// ! Display inventory availability : nb_item/nb_slots
// ! Display cooking item status (fill bar that grow with time)
namespace Utensils {
    public abstract class BaseFacilityInteraction : MonoBehaviour {
        public abstract string GetInteractionPrompt();                          // Describe action

        public abstract void Preview();

        public abstract void Interact();

        public virtual void OnFocus() {}                                        // When player around

        public virtual void OnLoseFocus() {}                                    // When player exit
    }
}
