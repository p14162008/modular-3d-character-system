using CharacterSystem.Animation;
using CharacterSystem.Attributes;
using CharacterSystem.Combat;
using CharacterSystem.Equipment;
using CharacterSystem.Input;
using CharacterSystem.StateMachine;
using CharacterSystem.StateMachine.Combat;
using CharacterSystem.StateMachine.Locomotion;
using UnityEngine;

namespace CharacterSystem.Core
{
    /// <summary>
    /// Master root orchestrator unifying Motor, Hierarchical State Machine,
    /// Data-Driven Attributes, Equipment, and Combat into a single modular actor.
    /// </summary>
    [SelectionBase]
    [RequireComponent(typeof(CharacterController3D))]
    public class ModularCharacter : MonoBehaviour
    {
        [Header("Archetype & Data Assets")]
        [Tooltip("Optional archetype bundle configuring stats, movement physics, and starting equipment.")]
        [SerializeField] private ClassArchetypeSO archetype;

        [Tooltip("Explicit stats asset if archetype is not specified.")]
        [SerializeField] private CharacterStatsSO fallbackStats;

        [Tooltip("Explicit movement config if archetype is not specified.")]
        [SerializeField] private MovementConfigSO fallbackMovement;

        [Header("Subsystems")]
        [SerializeField] private CharacterController3D controller;
        [SerializeField] private AttributeSystem attributes;
        [SerializeField] private EquipmentInventory equipment;
        [SerializeField] private CharacterCombat combat;
        [SerializeField] private CharacterAnimationController animationController;
        [SerializeField] private MonoBehaviour inputProviderComponent;

        [Header("Camera Anchor")]
        [Tooltip("Optional camera transform used for camera-relative movement calculations. Defaults to Camera.main.")]
        [SerializeField] private Transform cameraTransform;

        // Runtime State
        private StateMachine.StateMachine stateMachine;
        private CharacterContext context;
        private ICharacterInputProvider inputProvider;

        public StateMachine.StateMachine StateMachine => stateMachine;
        public CharacterContext Context => context;
        public ICharacterController Motor => controller;
        public AttributeSystem Attributes => attributes;
        public EquipmentInventory Equipment => equipment;
        public CharacterCombat Combat => combat;
        public CharacterAnimationController Animation => animationController;
        public ICharacterInputProvider InputProvider => inputProvider;

        private void Awake()
        {
            ResolveDependencies();
            ApplyArchetypeOrConfigs();
            InitializeStateMachine();
        }

        private void Start()
        {
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
                if (context != null)
                {
                    context.CameraTransform = cameraTransform;
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // Tick state logic
            stateMachine?.Update(dt);

            // Tick physical motor displacement
            if (controller != null)
            {
                controller.TickMotor(dt);
            }
        }

        private void FixedUpdate()
        {
            stateMachine?.FixedUpdate(Time.fixedDeltaTime);
        }

        private void ResolveDependencies()
        {
            if (controller == null)
                controller = GetComponent<CharacterController3D>();

            if (attributes == null)
            {
                attributes = GetComponent<AttributeSystem>();
                if (attributes == null)
                    attributes = gameObject.AddComponent<AttributeSystem>();
            }

            if (equipment == null)
            {
                equipment = GetComponent<EquipmentInventory>();
                if (equipment == null)
                    equipment = gameObject.AddComponent<EquipmentInventory>();
            }

            if (combat == null)
            {
                combat = GetComponent<CharacterCombat>();
                if (combat == null)
                    combat = gameObject.AddComponent<CharacterCombat>();
            }

            if (animationController == null)
            {
                animationController = GetComponentInChildren<CharacterAnimationController>();
            }

            if (inputProviderComponent is ICharacterInputProvider provider)
            {
                inputProvider = provider;
            }
            else
            {
                inputProvider = GetComponent<ICharacterInputProvider>();
                if (inputProvider == null)
                {
                    inputProvider = gameObject.AddComponent<PlayerCharacterInput>();
                }
            }
        }

        private void ApplyArchetypeOrConfigs()
        {
            CharacterStatsSO activeStats = archetype != null ? archetype.StatsAsset : fallbackStats;
            MovementConfigSO activeMovement = archetype != null ? archetype.MovementConfigAsset : fallbackMovement;

            if (activeStats != null && attributes != null)
            {
                attributes.InitializeFromStats(activeStats);
            }

            if (activeMovement != null && controller != null)
            {
                controller.SetConfiguration(activeMovement);
            }

            // Equip starting archetype gear
            if (archetype != null && equipment != null && archetype.StartingEquipment != null)
            {
                for (int i = 0; i < archetype.StartingEquipment.Count; i++)
                {
                    equipment.Equip(archetype.StartingEquipment[i]);
                }
            }
        }

        private void InitializeStateMachine()
        {
            stateMachine = new StateMachine.StateMachine();

            context = new CharacterContext(
                controller,
                stateMachine,
                attributes,
                equipment,
                combat,
                animationController,
                inputProvider,
                transform,
                cameraTransform
            );

            // Instantiate all states once (zero GC allocation during runtime gameplay)
            context.IdleState = new IdleState(context);
            context.WalkState = new WalkState(context);
            context.SprintState = new SprintState(context);
            context.JumpState = new JumpState(context);
            context.FallState = new FallState(context);
            context.LandState = new LandState(context);
            context.AttackState = new AttackState(context);
            context.BlockState = new BlockState(context);
            context.HurtState = new HurtState(context);
            context.DeathState = new DeathState(context);

            // Bind combat events to state machine
            if (combat != null)
            {
                combat.OnHitReceived += HandleHitReceived;
            }

            if (attributes != null)
            {
                attributes.OnDeath += HandleDeath;
            }

            // Start in Idle State
            stateMachine.Initialize(context.IdleState);
        }

        private void HandleHitReceived(DamageData damage)
        {
            if (attributes != null && attributes.IsDead)
            {
                stateMachine.ChangeState(context.DeathState);
                return;
            }

            if (context.HurtState is HurtState hurtState)
            {
                hurtState.SetHitData(damage);
                stateMachine.ChangeState(hurtState);
            }
        }

        private void HandleDeath()
        {
            stateMachine.ChangeState(context.DeathState);
        }

        private void OnDestroy()
        {
            if (combat != null)
            {
                combat.OnHitReceived -= HandleHitReceived;
            }

            if (attributes != null)
            {
                attributes.OnDeath -= HandleDeath;
            }
        }

        /// <summary>
        /// Allows swapping the input provider at runtime (e.g. possessed by player, switched to AI).
        /// </summary>
        public void SetInputProvider(ICharacterInputProvider newProvider)
        {
            inputProvider = newProvider;
            if (context != null)
            {
                context.Input = newProvider;
            }
        }
    }
}
