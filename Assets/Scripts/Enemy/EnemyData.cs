using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;
    public EnemyAIType aiType;

    [Header("Combat Stats")]
    public int maxHealth = 10;
    public int attackDamage = 1;
    public float attackRange = 2f;
    public float preAttackDuration = 0.3f;
    public float attackDuration = 0.2f;
    public float postAttackDuration = 0.4f;

    [Header("Movement")]
    public bool usesNavMesh = true;
    public float moveSpeed = 3f;

    [Header("Weapon")]
    public GameObject weaponPrefab;

    [Header("Awareness Settings")]
    public float awarenessRadius = 6f;

    [Header("Idle Wander Settings")]
    public bool enableWanderInIdle = true;
    public float idleWanderRadius = 2f;
    public float idleWanderInterval = 2f;

    [Header("Group Awareness")]
    public float groupAwarenessRadius = 5f;
}

public enum EnemyAIType
{
    Aggressive,
    Ranged,
    CollisionRetaliate,
    Patrol,
    Boss
}

[System.Serializable]
public struct EnemyEliteAttribute
{
    public EliteAttributeType type;
    public float value;
}

public enum EliteAttributeType
{
    HeavyArmor,
    Frenzy,
    Blink,
    ExtraDamage,
    Vacuuming,
    Beams
}