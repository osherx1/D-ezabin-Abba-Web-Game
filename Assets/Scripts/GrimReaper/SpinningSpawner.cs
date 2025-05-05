using System.Collections;
using UnityEngine;

public class SpinningSpawner : MonoBehaviour
{
    [SerializeField] private float startingSpawnTime = 3f;
    [SerializeField] private float endingSpawnTime = 8.75f;
    [SerializeField] private int spinningScytheAmount = 3;
    private float _spawnInterval;
    [SerializeField] private float spinningScytheSpeed = 3f;
    [SerializeField] private int spinningScytheHealth = 3;
    
    [SerializeField] private GameObject spinningScythePrefab;

    public void SetupSpawnerSettings(SpinningScytheSpawnerSettings spinningScytheSpawnerSettings, GameObject spinningScythePrefab)
    {
        startingSpawnTime = spinningScytheSpawnerSettings.startingSpawnTime;
        endingSpawnTime = spinningScytheSpawnerSettings.endingSpawnTime;
        spinningScytheAmount = spinningScytheSpawnerSettings.spinningScytheAmount;
        this.spinningScythePrefab = spinningScythePrefab;
    }
    
    private void CalculateSpawnInterval()
    {
        _spawnInterval = (endingSpawnTime - startingSpawnTime) / spinningScytheAmount;
    }

    public void StartSpawning()
    {
        CalculateSpawnInterval();
        StartCoroutine(SpawningRoutine());
    }

    private IEnumerator SpawningRoutine()
    {
        yield return new WaitForSeconds(startingSpawnTime);
        for (var i = 0; i < spinningScytheAmount; i++)
        {
            SpawnSpinningScythe();
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private void SpawnSpinningScythe()
    {
        var spawnPosition = (Vector2)transform.position;

        GameObject spinningScythe = Instantiate(spinningScythePrefab, spawnPosition, Quaternion.identity);
        spinningScythe.GetComponent<SpinningScytheAttack>().SetHealth(spinningScytheHealth);
        spinningScythe.GetComponent<SpinningScytheAttack>().SetSpeed(spinningScytheSpeed);
        spinningScythe.GetComponent<SpinningScytheAttack>().StartAttack();
    }
}
