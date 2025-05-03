using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

public class MinionSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] 
    private MinionSpawnerSettings spawnerSettings;
    
    [Header("Minion Settings")]
    [SerializeField] 
    private MinionSettings minionSettings;
    [SerializeField] 
    private MinionCounterAttackSettings minionCounterAttackSettings;
    
    public void SetSpawnerSettings(MinionSpawnerSettings newSettings)
    {
        spawnerSettings = newSettings;
    }
    public void SetMinionSettings(MinionSettings newSettings)
    {
        minionSettings = newSettings;
    }
    
    public void SetMinionCounterAttackSettings(MinionCounterAttackSettings newSettings)
    {
        minionCounterAttackSettings = newSettings;
    }
    public void StartSpawning()
    {
        StartCoroutine(SpawnMinions());
    }

    private IEnumerator SpawnMinions()
    {
        yield return new WaitForSeconds(spawnerSettings.spawnDelay);
        for (int i = 0; i < spawnerSettings.minionCount; i++)
        {
            Debug.Log("Spawning minion " + i);
            var spawnPosition = Random.insideUnitCircle * spawnerSettings.spawnRadius;
            spawnPosition += (Vector2)transform.position;

            var minion = MinionPool.Instance.Get();
            if (minion == null)
            {
                Debug.LogError("No minion available in the pool");
                break;
            }
            minion.transform.position = spawnPosition;
            minion.SetMinionSettings(minionSettings, minionCounterAttackSettings);
            minion.gameObject.SetActive(true);
        }
        GameEvents.StartMinionsAttack?.Invoke();
        // if there is a destrucion animation, then wait for it to finish
        // yield return new WaitForSeconds(destructionAnimationDuration);
        // else just destroy the spawner
        Destroy(gameObject);
    }
}
