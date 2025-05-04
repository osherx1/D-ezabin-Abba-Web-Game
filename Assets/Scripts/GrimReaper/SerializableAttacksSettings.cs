using UnityEngine;

[System.Serializable]
public class SwipingAttackSettings
{
    public float SwipingAttackSpeed = 1f;
    public int SwipingAttackHealth = 10;
    public Vector2 SwipingAttackRightPosition;
    public Vector2 SwipingAttackLeftPosition;
}

[System.Serializable]
public class SwipingCounterAttackSettings
{
    public float knockbackForce = 1f;
    public float knockbackDurationSeconds = 0.5f;
    public int damageAgainstSwipingAttack = 1;
}

[System.Serializable]
public class MinionSettings
{
    public float speed;
    public int minionStartingHealth = 5;
}

[System.Serializable]
public class MinionCounterAttackSettings
{
    public float knockbackForce;
    public float knockbackDurationSeconds = 0.5f;
    public int damageAgainstMinion = 1;
}

[System.Serializable]
public class MinionSpawnerSettings
{
    public int minionCount = 5;
    public float spawnRadius = 5f;
    public float spawnDelay = 2f;
}

[System.Serializable]
public class SpinningScytheAttackSettings
{
    public float spinningScytheSpeed = 1f;
    public int spinningScytheHealth = 10;
}