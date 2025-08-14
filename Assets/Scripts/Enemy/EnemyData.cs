using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public EnemyAIType aiType;

    [Header("Attack Style")]
    public EnemyAttackStyle attackStyle = EnemyAttackStyle.NormalMelee; // NEW

    [Header("Stats")]
    public float maxHealth = 10f;
    public float moveSpeed = 3f;

    [Header("Awareness")]
    public float awarenessRadius = 6f; // NEW
    public float groupAwarenessRadius = 4f; // NEW

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float preAttackDuration = 0.3f;
    public float attackDuration = 0.5f;
    public float postAttackDuration = 0.5f;

    public GameObject weaponPrefab;
    public int attackDamage = 5;

    [Header("Visuals & Audio")]
    public Sprite icon;
    public RuntimeAnimatorController animatorController;
    public AudioClip aggroSound;
    public AudioClip deathSound;

    [Header("AI Options")]
    public EnemyEliteAttribute[] eliteAttributes;
    public bool usesNavMesh = false;
}

public enum EnemyAIType
{
    Aggressive,
    Ranged,
    CollisionRetaliate,
    Patrol,
    Boss
}
public enum EnemyAttackStyle
{
    NormalMelee,
    Lunge
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