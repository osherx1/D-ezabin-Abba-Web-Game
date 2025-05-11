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
    private SpinningScytheAttackSettings _spinningScytheAttackSettings;
    
    [SerializeField] private GameObject spinningScythePrefab;

    public void SetupSpawnerSettings(SpinningScytheSpawnerSettings spinningScytheSpawnerSettings, GameObject spinningScythePrefab, SpinningScytheAttackSettings spinningScytheAttackSettings)
    {
        startingSpawnTime = spinningScytheSpawnerSettings.startingSpawnTime;
        endingSpawnTime = spinningScytheSpawnerSettings.endingSpawnTime;
        spinningScytheAmount = spinningScytheSpawnerSettings.spinningScytheAmount;
        _spinningScytheAttackSettings = spinningScytheAttackSettings;
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
        yield return new WaitForSeconds(3f);
        HandleDestruction();
    }

    private void SpawnSpinningScythe()
    {
        var spawnPosition = (Vector2)transform.position;

        GameObject spinningScythe = Instantiate(spinningScythePrefab, spawnPosition, Quaternion.identity);
        var spinningScytheAttack = spinningScythe.GetComponent<SpinningScytheAttack>();
        if (spinningScytheAttack == null) return;
        spinningScytheAttack.SetHealth(_spinningScytheAttackSettings.spinningScytheHealth);
        spinningScytheAttack.SetSpeed(_spinningScytheAttackSettings.spinningScytheSpeed);
        spinningScytheAttack.SetKnockbackDuration(_spinningScytheAttackSettings.knockbackDurationSeconds);
        spinningScytheAttack.StartAttack();
    }
    
    private void HandleDestruction()
    {
        // Handle destruction logic here
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
