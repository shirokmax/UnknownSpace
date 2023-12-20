using UnityEngine;

namespace UnknownSpace
{
    public class SpaceShipController : MonoBehaviour
    {
        [SerializeField] private SpaceShip m_TargetShip;

        private void Update()
        {
            float inclineTorque = 0;

            if (Input.GetKey(KeyCode.Q))
                inclineTorque = 1.0f;

            if (Input.GetKey(KeyCode.E))
                inclineTorque = -1.0f;

            m_TargetShip.ThrustControl = Input.GetAxis("Jump");
            m_TargetShip.HorizontalTorqueControl = Input.GetAxis("Horizontal");
            m_TargetShip.VerticalTorqueControl = Input.GetAxis("Vertical");
            m_TargetShip.InclineTorqueControl = inclineTorque;
        }

        public void SetTargetShip(SpaceShip ship)
        {
            m_TargetShip = ship;
        }
    }
}
