using System.Collections;
using GrimReaper;
using UnityEngine;
using Utilities;

public class GrimManager : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private Vector2 boardMinBounds;
    [SerializeField] private Vector2 boardMaxBounds;
    
    [Header("Grim Reaper Settings")]
    [SerializeField] private float timeBetweenAttacks = 15f;
    [SerializeField] private float timeBeforeFirstAttack = 10f;
    
    
    [Header("Swiping Attack Settings")]
    [SerializeField] private GameObject swipingAttackPrefab;
    [SerializeField] private SwipingAttackSettings swipingAttackSettings;
    [SerializeField] private SwipingCounterAttackSettings swipingAttackCounterAttackSettings;

    [Header("Spinning Attack Settings")] 
    [SerializeField] private GameObject spinningAttackSpawnerPrefab;
    [SerializeField] private SpinningScytheSpawnerSettings spinningScytheSpawnerSettings;
    [SerializeField] private GameObject spinningAttackPrefab;
    [SerializeField] private SpinningScytheAttackSettings spinningScytheAttackSettings;
    
    [Header("Minions Attack Settings")] 
    [SerializeField] private GameObject spawnerPrefab;
    [SerializeField] private MinionSpawnerSettings minionSpawnerSettings;
    [SerializeField] private MinionSettings minionSettings;
    [SerializeField] private MinionCounterAttackSettings minionCounterAttackSettings;
    

    private void OnEnable()
    {
        StartCoroutine(AttacksRoutine());
        // StartCoroutine(StartMinionAttack());
        StartCoroutine(SetupSwipingAttack()); 
        // StartCoroutine(StartSwipingAttackRoutine());
        // StartCoroutine(StartSpinningAttack());
    }

    private IEnumerator AttacksRoutine()
    {
        yield return new WaitForSeconds(timeBeforeFirstAttack);
        while (true)
        {
            int attackType = Random.Range(0, 2);
            switch (attackType)
            {
                case 0:
                    StartCoroutine(StartSpinningAttack());
                    break;
                case 1:
                    StartCoroutine(StartMinionAttack());
                    break;
            }
            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }

    private IEnumerator StartSpinningAttack()
    {
        yield return new WaitForEndOfFrame();
        var spawnPosition = new Vector2(Random.Range(boardMinBounds.x, boardMaxBounds.x), Random.Range(boardMinBounds.y, boardMaxBounds.y));
        var spinningScytheSpawner = Instantiate(spinningAttackSpawnerPrefab, spawnPosition, Quaternion.identity);
        var spinningScytheSpawnerComponent = spinningScytheSpawner.GetComponent<SpinningSpawner>();
        spinningScytheSpawnerComponent.SetupSpawnerSettings(spinningScytheSpawnerSettings, spinningAttackPrefab);
        spinningScytheSpawnerComponent.StartSpawning();
        // var spinningAttackGameObject = Instantiate(spinningAttackPrefab, spawnPosition, Quaternion.identity);
        // var spinningAttack = spinningAttackGameObject.GetComponent<SpinningScytheAttack>();
        // spinningAttack.SetHealth(spinningScytheAttackSettings.spinningScytheHealth);
        // spinningAttack.SetSpeed(spinningScytheAttackSettings.spinningScytheSpeed);
        // spinningAttack.StartAttack();
    }

    private IEnumerator StartSwipingAttackRoutine()
    {
        yield return new WaitForSeconds(1f);
        CallSwipingAttack();
    }

    private IEnumerator StartMinionAttack()
    {
        yield return new WaitForEndOfFrame();
        CallMinionsAttack();
    }

    private IEnumerator SetupSwipingAttack()
    {
        yield return new WaitForEndOfFrame();
        var swipingAttackGameObject = Instantiate(swipingAttackPrefab, swipingAttackSettings.swipingAttackRightPosition, Quaternion.identity);
        var swipingAttack = swipingAttackGameObject.GetComponent<SwipingAttack>();
        swipingAttack.SetSwipingAttackSpeed(swipingAttackSettings.swipingAttackSpeed);
        swipingAttack.SetSwipingAttackHealth(swipingAttackSettings.swipingAttackHealth);
        swipingAttack.SetSwipingAttackRightPosition(swipingAttackSettings.swipingAttackRightPosition);
        swipingAttack.SetSwipingAttackLeftPosition(swipingAttackSettings.swipingAttackLeftPosition);
        swipingAttack.SetKnockbackForce(swipingAttackCounterAttackSettings.knockbackForce);
        swipingAttack.SetKnockbackDurationSeconds(swipingAttackCounterAttackSettings.knockbackDurationSeconds);
        swipingAttack.SetDamageAgainstSwipingAttack(swipingAttackCounterAttackSettings.damageAgainstSwipingAttack);
    }
    
    private void CallSwipingAttack()
    {
        GameEvents.StartSwipingAttack?.Invoke();
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

