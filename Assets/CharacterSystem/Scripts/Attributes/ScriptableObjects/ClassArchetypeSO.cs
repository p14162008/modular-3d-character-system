using System.Collections.Generic;
using CharacterSystem.Equipment;
using UnityEngine;

namespace CharacterSystem.Attributes
{
    /// <summary>
    /// Bundles all baseline configuration data for a character class/archetype.
    /// Designers can drag and drop this onto any character prefab to fully reconfigure their role.
    /// </summary>
    [CreateAssetMenu(fileName = "NewClassArchetype", menuName = "Character System/Stats/Class Archetype Asset")]
    public class ClassArchetypeSO : ScriptableObject
    {
        [SerializeField] private string archetypeName = "Warrior";
        [TextArea(2, 4)]
        [SerializeField] private string description = "A resilient front-line combatant.";

        [Header("Configurations")]
        [SerializeField] private CharacterStatsSO statsAsset;
        [SerializeField] private MovementConfigSO movementConfigAsset;

        [Header("Starting Equipment")]
        [SerializeField] private List<EquipmentItemSO> startingEquipment = new List<EquipmentItemSO>();

        public string ArchetypeName => archetypeName;
        public string Description => description;
        public CharacterStatsSO StatsAsset => statsAsset;
        public MovementConfigSO MovementConfigAsset => movementConfigAsset;
        public IReadOnlyList<EquipmentItemSO> StartingEquipment => startingEquipment;
    }
}
