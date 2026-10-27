using CharacterSystem.Attributes;
using CharacterSystem.Combat;
using CharacterSystem.Equipment;
using NUnit.Framework;
using UnityEngine;

namespace CharacterSystem.Tests.EditMode
{
    [TestFixture]
    public class CombatComboTests
    {
        private GameObject go;
        private AttributeSystem attributes;
        private CharacterCombat combat;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("TestCombatEntity");
            attributes = go.AddComponent<AttributeSystem>();
            combat = go.AddComponent<CharacterCombat>();

            var stats = ScriptableObject.CreateInstance<CharacterStatsSO>();
            attributes.InitializeFromStats(stats);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        [Test]
        public void InitialState_CanInitiateAttack()
        {
            Assert.IsTrue(combat.CanInitiateAttack());
            Assert.IsFalse(combat.IsAttacking);
            Assert.AreEqual(0, combat.CurrentComboIndex);
        }

        [Test]
        public void TriggerAttack_StartsComboStepZero()
        {
            bool triggered = combat.TriggerAttack();

            Assert.IsTrue(triggered);
            Assert.IsTrue(combat.IsAttacking);
            Assert.AreEqual(0, combat.CurrentComboIndex);
        }

        [Test]
        public void ComboChaining_BuffersAndAdvancesToNextStep()
        {
            combat.TriggerAttack();
            Assert.AreEqual(0, combat.CurrentComboIndex);

            var seq = combat.GetCurrentComboSequence();
            var step0 = seq[0];

            // Advance time into combo window (e.g. 50% into attack)
            float advanceTime = step0.attackDuration * 0.5f;
            combat.UpdateCombat(advanceTime);

            // Buffer second strike
            bool buffered = combat.TriggerAttack();
            Assert.IsTrue(buffered);

            // Advance past combo window start so chain triggers
            combat.UpdateCombat(0.05f);

            Assert.AreEqual(1, combat.CurrentComboIndex, "Should advance to combo step 1 (Right Cross)");
            Assert.IsTrue(combat.IsAttacking);
        }

        [Test]
        public void ComboTimeout_ResetsComboIndexToZero()
        {
            combat.TriggerAttack();
            Assert.IsTrue(combat.IsAttacking);

            var seq = combat.GetCurrentComboSequence();
            // Advance past total duration without buffering
            combat.UpdateCombat(seq[0].attackDuration + 0.1f);

            Assert.IsFalse(combat.IsAttacking);
            Assert.AreEqual(0, combat.CurrentComboIndex);
        }

        [Test]
        public void ActiveBlock_AbsorbsFrontalDamageAndMitigatesImpact()
        {
            combat.IsBlocking = true;

            DamageData frontalHit = new DamageData(
                50f,
                go.transform.position + go.transform.forward,
                -go.transform.forward, // attacker in front
                10f,
                false,
                null);

            float damageTaken = combat.TakeDamage(frontalHit);

            // With default block mitigation 75%, damage should be reduced significantly
            Assert.Less(damageTaken, 20f);
        }
    }
}
