using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    [SerializeField][Min(0)] private float m_LifeTime;

    private float m_Timer;

    private void Update()
    {
        if (m_Timer < m_LifeTime)
            m_Timer += Time.deltaTime;
        else
            Destroy(gameObject);
    }
}