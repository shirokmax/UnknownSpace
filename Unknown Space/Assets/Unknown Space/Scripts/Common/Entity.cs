using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Базовый класс для всех интерактивных объектов на сцене.
/// </summary>
public abstract class Entity : MonoBehaviour
{
    [SerializeField] private string m_Nickname;
    public string Nickname => m_Nickname;

    [SerializeField] private EntityType m_Type;
    public EntityType Type => m_Type;

    public event UnityAction m_EventOnDestroy;

    private void OnDestroy()
    {
        m_EventOnDestroy?.Invoke();
    }
}