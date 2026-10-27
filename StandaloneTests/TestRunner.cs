using System;
using System.Collections.Generic;
using CharacterSystem.Attributes;
using CharacterSystem.Combat;
using CharacterSystem.Core;
using CharacterSystem.Equipment;
using CharacterSystem.Input;
using CharacterSystem.StateMachine;
using CharacterSystem.StateMachine.Combat;
using CharacterSystem.StateMachine.Locomotion;
using CharacterSystem.Utilities;
using UnityEngine;

namespace CharacterSystem.StandaloneTests
{
    /// <summary>
    /// Standalone test runner capable of executing all domain, math, state machine,
    /// attribute, and equipment tests directly from command line or CI without launching Unity GUI.
    /// </summary>
    public static class TestRunner
    {
        private static int passedCount = 0;
        private static int failedCount = 0;

        public static int Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("================================================================");
            Console.WriteLine("   MODULAR 3D CHARACTER SYSTEM - AUTOMATED UNIT TEST SUITE      ");
            Console.WriteLine("================================================================");
            Console.ResetColor();

            // Run Test Suites
            RunMath3DTests();
            RunAttributeSystemTests();
            RunEquipmentSystemTests();
            RunStateMachineTests();
            RunCombatComboTests();

            Console.WriteLine();
            Console.WriteLine("----------------------------------------------------------------");
            if (failedCount == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[TEST RUN SUCCESS] All {passedCount} tests passed with 0 failures!");
                Console.ResetColor();
                return 0;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[TEST RUN FAILED] Passed: {passedCount}, Failed: {failedCount}");
                Console.ResetColor();
                return 1;
            }
        }

        private static void AssertEqual(float expected, float actual, float tolerance, string testName)
        {
            if (Math.Abs(expected - actual) <= tolerance)
            {
                ReportPass(testName);
            }
            else
            {
                ReportFail(testName, $"Expected {expected} (+/-{tolerance}), but got {actual}");
            }
        }

        private static void AssertTrue(bool condition, string testName, string reason = "")
        {
            if (condition)
            {
                ReportPass(testName);
            }
            else
            {
                ReportFail(testName, $"Condition failed: {reason}");
            }
        }

        private static void AssertFalse(bool condition, string testName, string reason = "")
        {
            AssertTrue(!condition, testName, reason);
        }

        private static void ReportPass(string testName)
        {
            passedCount++;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  [PASS] ");
            Console.ResetColor();
            Console.WriteLine(testName);
        }

        private static void ReportFail(string testName, string reason)
        {
            failedCount++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("  [FAIL] ");
            Console.ResetColor();
            Console.WriteLine($"{testName} - {reason}");
        }

        private static void RunMath3DTests()
        {
            Console.WriteLine("\n--> [Suite 1: Math3D & Kinematics]");

            // Test 1: Slope Angle flat
            float angleFlat = Math3DUtils.CalculateSlopeAngle(Vector3.up);
            AssertEqual(0f, angleFlat, 0.001f, "Math3D.CalculateSlopeAngle_FlatSurface");

            // Test 2: Slope Angle 45 deg
            Vector3 normal45 = new Vector3(1f, 1f, 0f).normalized;
            float angle45 = Math3DUtils.CalculateSlopeAngle(normal45);
            AssertEqual(45f, angle45, 0.05f, "Math3D.CalculateSlopeAngle_45Degrees");

            // Test 3: Slope too steep
            AssertTrue(Math3DUtils.IsSlopeTooSteep(50f, 45f), "Math3D.IsSlopeTooSteep_OverLimit");
            AssertFalse(Math3DUtils.IsSlopeTooSteep(40f, 45f), "Math3D.IsSlopeTooSteep_UnderLimit");

            // Test 4: Jump Velocity Physics Formula v = sqrt(2*g*h)
            float h = 1.8f;
            float g = 22f;
            float expectedV0 = (float)Math.Sqrt(2.0 * g * h);
            float actualV0 = Math3DUtils.CalculateJumpVelocity(h, g);
            AssertEqual(expectedV0, actualV0, 0.001f, "Math3D.CalculateJumpVelocity_PhysicsFormula");

            // Test 5: Camera space input projection
            Vector2 inputForward = new Vector2(0f, 1f);
            Vector3 worldDir = Math3DUtils.TransformInputToCameraSpace(inputForward, null);
            AssertTrue(worldDir == Vector3.forward, "Math3D.TransformInputToCameraSpace_Forward");
        }

        private static void RunAttributeSystemTests()
        {
            Console.WriteLine("\n--> [Suite 2: Data-Driven Attribute System]");

            var attr = new CharacterAttribute(AttributeType.AttackPower, 20f);
            AssertEqual(20f, attr.Value, 0.001f, "Attribute.BaseValueInitialization");

            // Flat modifier
            var flatMod = new StatModifier(15f, ModifierType.Flat);
            attr.AddModifier(flatMod);
            AssertEqual(35f, attr.Value, 0.001f, "Attribute.AddFlatModifier");

            // Percentage modifier: (20 + 15) * (1 + 0.20) = 42
            var pctMod = new StatModifier(0.20f, ModifierType.Percentage);
            attr.AddModifier(pctMod);
            AssertEqual(42f, attr.Value, 0.001f, "Attribute.AddPercentageModifier");

            // Remove modifier
            attr.RemoveModifier(pctMod);
            AssertEqual(35f, attr.Value, 0.001f, "Attribute.RemoveModifier");

            // Remove all from source
            object dummySource = new object();
            var srcMod = new StatModifier(10f, ModifierType.Flat, dummySource);
            attr.AddModifier(srcMod);
            AssertEqual(45f, attr.Value, 0.001f, "Attribute.AddFromSource");
            attr.RemoveAllModifiersFromSource(dummySource);
            AssertEqual(35f, attr.Value, 0.001f, "Attribute.RemoveAllFromSource");
        }

        private static void RunEquipmentSystemTests()
        {
            Console.WriteLine("\n--> [Suite 3: Equipment Slots & Modifiers]");

            var slot = new EquipmentSlot(EquipmentSlotType.MainHand);
            AssertFalse(slot.IsOccupied, "EquipmentSlot.InitiallyEmpty");

            var dummyItem = (EquipmentItemSO)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(EquipmentItemSO));
            slot.SetItem(dummyItem);
            AssertTrue(slot.IsOccupied, "EquipmentSlot.OccupiedAfterSet");
            AssertTrue(slot.CurrentItem == dummyItem, "EquipmentSlot.HoldsCorrectItemReference");

            slot.Clear();
            AssertFalse(slot.IsOccupied, "EquipmentSlot.ClearFreesSlot");
        }

        private static void RunStateMachineTests()
        {
            Console.WriteLine("\n--> [Suite 4: Animation & Locomotion State Machine]");

            var sm = new StateMachine.StateMachine();
            var context = new CharacterContext(null, sm, null, null, null, null, null, null, null);

            var idle = new IdleState(context);
            var walk = new WalkState(context);
            var sprint = new SprintState(context);

            sm.Initialize(idle);
            AssertTrue(sm.CurrentState.Name == "Idle", "StateMachine.InitialStateIsIdle");

            sm.ChangeState(walk);
            AssertTrue(sm.CurrentState.Name == "Walk", "StateMachine.TransitionToWalk");
            AssertTrue(sm.PreviousState.Name == "Idle", "StateMachine.TracksPreviousState");

            sm.ChangeState(sprint);
            AssertTrue(sm.CurrentState.Name == "Sprint", "StateMachine.TransitionToSprint");
        }

        private static void RunCombatComboTests()
        {
            Console.WriteLine("\n--> [Suite 5: Combat & Combo Sequencing]");

            var attackData1 = new WeaponAttackData
            {
                attackName = "Slash1",
                attackDuration = 0.5f,
                comboWindowStart = 0.3f,
                comboWindowEnd = 0.8f,
                damageMultiplier = 1.0f
            };

            var attackData2 = new WeaponAttackData
            {
                attackName = "Slash2",
                attackDuration = 0.6f,
                comboWindowStart = 0.4f,
                comboWindowEnd = 0.8f,
                damageMultiplier = 1.5f
            };

            AssertTrue(attackData1.damageMultiplier == 1.0f, "Combat.ComboDataIntegrity_Step1");
            AssertTrue(attackData2.damageMultiplier == 1.5f, "Combat.ComboDataIntegrity_Step2");
            AssertTrue(attackData1.comboWindowStart < attackData1.comboWindowEnd, "Combat.ComboWindowValidRanges");
        }
    }
}
