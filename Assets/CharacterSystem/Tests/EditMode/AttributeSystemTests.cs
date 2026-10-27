using CharacterSystem.Attributes;
using NUnit.Framework;
using UnityEngine;

namespace CharacterSystem.Tests.EditMode
{
    [TestFixture]
    public class AttributeSystemTests
    {
        private GameObject go;
        private AttributeSystem attributes;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("TestCharacter");
            attributes = go.AddComponent<AttributeSystem>();

            // Create in-memory stats SO
            var stats = ScriptableObject.CreateInstance<CharacterStatsSO>();
            typeof(CharacterStatsSO).GetField("maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(stats, 100f);
            typeof(CharacterStatsSO).GetField("maxStamina", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(stats, 100f);
            typeof(CharacterStatsSO).GetField("baseMoveSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(stats, 5.0f);
            typeof(CharacterStatsSO).GetField("baseAttackPower", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(stats, 20.0f);
            typeof(CharacterStatsSO).GetField("baseDefense", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(stats, 10.0f);

            attributes.InitializeFromStats(stats);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Initialization_CorrectlyAssignsBaseValues()
        {
            Assert.AreEqual(100f, attributes.MaxHealth);
            Assert.AreEqual(100f, attributes.CurrentHealth);
            Assert.AreEqual(100f, attributes.MaxStamina);
            Assert.AreEqual(5.0f, attributes.MoveSpeed);
            Assert.AreEqual(20.0f, attributes.AttackPower);
            Assert.AreEqual(10.0f, attributes.Defense);
            Assert.IsFalse(attributes.IsDead);
        }

        [Test]
        public void AddModifier_Flat_IncreasesCalculatedValue()
        {
            StatModifier flatMod = new StatModifier(15f, ModifierType.Flat);
            attributes.AddModifier(AttributeType.AttackPower, flatMod);

            // 20 + 15 = 35
            Assert.AreEqual(35f, attributes.AttackPower);
        }

        [Test]
        public void AddModifier_Percentage_MultipliesValue()
        {
            StatModifier percentMod = new StatModifier(0.5f, ModifierType.Percentage); // +50%
            attributes.AddModifier(AttributeType.AttackPower, percentMod);

            // 20 * 1.5 = 30
            Assert.AreEqual(30f, attributes.AttackPower);
        }

        [Test]
        public void AddModifier_CombinedFlatAndPercentage_AppliesCorrectFormula()
        {
            // Formula: (Base + Flat) * (1 + Percentage)
            StatModifier flatMod = new StatModifier(10f, ModifierType.Flat); // 20 + 10 = 30
            StatModifier percentMod = new StatModifier(0.2f, ModifierType.Percentage); // 30 * 1.2 = 36

            attributes.AddModifier(AttributeType.AttackPower, flatMod);
            attributes.AddModifier(AttributeType.AttackPower, percentMod);

            Assert.AreEqual(36f, attributes.AttackPower, 0.001f);
        }

        [Test]
        public void RemoveModifier_RestoresPreviousValue()
        {
            StatModifier mod = new StatModifier(25f, ModifierType.Flat);
            attributes.AddModifier(AttributeType.AttackPower, mod);
            Assert.AreEqual(45f, attributes.AttackPower);

            bool removed = attributes.RemoveModifier(AttributeType.AttackPower, mod);
            Assert.IsTrue(removed);
            Assert.AreEqual(20f, attributes.AttackPower);
        }

        [Test]
        public void ApplyDamage_WithDefenseMitigation_ReducesHealthCorrectly()
        {
            // RawDamage: 50, Defense: 10
            // Mitigation = 100 / (100 + 10) = 100 / 110 ≈ 0.90909
            // EffectiveDamage = 50 * 0.90909 ≈ 45.4545
            float rawDamage = 50f;
            float expectedMitigated = rawDamage * (100f / 110f);

            float damageTaken = attributes.ApplyDamage(rawDamage);

            Assert.AreEqual(expectedMitigated, damageTaken, 0.01f);
            Assert.AreEqual(100f - expectedMitigated, attributes.CurrentHealth, 0.01f);
        }

        [Test]
        public void ApplyDamage_ExceedingMaxHealth_TriggersDeathEventAndClampsAtZero()
        {
            bool deathFired = false;
            attributes.OnDeath += () => deathFired = true;

            attributes.ApplyDamage(500f);

            Assert.AreEqual(0f, attributes.CurrentHealth);
            Assert.IsTrue(attributes.IsDead);
            Assert.IsTrue(deathFired);
        }

        [Test]
        public void Heal_CannotExceedMaxHealth()
        {
            attributes.ApplyDamage(40f);
            Assert.Less(attributes.CurrentHealth, 100f);

            float healed = attributes.Heal(100f);

            Assert.AreEqual(100f, attributes.CurrentHealth);
            Assert.AreEqual(40f, healed, 0.01f);
        }

        [Test]
        public void ConsumeStamina_DeductsAccurately_AndRejectsInsufficientFunds()
        {
            bool success = attributes.ConsumeStamina(30f);
            Assert.IsTrue(success);
            Assert.AreEqual(70f, attributes.CurrentStamina);

            bool fail = attributes.ConsumeStamina(80f);
            Assert.IsFalse(fail);
            Assert.AreEqual(70f, attributes.CurrentStamina);
        }
    }
}
