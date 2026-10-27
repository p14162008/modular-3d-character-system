using CharacterSystem.Attributes;
using CharacterSystem.Core;
using CharacterSystem.Input;
using CharacterSystem.StateMachine;
using CharacterSystem.StateMachine.Combat;
using CharacterSystem.StateMachine.Locomotion;
using NUnit.Framework;
using UnityEngine;

namespace CharacterSystem.Tests.EditMode
{
    [TestFixture]
    public class StateMachineTests
    {
        private GameObject go;
        private CharacterController3D controller;
        private AttributeSystem attributes;
        private AICharacterInput aiInput;
        private StateMachine.StateMachine stateMachine;
        private CharacterContext context;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("TestSMCharacter");
            controller = go.AddComponent<CharacterController3D>();
            attributes = go.AddComponent<AttributeSystem>();
            aiInput = go.AddComponent<AICharacterInput>();

            // Setup movement config
            var moveConfig = ScriptableObject.CreateInstance<MovementConfigSO>();
            controller.SetConfiguration(moveConfig);

            // Setup stats
            var stats = ScriptableObject.CreateInstance<CharacterStatsSO>();
            attributes.InitializeFromStats(stats);

            stateMachine = new StateMachine.StateMachine();
            context = new CharacterContext(
                controller,
                stateMachine,
                attributes,
                null,
                null,
                null,
                aiInput,
                go.transform,
                null);

            context.IdleState = new IdleState(context);
            context.WalkState = new WalkState(context);
            context.SprintState = new SprintState(context);
            context.JumpState = new JumpState(context);
            context.FallState = new FallState(context);
            context.LandState = new LandState(context);
            context.DeathState = new DeathState(context);

            stateMachine.Initialize(context.IdleState);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        [Test]
        public void StateMachine_StartsInIdleState()
        {
            Assert.AreEqual("Idle", stateMachine.CurrentState.Name);
        }

        [Test]
        public void MovementInput_TransitionsFromIdleToWalk()
        {
            aiInput.MoveInput = new Vector2(0f, 1f); // Forward
            stateMachine.Update(0.016f);

            Assert.AreEqual("Walk", stateMachine.CurrentState.Name);
        }

        [Test]
        public void SprintHeld_TransitionsFromWalkToSprint()
        {
            aiInput.MoveInput = new Vector2(0f, 1f);
            aiInput.IsSprintHeld = true;
            stateMachine.Update(0.016f);

            Assert.AreEqual("Sprint", stateMachine.CurrentState.Name);
        }

        [Test]
        public void StoppingInput_TransitionsBackToIdle()
        {
            aiInput.MoveInput = new Vector2(0f, 1f);
            stateMachine.Update(0.016f);
            Assert.AreEqual("Walk", stateMachine.CurrentState.Name);

            aiInput.MoveInput = Vector2.zero;
            stateMachine.Update(0.016f);
            Assert.AreEqual("Idle", stateMachine.CurrentState.Name);
        }

        [Test]
        public void Death_TransitionsImmediatelyToDeathStateAndLocks()
        {
            attributes.ApplyDamage(9999f);
            stateMachine.Update(0.016f);

            Assert.AreEqual("Death", stateMachine.CurrentState.Name);

            // Cannot transition out even if input arrives
            aiInput.MoveInput = new Vector2(0f, 1f);
            stateMachine.Update(0.016f);
            Assert.AreEqual("Death", stateMachine.CurrentState.Name);
        }
    }
}
