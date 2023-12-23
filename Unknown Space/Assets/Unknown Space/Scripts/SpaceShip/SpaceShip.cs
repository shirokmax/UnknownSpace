using UnityEngine;

namespace UnknownSpace
{
    [RequireComponent (typeof (Rigidbody))]
    public class SpaceShip : Destructible
    {
        /// <summary>
        /// ����� ��� �������������� ��������� � Rigidbody.
        /// </summary>
        [SerializeField] private float m_Mass;

        /// <summary>
        /// ��������� ������ ����.
        /// </summary>
        [Space]
        [SerializeField] private float m_Thrust;
        /// <summary>
        /// ��������� ����.
        /// </summary>
        [SerializeField] private float m_Mobility;

        /// <summary>
        /// ������������ �������� ��������
        /// </summary>
        [Space]
        [SerializeField] private float m_MaxLinearVelocity;
        /// <summary>
        /// ������������ ������������ ��������. � ��������/���.
        /// </summary>
        [SerializeField] private float m_MaxAngularVelocity;

        /// <summary>
        /// �������� ���������� �������� �������.
        /// </summary>
        [Space]
        [SerializeField] private float m_LinearVelocityDecelerationMult = 2;
        /// <summary>
        /// �������� ���������� ��������� �������.
        /// </summary>
        [SerializeField] private float m_AngularVelocityDeceleration;

        /// <summary>
        /// ���������� �������� �����. (�� -1.0 �� 1.0)
        /// </summary>
        public float ThrustControl { get; set; }
        /// <summary>
        /// ���������� �������������� ������������ �����. (�� -1.0 �� 1.0)
        /// </summary>
        public float HorizontalTorqueControl { get; set; }
        /// <summary>
        /// ���������� ������������ ������������ �����. (�� -1.0 �� 1.0)
        /// </summary>
        public float VerticalTorqueControl { get; set; }
        /// <summary>
        /// ���������� ��������� ������������ �����. (�� -1.0 �� 1.0)
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
        public float m_LinearVelocityValue = 0;

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

        private void UpdateRigidbody()
        {
            LinearMove();
            AngularMove();
        }

        /// <summary>
        /// �������� ������� � ������� �������� ������� ������� velocity.
        /// </summary>
        private void LinearMove()
        {
            // ������
            if (m_LinearVelocityValue < m_MaxLinearVelocity)
                m_LinearVelocityValue += ThrustControl * m_Thrust * Time.fixedDeltaTime;
            else
                m_LinearVelocityValue = m_MaxLinearVelocity;

            // ����������
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
        /// �������� ������� � ������� ���������� ������������ ���� AddTorque.
        /// </summary>
        private void AngularMove()
        {
            // ��������� �������� �� �������������� ��� � ������������ �� ��������
            if (m_Rigidbody.angularVelocity.magnitude < m_MaxAngularVelocity)
            {
                m_Rigidbody.AddTorque(transform.up * HorizontalTorqueControl * m_Mobility * Time.fixedDeltaTime, ForceMode.Force);
                m_Rigidbody.AddTorque(transform.right * VerticalTorqueControl * m_Mobility * Time.fixedDeltaTime, ForceMode.Force);
                m_Rigidbody.AddTorque(transform.forward * InclineTorqueControl * m_Mobility * Time.fixedDeltaTime, ForceMode.Force);
            }

            // ���������� ���������
            m_Rigidbody.AddTorque(-m_Rigidbody.angularVelocity * m_AngularVelocityDeceleration * Time.fixedDeltaTime, ForceMode.Force);
        }
    }
}
