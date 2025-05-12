using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class SwipingAttackSettings
{
    public float swipingAttackSpeed = 1f;
    public int swipingAttackHealth = 10;
    public Vector2 swipingAttackRightPosition;
    public Vector2 swipingAttackLeftPosition;
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
    public string[] movementClipNames;
    public string destructionClipName = "MinionHit";
    public float timeBetweenClips = 0.5f;
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
    public float knockbackDurationSeconds = 0.5f;
    public string movementClipName = "SpinningScytheMovement";
    public string hitClipName = "SpinningScytheHit";
    public string deathClipName = "SpinningScytheDeath";
}

[System.Serializable]
public class SpinningScytheSpawnerSettings
{
    public float startingSpawnTime = 3f;
    public float endingSpawnTime = 8.75f;
    public int spinningScytheAmount = 3;
}

[System.Serializable]
public class DifficultySettings
{
    public string name;
    public CreatureStage creatureStage;
    public float timeBetweenAttacks = 5f;
    public int minionCount = 5;
    public float minionSpeed = 1f;
    public int minionStartingHealth = 1;
    public int spinningScytheAmount = 3;
    public float spinningScytheSpeed = 1f;
    public int spinningScytheHealth = 10;
}