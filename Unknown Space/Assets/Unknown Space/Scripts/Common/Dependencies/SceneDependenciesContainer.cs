using UnityEngine;

namespace UnknownSpace
{
    public class SceneDependenciesContainer : Dependency
    {
        [SerializeField] private SpaceShip m_Ship;
        [SerializeField] private UITargetSelection m_TargetSelection;

        protected override void BindAll(MonoBehaviour monoBehaviourInScene)
        {
            Bind<SpaceShip>(m_Ship, monoBehaviourInScene);
            Bind<UITargetSelection>(m_TargetSelection, monoBehaviourInScene);
        }

        private void Awake()
        {
            FindAllObjectsToBind();
        }
    }
}
