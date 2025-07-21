using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public EnemyAIType aiType;
    public float maxHealth;
    public float moveSpeed;

    public float attackRange;
    public float preAttackDuration;
    public float attackDuration;
    public float postAttackDuration;

    public GameObject weaponPrefab;
    public int attackDamage;

    public Sprite icon;
    public RuntimeAnimatorController animatorController;
    public AudioClip aggroSound;
    public AudioClip deathSound;

    public EnemyEliteAttribute[] eliteAttributes;
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