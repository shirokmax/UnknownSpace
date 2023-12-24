using UnityEngine;

namespace UnknownSpace
{
    public class SceneDependenciesContainer : Dependency
    {
        [SerializeField] private SpaceShip m_Ship;

        protected override void BindAll(MonoBehaviour monoBehaviourInScene)
        {
            Bind<SpaceShip>(m_Ship, monoBehaviourInScene);
        }

        private void Awake()
        {
            FindAllObjectsToBind();
        }
    }
}
