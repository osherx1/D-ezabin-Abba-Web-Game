using System.Collections;
using GrimReaper;
using UnityEngine;
using Utilities;

public class GrimManager : MonoBehaviour
{
    //TODO: attacks timing? 
    //todo: implement the Grim Reaper's attacks (Scythe, minions, etc...)
    //need : a way to find the most earning creature
    // todo: defense against the Grim Reaper's attacks, power ups?
    [Header("Scythe Settings")]
    [SerializeField] private ScytheAttack scythe;
    [SerializeField] private ScytheAttackSettings scytheAttackSettings;
    [SerializeField] private ScytheCounterAttackSettings scytheCounterAttackSettings;

    [Header("Minions Settings")] 
    [SerializeField] private GameObject spawnerPrefab;
    [SerializeField] private Vector2 boardMinBounds;
    [SerializeField] private Vector2 boardMaxBounds;
    [SerializeField] private MinionSpawnerSettings minionSpawnerSettings;
    [SerializeField] private MinionSettings minionSettings;
    [SerializeField] private MinionCounterAttackSettings minionCounterAttackSettings;
    
    /// Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // SetupScytheAttack();
    }

    private void OnEnable()
    {
        StartCoroutine(StartMinionAttack());
        StartCoroutine(SetupScytheAttack());
    }

    private IEnumerator StartMinionAttack()
    {
        yield return new WaitForSeconds(3f);
        CallMinionsAttack();
    }

    private IEnumerator SetupScytheAttack()
    {
        yield return new WaitForEndOfFrame();
        scythe.SetScytheSpeed(scytheAttackSettings.scytheSpeed);
        scythe.SetScytheHealth(scytheAttackSettings.scytheHealth);
        scythe.SetScytheRightPosition(scytheAttackSettings.scytheRightPosition);
        scythe.SetScytheLeftPosition(scytheAttackSettings.scytheLeftPosition);
        scythe.SetKnockbackForce(scytheCounterAttackSettings.knockbackForce);
        scythe.SetKnockbackDurationSeconds(scytheCounterAttackSettings.knockbackDurationSeconds);
        scythe.SetDamageAgainstScythe(scytheCounterAttackSettings.damageAgainstScythe);
    }
    
    private void CallScytheAttack()
    {
        GameEvents.StartScytheAttack?.Invoke();
    }

    private void CallMinionsAttack()
    {
        CreateMinionSpawner();
    }

    private void CreateMinionSpawner()
    {
        var spawnPosition = new Vector2(Random.Range(boardMinBounds.x, boardMaxBounds.x), Random.Range(boardMinBounds.y, boardMaxBounds.y));
        // var spawnPosition = boardMinBounds;
        var spawner = Instantiate(spawnerPrefab, spawnPosition, Quaternion.identity);
        var minionSpawner = spawner.GetComponent<MinionSpawner>();
        minionSpawner.SetSpawnerSettings(minionSpawnerSettings);
        minionSpawner.SetMinionSettings(minionSettings);
        minionSpawner.SetMinionCounterAttackSettings(minionCounterAttackSettings);
        minionSpawner.StartSpawning();
    }
}

// [System.Serializable]
// public class ScytheAttackSettings
// {
//     public float scytheSpeed = 1f;
//     public int scytheHealth = 10;
//     public Vector2 scytheRightPosition;
//     public Vector2 scytheLeftPosition;
// }
//
// [System.Serializable]
// public class ScytheCounterAttackSettings
// {
//     public float knockbackForce = 1f;
//     public float knockbackDurationSeconds = 0.5f;
//     public int damageAgainstScythe = 1;
// }

