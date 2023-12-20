using UnityEngine;

namespace UnknownSpace
{
    [RequireComponent (typeof (Rigidbody))]
    public class SpaceShip : Destructible
    {
        /// <summary>
        /// Масса для автоматической установки в Rigidbody.
        /// </summary>
        [SerializeField] private float m_Mass;

        /// <summary>
        /// Толкающая вперед сила.
        /// </summary>
        [Space]
        [SerializeField] private float m_Thrust;
        /// <summary>
        /// Вращающая сила.
        /// </summary>
        [SerializeField] private float m_Mobility;

        /// <summary>
        /// Максимальная линейная скорость
        /// </summary>
        [Space]
        [SerializeField] private float m_MaxLinearVelocity;
        /// <summary>
        /// Максимальная вращательная скорость. В градусах/сек.
        /// </summary>
        [SerializeField] private float m_MaxAngularVelocity;

        /// <summary>
        /// Скорость замедления движения корабля.
        /// </summary>
        [Space]
        [SerializeField] private float m_LinearVelocityDecelerationMult = 2;
        /// <summary>
        /// Скорость замедления поворотов корабля.
        /// </summary>
        [SerializeField] private float m_AngularVelocityDeceleration;

        /// <summary>
        /// Управление линейной тягой. (от -1.0 до 1.0)
        /// </summary>
        public float ThrustControl { get; set; }
        /// <summary>
        /// Управление горизонтальной вращательной тягой. (от -1.0 до 1.0)
        /// </summary>
        public float HorizontalTorqueControl { get; set; }
        /// <summary>
        /// Управление вертикальной вращательной тягой. (от -1.0 до 1.0)
        /// </summary>
        public float VerticalTorqueControl { get; set; }
        /// <summary>
        /// Управление наклонной вращательной тягой. (от -1.0 до 1.0)
        /// </summary>
        public float InclineTorqueControl { get; set; }

        public float Thrust => m_Thrust;
        public float Mobility => m_Mobility;
        public float LinearVelocity => m_Rigidbody.velocity.magnitude;
        public float MaxLinearVelocity => m_MaxLinearVelocity;
        public float MinAngularVelocity => m_MaxAngularVelocity;

        private Rigidbody m_Rigidbody;

        // DEBUG
        [Header("DEBUG")]
        public float Speed;
        public float RotationSpeed;
        float speedValue = 0;

        protected override void Awake()
        {
            base.Awake();

            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.mass = m_Mass;
        }

        private void Update()
        {
            Speed = LinearVelocity;
            RotationSpeed = m_Rigidbody.angularVelocity.magnitude;
        }

        private void FixedUpdate()
        {
            UpdateRigidbody();
        }

        /// <summary>
        /// Метод добавления сил кораблю для движения.
        /// </summary>
        private void UpdateRigidbody()
        {
            // ЗАГОТОВКА ПОД ДВИЖЕНИЕ КОРАБЛЯ //
            if (m_Rigidbody.velocity.magnitude < m_MaxLinearVelocity)
                speedValue += ThrustControl * m_Thrust * Time.fixedDeltaTime;

            if (ThrustControl == 0 && speedValue > 0)
                speedValue -= m_Thrust * m_LinearVelocityDecelerationMult * Time.fixedDeltaTime;

            m_Rigidbody.velocity = speedValue * transform.forward * Time.fixedDeltaTime;
            ////////////////////////////////////

            // Ускорение поворота по горизонтальной оси с ограничением по скорости
            if (m_Rigidbody.angularVelocity.magnitude < m_MaxAngularVelocity)
            {
                m_Rigidbody.AddTorque(transform.up * HorizontalTorqueControl * m_Mobility * Time.fixedDeltaTime, ForceMode.Force);
                m_Rigidbody.AddTorque(transform.right * VerticalTorqueControl * m_Mobility * Time.fixedDeltaTime, ForceMode.Force);
                m_Rigidbody.AddTorque(transform.forward * InclineTorqueControl * m_Mobility * Time.fixedDeltaTime, ForceMode.Force);
            }

            // Замедление поворотов
            m_Rigidbody.AddTorque(-m_Rigidbody.angularVelocity * m_AngularVelocityDeceleration * Time.fixedDeltaTime, ForceMode.Force);
        }
    }
}
