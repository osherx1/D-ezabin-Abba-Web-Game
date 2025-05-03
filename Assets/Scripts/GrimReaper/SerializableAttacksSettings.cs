using UnityEngine;

[System.Serializable]
public class ScytheAttackSettings
{
    public float scytheSpeed = 1f;
    public int scytheHealth = 10;
    public Vector2 scytheRightPosition;
    public Vector2 scytheLeftPosition;
}

[System.Serializable]
public class ScytheCounterAttackSettings
{
    public float knockbackForce = 1f;
    public float knockbackDurationSeconds = 0.5f;
    public int damageAgainstScythe = 1;
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