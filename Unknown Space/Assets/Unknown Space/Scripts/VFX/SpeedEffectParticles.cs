using UnityEngine;

namespace UnknownSpace
{
    [RequireComponent (typeof (ParticleSystem))]
    public class SpeedEffectParticles : MonoBehaviour, IDependency<SpaceShip>
    {
        [SerializeField] private float m_ParticlesMaxSpeed;
        [SerializeField] private float m_MaxLifeTime = 6;
        [SerializeField] private float m_MaxEmission = 100;

        [Space]
        [SerializeField] private AnimationCurve m_LifeTimeCurve;
        [SerializeField] private AnimationCurve m_EmissionCurve;

        private SpaceShip m_Ship;
        public void Construct(SpaceShip obj) => m_Ship = obj;

        private ParticleSystem m_Particles;
        private ParticleSystem.MainModule m_Main;
        private ParticleSystem.EmissionModule m_Emission;

        private void Awake()
        {
            m_Particles = GetComponent<ParticleSystem>();
            m_Main = m_Particles.main;
            m_Emission = m_Particles.emission;
        }

        private void Update()
        {
            float particlesCurrentSpeed = m_Ship.LinearVelocity * (m_ParticlesMaxSpeed / m_Ship.MaxLinearVelocity);

            m_Main.startSpeed = particlesCurrentSpeed;
            m_Main.startLifetime = m_LifeTimeCurve.Evaluate(particlesCurrentSpeed / m_ParticlesMaxSpeed) * m_MaxLifeTime;
            m_Emission.rateOverTime = m_EmissionCurve.Evaluate(particlesCurrentSpeed / m_ParticlesMaxSpeed) * m_MaxEmission;
        }
    }
}
