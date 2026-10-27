using System;
using UnityEngine;

namespace CharacterSystem.Equipment
{
    /// <summary>
    /// Represents an individual equipment slot holding an item and its runtime visual instance.
    /// </summary>
    [Serializable]
    public class EquipmentSlot
    {
        [SerializeField] private EquipmentSlotType slotType;
        [SerializeField] private EquipmentItemSO currentItem;

        public EquipmentSlotType SlotType => slotType;
        public EquipmentItemSO CurrentItem => currentItem;
        public GameObject VisualInstance { get; set; }

        public bool IsOccupied => (object)currentItem != null;

        public EquipmentSlot(EquipmentSlotType type)
        {
            slotType = type;
            currentItem = null;
            VisualInstance = null;
        }

        public void SetItem(EquipmentItemSO item)
        {
            currentItem = item;
        }

        public void Clear()
        {
            currentItem = null;
            if (VisualInstance != null)
            {
                UnityEngine.Object.Destroy(VisualInstance);
                VisualInstance = null;
            }
        }
    }
}
