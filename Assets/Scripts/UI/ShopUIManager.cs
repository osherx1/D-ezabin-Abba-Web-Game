using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace UI
{
    public class ShopUIManager : MonoBehaviour
    {
        [System.Serializable]
        private struct ButtonEntry
        {
            public CreatureStage kind;
            public AnimalButton button;
        }

        [SerializeField] private List<ButtonEntry> buttons;
        private Dictionary<CreatureStage, AnimalButton> _lookup;

        private void Awake()
        {
            _lookup = new Dictionary<CreatureStage, AnimalButton>();
            foreach (var e in buttons) _lookup[e.kind] = e.button;
        }

        private void OnEnable()
        {
            GameEvents.OnCreatureMerged += UnlockButton; // ← NEW
        }

        private void OnDisable()
        {
            GameEvents.OnCreatureMerged -= UnlockButton; // ← NEW
        }

        private void Start()
        {
            UnlockButton(CreatureStage.Goat);
        }

        private void UnlockButton(CreatureStage kind)
        {
            if (_lookup.TryGetValue(kind, out var btn))
                btn.Unlock();
        }
    }
}