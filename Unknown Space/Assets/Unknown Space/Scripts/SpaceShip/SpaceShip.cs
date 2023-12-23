using UnityEngine;

namespace UnknownSpace
{
    [RequireComponent (typeof (Rigidbody))]
    public class SpaceShip : Destructible
    {
        public const float ALIGN_ON_HORIZON_SPEED_MULT = 0.02f;

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
        public float NormalizeLinearVelocity => m_Rigidbody.velocity.magnitude / m_MaxLinearVelocity;
        public float MaxLinearVelocity => m_MaxLinearVelocity;
        public float MinAngularVelocity => m_MaxAngularVelocity;

        private Rigidbody m_Rigidbody;

        private float m_LinearVelocityValue = 0;

        private bool alignOnHorizon;
        private float alignOnHorizonThreshold = 0.05f;

        // DEBUG
        [Header("DEBUG")]
        public float Speed;
        public float RotationSpeed;

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

            AlignOnHorizonControl();
        }

        private void FixedUpdate()
        {
            UpdateRigidbody();
            AlignOnHorizon();
        }

        private void UpdateRigidbody()
        {
            LinearMove();
            AngularMove();
        }

        /// <summary>
        /// Движение корабля с помощью жесткого задания вектора velocity.
        /// </summary>
        private void LinearMove()
        {
            // Разгон
            if (m_LinearVelocityValue < m_MaxLinearVelocity)
                m_LinearVelocityValue += ThrustControl * m_Thrust * Time.fixedDeltaTime;
            else
                m_LinearVelocityValue = m_MaxLinearVelocity;

            // Замедление
            if (ThrustControl == 0)
            {
                if (m_LinearVelocityValue > 0)
                    m_LinearVelocityValue -= m_Thrust * m_LinearVelocityDecelerationMult * Time.fixedDeltaTime;
                else
                    m_LinearVelocityValue = 0;
            }

            m_Rigidbody.velocity = transform.forward * m_LinearVelocityValue;
        }

        /// <summary>
        /// Вращение корабля с помощью добавления вращательной силы AddTorque.
        /// </summary>
        private void AngularMove()
        {
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

        /// <summary>
        /// Выравнивание корабля по горизонту.
        /// </summary>
        private void AlignOnHorizon()
        {
            if (alignOnHorizon == false) return;

            float rotationX = Mathf.MoveTowardsAngle(transform.rotation.eulerAngles.x, 0f, Time.deltaTime * m_Mobility * ALIGN_ON_HORIZON_SPEED_MULT);
            float rotationZ = Mathf.MoveTowardsAngle(transform.rotation.eulerAngles.z, 0f, Time.deltaTime * m_Mobility * ALIGN_ON_HORIZON_SPEED_MULT);

            if ((rotationX > 360f - alignOnHorizonThreshold || rotationX < alignOnHorizonThreshold) &&
                (rotationZ > 360f - alignOnHorizonThreshold || rotationZ < alignOnHorizonThreshold))
            {
                transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
                alignOnHorizon = false;
                return;
            }

            transform.rotation = Quaternion.Euler(rotationX, transform.rotation.eulerAngles.y, rotationZ);
        }

        /// <summary>
        /// Управление Включением/выключением выравнивания корабля по горизонту.
        /// </summary>
        private void AlignOnHorizonControl()
        {
            if (Input.GetKeyDown(KeyCode.V) && alignOnHorizon == false)
                alignOnHorizon = true;

            if (HorizontalTorqueControl != 0 || VerticalTorqueControl != 0 || InclineTorqueControl != 0)
                alignOnHorizon = false;
        }
    }
}
