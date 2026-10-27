using System;
using System.Collections.Generic;
using CharacterSystem.Attributes;
using UnityEngine;

namespace CharacterSystem.Equipment
{
    /// <summary>
    /// Component managing character equipment slots, applying stat modifiers to the AttributeSystem,
    /// and instantiating modular gear meshes onto skeletal sockets.
    /// </summary>
    public class EquipmentInventory : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private AttributeSystem attributeSystem;

        private readonly Dictionary<EquipmentSlotType, EquipmentSlot> slots =
            new Dictionary<EquipmentSlotType, EquipmentSlot>();

        public event Action<EquipmentSlotType, EquipmentItemSO> OnItemEquipped;
        public event Action<EquipmentSlotType, EquipmentItemSO> OnItemUnequipped;

        public WeaponItemSO EquippedWeapon =>
            GetEquippedItem(EquipmentSlotType.MainHand) as WeaponItemSO;

        private void Awake()
        {
            if (attributeSystem == null)
            {
                attributeSystem = GetComponent<AttributeSystem>();
            }

            InitializeSlots();
        }

        private void InitializeSlots()
        {
            foreach (EquipmentSlotType slotType in Enum.GetValues(typeof(EquipmentSlotType)))
            {
                if (!slots.ContainsKey(slotType))
                {
                    slots[slotType] = new EquipmentSlot(slotType);
                }
            }
        }

        public EquipmentItemSO GetEquippedItem(EquipmentSlotType slotType)
        {
            if (slots.TryGetValue(slotType, out EquipmentSlot slot))
            {
                return slot.CurrentItem;
            }
            return null;
        }

        /// <summary>
        /// Equips an item into its designated slot. Any existing item in the slot is unequipped first.
        /// Automatically binds stat modifiers to the AttributeSystem and attaches visual meshes.
        /// </summary>
        public bool Equip(EquipmentItemSO item)
        {
            if (item == null)
                return false;

            InitializeSlots();

            EquipmentSlotType slotType = item.SlotType;
            EquipmentSlot slot = slots[slotType];

            // Unequip current item if present
            if (slot.IsOccupied)
            {
                Unequip(slotType);
            }

            slot.SetItem(item);

            // Apply stat modifiers to attribute system
            if (attributeSystem != null)
            {
                for (int i = 0; i < item.StatModifiers.Count; i++)
                {
                    StatModifierEntry entry = item.StatModifiers[i];
                    StatModifier mod = new StatModifier(entry.value, entry.modifierType, item);
                    attributeSystem.AddModifier(entry.attributeType, mod);
                }
            }

            // Instantiate visual mesh if socket found
            AttachVisualMesh(slot, item);

            OnItemEquipped?.Invoke(slotType, item);
            return true;
        }

        /// <summary>
        /// Unequips an item from the specified slot, removing its stat modifiers and destroying visual meshes.
        /// </summary>
        public EquipmentItemSO Unequip(EquipmentSlotType slotType)
        {
            InitializeSlots();

            if (!slots.TryGetValue(slotType, out EquipmentSlot slot) || !slot.IsOccupied)
                return null;

            EquipmentItemSO unequippedItem = slot.CurrentItem;

            // Remove modifiers from attribute system
            if (attributeSystem != null)
            {
                attributeSystem.RemoveAllModifiersFromSource(unequippedItem);
            }

            slot.Clear();

            OnItemUnequipped?.Invoke(slotType, unequippedItem);
            return unequippedItem;
        }

        private void AttachVisualMesh(EquipmentSlot slot, EquipmentItemSO item)
        {
            if (item.VisualPrefab == null)
                return;

            Transform socket = FindSocketTransform(item.AttachmentSocketName);
            Transform parentTransform = socket != null ? socket : transform;

            GameObject instance = Instantiate(item.VisualPrefab, parentTransform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            slot.VisualInstance = instance;
        }

        private Transform FindSocketTransform(string socketName)
        {
            if (string.IsNullOrEmpty(socketName))
                return null;

            Transform[] children = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name.Equals(socketName, StringComparison.OrdinalIgnoreCase))
                {
                    return children[i];
                }
            }

            return null;
        }
    }
}
