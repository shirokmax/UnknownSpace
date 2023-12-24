using UnityEngine;

namespace UnknownSpace
{
    public class Bursts : MonoBehaviour, IDependency<SpaceShip>
    {
        [SerializeField] private ParticleSystem[] m_Particles;
        [SerializeField] private float m_MaxSpeed = 0.2f;

        private SpaceShip m_Ship;
        public void Construct(SpaceShip obj) => m_Ship = obj;

        private ParticleSystem.MainModule[] m_BurstsMains;

        private void Awake()
        {
            m_BurstsMains = new ParticleSystem.MainModule[m_Particles.Length];

            for (int i = 0; i < m_Particles.Length; i++)
                m_BurstsMains[i] = m_Particles[i].main;
        }

        private void Update()
        {
            for (int i = 0; i < m_BurstsMains.Length; i++)
                m_BurstsMains[i].startSpeed = m_Ship.ThrustControl * m_MaxSpeed;
        }
    }
}