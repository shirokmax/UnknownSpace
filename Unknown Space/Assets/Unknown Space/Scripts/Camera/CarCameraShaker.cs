using UnityEngine;

namespace UnknownSpace
{
    [RequireComponent(typeof(Camera))]
    public class CarCameraShaker : MonoBehaviour
    {
        [SerializeField] private SpaceShip m_Ship;
        [SerializeField][Range(0f, 1f)] private float m_NormalizeSpeedShakeStart;
        [SerializeField] private float m_ShakeAmplitude;
        [SerializeField] private float m_ShakeAmount;

        private void Update()
        {
            if (m_Ship.NormalizeLinearVelocity >= m_NormalizeSpeedShakeStart)
            {
                Vector3 newRandPosition = transform.position + Random.insideUnitSphere * m_ShakeAmplitude * m_Ship.NormalizeLinearVelocity;

                transform.position = Vector3.Lerp(transform.position, newRandPosition, m_ShakeAmount * Time.deltaTime);
            }
        }
    }
}