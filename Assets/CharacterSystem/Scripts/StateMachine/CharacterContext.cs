using CharacterSystem.Animation;
using CharacterSystem.Attributes;
using CharacterSystem.Combat;
using CharacterSystem.Core;
using CharacterSystem.Equipment;
using CharacterSystem.Input;
using UnityEngine;

namespace CharacterSystem.StateMachine
{
    /// <summary>
    /// Shared service context injected into character states.
    /// Eliminates expensive GetComponent lookups and maintains clean decoupling.
    /// </summary>
    public class CharacterContext
    {
        public CharacterController3D Controller { get; }
        public StateMachine StateMachine { get; }
        public AttributeSystem Attributes { get; }
        public EquipmentInventory Equipment { get; }
        public CharacterCombat Combat { get; }
        public CharacterAnimationController Animation { get; }
        public ICharacterInputProvider Input { get; set; }
        public Transform Transform { get; }
        public Transform CameraTransform { get; set; }

        // Cached States
        public IState IdleState { get; set; }
        public IState WalkState { get; set; }
        public IState SprintState { get; set; }
        public IState JumpState { get; set; }
        public IState FallState { get; set; }
        public IState LandState { get; set; }
        public IState AttackState { get; set; }
        public IState BlockState { get; set; }
        public IState HurtState { get; set; }
        public IState DeathState { get; set; }

        public CharacterContext(
            CharacterController3D controller,
            StateMachine stateMachine,
            AttributeSystem attributes,
            EquipmentInventory equipment,
            CharacterCombat combat,
            CharacterAnimationController animation,
            ICharacterInputProvider input,
            Transform transform,
            Transform cameraTransform)
        {
            Controller = controller;
            StateMachine = stateMachine;
            Attributes = attributes;
            Equipment = equipment;
            Combat = combat;
            Animation = animation;
            Input = input;
            Transform = transform;
            CameraTransform = cameraTransform;
        }
    }
}
