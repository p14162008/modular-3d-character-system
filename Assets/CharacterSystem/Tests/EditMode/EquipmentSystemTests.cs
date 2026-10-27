using CharacterSystem.Attributes;
using CharacterSystem.Equipment;
using NUnit.Framework;
using UnityEngine;

namespace CharacterSystem.Tests.EditMode
{
    [TestFixture]
    public class EquipmentSystemTests
    {
        private GameObject go;
        private AttributeSystem attributes;
        private EquipmentInventory equipment;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("TestEquipmentCharacter");
            attributes = go.AddComponent<AttributeSystem>();
            equipment = go.AddComponent<EquipmentInventory>();

            var stats = ScriptableObject.CreateInstance<CharacterStatsSO>();
            typeof(CharacterStatsSO).GetField("maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(stats, 100f);
            typeof(CharacterStatsSO).GetField("baseAttackPower", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(stats, 20f);
            typeof(CharacterStatsSO).GetField("baseDefense", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(stats, 10f);

            attributes.InitializeFromStats(stats);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        [Test]
        public void EquipWeapon_AppliesStatModifiersToAttributes()
        {
            var sword = ScriptableObject.CreateInstance<WeaponItemSO>();
            typeof(EquipmentItemSO).GetField("slotType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(sword, EquipmentSlotType.MainHand);

            var mods = (System.Collections.Generic.List<StatModifierEntry>)typeof(EquipmentItemSO)
                .GetField("statModifiers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(sword);

            mods.Add(new StatModifierEntry
            {
                attributeType = AttributeType.AttackPower,
                modifierType = ModifierType.Flat,
                value = 30f
            });

            equipment.Equip(sword);

            // 20 base + 30 sword = 50
            Assert.AreEqual(50f, attributes.AttackPower);
            Assert.AreEqual(sword, equipment.EquippedWeapon);
        }

        [Test]
        public void UnequipWeapon_RemovesStatModifiersCleanly()
        {
            var sword = ScriptableObject.CreateInstance<WeaponItemSO>();
            typeof(EquipmentItemSO).GetField("slotType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(sword, EquipmentSlotType.MainHand);

            var mods = (System.Collections.Generic.List<StatModifierEntry>)typeof(EquipmentItemSO)
                .GetField("statModifiers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(sword);

            mods.Add(new StatModifierEntry
            {
                attributeType = AttributeType.AttackPower,
                modifierType = ModifierType.Flat,
                value = 30f
            });

            equipment.Equip(sword);
            Assert.AreEqual(50f, attributes.AttackPower);

            equipment.Unequip(EquipmentSlotType.MainHand);
            Assert.AreEqual(20f, attributes.AttackPower);
            Assert.IsNull(equipment.EquippedWeapon);
        }

        [Test]
        public void EquipOverOccupiedSlot_ReplacesItemAndRecalculatesModifiers()
        {
            var sword1 = ScriptableObject.CreateInstance<WeaponItemSO>();
            typeof(EquipmentItemSO).GetField("slotType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(sword1, EquipmentSlotType.MainHand);
            var mods1 = (System.Collections.Generic.List<StatModifierEntry>)typeof(EquipmentItemSO)
                .GetField("statModifiers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(sword1);
            mods1.Add(new StatModifierEntry { attributeType = AttributeType.AttackPower, modifierType = ModifierType.Flat, value = 15f });

            var sword2 = ScriptableObject.CreateInstance<WeaponItemSO>();
            typeof(EquipmentItemSO).GetField("slotType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(sword2, EquipmentSlotType.MainHand);
            var mods2 = (System.Collections.Generic.List<StatModifierEntry>)typeof(EquipmentItemSO)
                .GetField("statModifiers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(sword2);
            mods2.Add(new StatModifierEntry { attributeType = AttributeType.AttackPower, modifierType = ModifierType.Flat, value = 40f });

            equipment.Equip(sword1);
            Assert.AreEqual(35f, attributes.AttackPower); // 20 + 15

            equipment.Equip(sword2);
            Assert.AreEqual(60f, attributes.AttackPower); // 20 + 40 (15 removed!)
            Assert.AreEqual(sword2, equipment.EquippedWeapon);
        }
    }
}
