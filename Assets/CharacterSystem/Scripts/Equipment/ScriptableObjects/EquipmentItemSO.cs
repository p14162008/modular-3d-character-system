using System;
using System.Collections.Generic;
using CharacterSystem.Attributes;
using UnityEngine;

namespace CharacterSystem.Equipment
{
    [Serializable]
    public struct StatModifierEntry
    {
        public AttributeType attributeType;
        public ModifierType modifierType;
        public float value;
    }

    /// <summary>
    /// Base ScriptableObject asset for equippable gear (Armor, Accessories, Weapons).
    /// </summary>
    [CreateAssetMenu(fileName = "NewEquipmentItem", menuName = "Character System/Equipment/Equipment Item Asset")]
    public class EquipmentItemSO : ScriptableObject
    {
        [Header("Item Identity")]
        [SerializeField] private string itemId = "item_id";
        [SerializeField] private string itemName = "New Equipment";
        [TextArea(2, 3)]
        [SerializeField] private string description = "Equipment description.";
        [SerializeField] private EquipmentSlotType slotType = EquipmentSlotType.Chest;

        [Header("Stat Modifiers")]
        [SerializeField] private List<StatModifierEntry> statModifiers = new List<StatModifierEntry>();

        [Header("Visual Representation")]
        [Tooltip("Prefab instantiated at character socket or attached to skeletal mesh.")]
        [SerializeField] private GameObject visualPrefab;

        [Tooltip("Name of the transform socket on the character hierarchy.")]
        [SerializeField] private string attachmentSocketName = "Weapon_R";

        public string ItemId => itemId;
        public string ItemName => itemName;
        public string Description => description;
        public EquipmentSlotType SlotType => slotType;
        public IReadOnlyList<StatModifierEntry> StatModifiers => statModifiers;
        public GameObject VisualPrefab => visualPrefab;
        public string AttachmentSocketName => attachmentSocketName;
    }
}
