using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ������������ ������ �� �����. ��, ��� ����� ����� ���������.
/// </summary>
public class Destructible : Entity
{
    #region Properties
    public const int TEAM_ID_NEUTRAL = 0;

    /// <summary>
    /// ������ ���������� �����������.
    /// </summary>
    [SerializeField] protected bool m_Indestructible;
    public bool IsIndestructible => m_Indestructible;

    /// <summary>
    /// ��������� ���-�� ����������.
    /// </summary>
    [SerializeField] protected int m_MaxHitPoints;
    public int MaxHitPoints => m_MaxHitPoints;

    [SerializeField] private int m_TeamId;
    public int TeamId => m_TeamId;

    [SerializeField][Range(0f, 1f)] private float m_FriendlyFirePercentage;
    public float FriendlyFirePercentage => m_FriendlyFirePercentage;

    /// <summary>
    /// ������� ���������.
    /// </summary>
    protected int m_CurrentHitPoints;
    public int CurrentHitPoints => m_CurrentHitPoints;

    /// <summary>
    /// �������, ���������� ��� "������" destructible.
    /// </summary>
    public event UnityAction m_EventOnDeath;

    private static HashSet<Destructible> m_AllDestructibles;
    public static IReadOnlyCollection<Destructible> AllDestructibles => m_AllDestructibles;

    #endregion

    #region Unity Events
    protected virtual void Awake()
    {
        m_CurrentHitPoints = m_MaxHitPoints;
    }

    #endregion

    #region Public API
    /// <summary>
    /// ���������� ����� � �������.
    /// </summary>
    /// <param name="damage">����, ��������� �������.</param>
    /// <returns>���������� ��������� ��������.</returns>
    public virtual bool ApplyDamage(int damage)
    {
        return ApplyDamage(damage, TEAM_ID_NEUTRAL, 1);
    }

    /// <summary>
    /// ���������� ����� � ������� � ����������� "����� �� �����".
    /// </summary>
    /// <param name="damage">����, ��������� �������.</param>
    /// <param name="teamId">Id ������� �������, ���������� ����.</param>
    /// <param name="friendlyFirePercentage">������� "����� �� �����" �������, ���������� ����.</param>
    /// <returns>���������� ��������� ��������.</returns>
    public virtual bool ApplyDamage(int damage, int teamId, float friendlyFirePercentage)
    {
        if (IsIndestructible)
            return false;

        if (teamId == m_TeamId && friendlyFirePercentage == 0)
            return false;

        if (teamId == m_TeamId && friendlyFirePercentage > 0f)
            m_CurrentHitPoints -= (int)(damage * friendlyFirePercentage);
        else
            m_CurrentHitPoints -= damage;

        if (m_CurrentHitPoints <= 0)
        {
            m_CurrentHitPoints = 0;
            OnDeath();
        }

        return true;
    }

    /// <summary>
    /// ����������� �������� � �������. �������� ������� �� ����� ����� ������ �������������.
    /// </summary>
    /// <param name="healAmount">���-�� ������������� ��������.</param>
    public bool Heal(int healAmount)
    {
        if (m_CurrentHitPoints < m_MaxHitPoints)
        {
            if (m_CurrentHitPoints + healAmount > m_MaxHitPoints)
                m_CurrentHitPoints = m_MaxHitPoints;
            else
                m_CurrentHitPoints += healAmount;

            return true;
        }

        return false;
    }

    #endregion

    #region Protected API
    /// <summary>
    /// ���������������� ������� ����������� �������, ����� ��������� ���� ����.
    /// </summary>
    protected virtual void OnDeath()
    {
        Destroy(gameObject);

        m_EventOnDeath?.Invoke();
    }

    protected virtual void OnEnable()
    {
        if (m_AllDestructibles == null)
            m_AllDestructibles = new HashSet<Destructible>();

        m_AllDestructibles.Add(this);
    }

    protected virtual void OnDestroy()
    {
        m_AllDestructibles.Remove(this);
    }

    #endregion
}
