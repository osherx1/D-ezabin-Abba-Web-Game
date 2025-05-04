using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace UI
{
    /// <summary>
    /// Manages the creature‑shop buttons:
    /// * Unlocks a button when the matching creature is first created.
    /// * Shows the silhouette of the *next* creature in the chain.
    /// </summary>
    public class ShopUIManager : MonoBehaviour
    {
        [Serializable]
        private struct ButtonEntry
        {
            public CreatureStage kind; // Enum value
            public AnimalButton button; // UI component
        }

        [Header("Button List (drag in Inspector)")] [SerializeField]
        private List<ButtonEntry> buttons;

        private Dictionary<CreatureStage, AnimalButton> _lookup;

        /* ------------------------------------------------------------------ */
        private void Awake()
        {
            _lookup = new Dictionary<CreatureStage, AnimalButton>();
            foreach (var e in buttons)
                _lookup[e.kind] = e.button;
        }

        private void OnEnable()
        {
            GameEvents.OnCreatureMerged += HandleCreatureMerged;
        }

        private void OnDisable()
        {
            GameEvents.OnCreatureMerged -= HandleCreatureMerged;
        }

        private void Start()
        {
            UnlockChain(CreatureStage.Goat); // Goat is the default starter
        }

        /* ------------------------- Event Handler -------------------------- */
        private void HandleCreatureMerged(CreatureStage newStage)
        {
            UnlockChain(newStage);
        }

        /* ------------------------- Core Logic ----------------------------- */
        /// <summary>
        /// Unlocks the current stage and reveals the silhouette of the next.
        /// </summary>
        private void UnlockChain(CreatureStage stage)
        {
            // 1. Unlock current button (if it exists)
            if (_lookup.TryGetValue(stage, out var currentBtn))
                currentBtn.Unlock();

            // 2. Reveal silhouette for the next stage (if it exists)
            CreatureStage nextStage = (CreatureStage)((int)stage + 1);
            if (Enum.IsDefined(typeof(CreatureStage), nextStage) &&
                _lookup.TryGetValue(nextStage, out var nextBtn))
            {
                // Reveal silhouette only if the button is still in Question state
                nextBtn.RevealSilhouette();
            }
        }
    }
}