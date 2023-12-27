using UnityEngine;
using UnityEngine.UI;

namespace UnknownSpace
{
    public class UITargetSelection : MonoBehaviour, IDependency<SpaceShip>
    {
        [SerializeField] private Camera m_Camera;
        [SerializeField] private GameObject m_TargetFrame;
        [SerializeField] private Text m_TargetNameText;
        [SerializeField] private GameObject m_HitpointsBar;
        [SerializeField] private Image m_HitpointsBarFillImage;
        [SerializeField] private Text m_CurrentHitpointsText;
        [SerializeField] private Text m_MaxHitpointsText;

        private SpaceShip m_Ship;
        public void Construct(SpaceShip obj) => m_Ship = obj;

        private Entity m_CurrentTarget;
        public Entity CurrentTarget => m_CurrentTarget;

        private void Start()
        {
            m_HitpointsBar.SetActive(false);
            m_TargetFrame.SetActive(false);
        }

        private void Update()
        {
            SelectTarget();
            ResetTarget();
            UpdateTarget();
        }

        private void SelectTarget()
        {
            if (Input.GetMouseButtonUp(0))
            {
                Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit))
                {
                    var entity = hit.transform.GetComponentInParent<Entity>();

                    if (entity == null || entity == m_Ship) return;

                    m_CurrentTarget = entity;
                    m_TargetFrame.SetActive(value: true);

                    if (m_CurrentTarget is Destructible == false)
                        m_HitpointsBar.SetActive(false);
                    else
                        m_HitpointsBar.SetActive(true);

                    m_TargetNameText.text = entity.Nickname;
                }
            }
        }

        private void UpdateTarget()
        {
            if (m_CurrentTarget == null)
            {
                m_TargetFrame.SetActive(value: false);
                return;
            }

            Vector3 targetPos = m_Camera.WorldToScreenPoint(m_CurrentTarget.transform.position);
            targetPos.z = 0;

            m_TargetFrame.transform.position = targetPos;

            if (m_CurrentTarget is Destructible)
            {
                int currentHitpoints = (m_CurrentTarget as Destructible).CurrentHitPoints;
                int maxHitpoints = (m_CurrentTarget as Destructible).MaxHitPoints;

                m_CurrentHitpointsText.text = currentHitpoints.ToString();
                m_MaxHitpointsText.text = maxHitpoints.ToString();

                m_HitpointsBarFillImage.fillAmount = (float)currentHitpoints / maxHitpoints;
            }
        }

        private void ResetTarget()
        {
            if (Input.GetKeyDown(KeyCode.C))
                m_CurrentTarget = null;
        }    
    }
}