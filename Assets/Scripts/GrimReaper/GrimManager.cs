using System;
using System.Collections;
using Audio;
using GrimReaper;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

public class GrimManager : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private Vector2 boardMinBounds;
    [SerializeField] private Vector2 boardMaxBounds;
    
    [Header("Grim Reaper Settings")]
    [SerializeField] private float timeBetweenAttacks = 45f;
    [SerializeField] private float timeBeforeFirstAttack = 25f;
    [SerializeField] private float timeDiviation = 3f;
    [SerializeField] private float timeToStartAudioBeforeAttack = 4f;
    // private float _timeBetweenAttacks;
    
    [Header("Swiping Attack Settings")]
    [SerializeField] private GameObject swipingAttackPrefab;
    [SerializeField] private SwipingAttackSettings swipingAttackSettings;
    [SerializeField] private SwipingCounterAttackSettings swipingAttackCounterAttackSettings;

    [Header("Spinning Attack Settings")] 
    [SerializeField] private GameObject spinningAttackSpawnerPrefab;
    [SerializeField] private SpinningScytheSpawnerSettings spinningScytheSpawnerSettings;
    [SerializeField] private GameObject spinningAttackPrefab;
    [SerializeField] private SpinningScytheAttackSettings spinningScytheAttackSettings;
    [SerializeField] private string spinningScytheAttackAudioName;
    [SerializeField] private string spinningScytheSummoningAudioName;
    
    [Header("Minions Attack Settings")] 
    [SerializeField] private GameObject spawnerPrefab;
    [SerializeField] private MinionSpawnerSettings minionSpawnerSettings;
    [SerializeField] private MinionSettings minionSettings;
    [SerializeField] private MinionCounterAttackSettings minionCounterAttackSettings;
    [SerializeField] private string minionAttackAudioName;
    [SerializeField] private string minionSummoningAudioName;

    private CreatureStage _bestCreatureStage = CreatureStage.Goat;

    private void OnEnable()
    {
        // _timeBetweenAttacks = timeBetweenAttacks;
        // StartCoroutine(AttacksRoutine());
        GameEvents.OnCreatureMerged += HandleCreatureMerged;
    }
    
    private void OnDisable()
    {
        GameEvents.OnCreatureMerged -= HandleCreatureMerged;
    }

    private void HandleCreatureMerged(CreatureStage stage)
    {
        if (_bestCreatureStage == CreatureStage.Goat)
        {
            StartCoroutine(AttacksRoutine());
        }
        if(_bestCreatureStage < stage)
        {
            _bestCreatureStage = stage;
            IncreaseDifficulty();
            // Debug.Log("Best creature stage: " + _bestCreatureStage);
            // CallMinionsAttack();
        }
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
                    AudioManager.Instance.PlaySound(transform.position, spinningScytheSummoningAudioName);
                    yield return new WaitForSeconds(timeToStartAudioBeforeAttack);
                    StartCoroutine(StartSpinningAttack());
                    break;
                case 1:
                    AudioManager.Instance.PlaySound(transform.position, minionSummoningAudioName);
                    yield return new WaitForSeconds(timeToStartAudioBeforeAttack);
                    StartCoroutine(StartMinionAttack());
                    break;
            }
            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }
    
    private void IncreaseDifficulty()
    {
        // float randomTimeDeviation = Random.Range(-timeDiviation, timeDiviation);
        switch (_bestCreatureStage)
        {
            case CreatureStage.Goat:
                return;
            case CreatureStage.GoatCat:
                minionSpawnerSettings.minionCount++;
                break;
            case CreatureStage.Cat:
                minionSpawnerSettings.minionCount++;
                spinningScytheSpawnerSettings.spinningScytheAmount++;
                timeBetweenAttacks -= 5f;
                break;
            case CreatureStage.CatDog:
                minionSpawnerSettings.minionCount++;
                minionSettings.speed += 0.5f;
                spinningScytheAttackSettings.spinningScytheSpeed += 0.5f;
                break;
            case CreatureStage.Dog:
                timeBetweenAttacks -= 5f;
                minionSpawnerSettings.minionCount++;
                spinningScytheSpawnerSettings.spinningScytheAmount++;
                break;
            case CreatureStage.DogOx:
                timeBetweenAttacks -= 5f;
                minionSpawnerSettings.minionCount++;
                minionSettings.speed += 0.5f;
                spinningScytheAttackSettings.spinningScytheSpeed += 0.5f;
                spinningScytheAttackSettings.spinningScytheHealth++;
                break;
            case CreatureStage.Ox:
                timeBetweenAttacks -= 5f;
                minionSpawnerSettings.minionCount++;
                spinningScytheSpawnerSettings.spinningScytheAmount++;
                minionSettings.speed += 0.5f;
                spinningScytheAttackSettings.spinningScytheSpeed += 0.5f;
                spinningScytheAttackSettings.spinningScytheHealth++;
                break;
        }
        
    }

    private IEnumerator StartSpinningAttack()
    {
        yield return new WaitForEndOfFrame();
        var spawnPosition = new Vector2(Random.Range(boardMinBounds.x, boardMaxBounds.x), Random.Range(boardMinBounds.y, boardMaxBounds.y));
        var spinningScytheSpawner = Instantiate(spinningAttackSpawnerPrefab, spawnPosition, Quaternion.identity);
        var spinningScytheSpawnerComponent = spinningScytheSpawner.GetComponent<SpinningSpawner>();
        spinningScytheSpawnerComponent.SetupSpawnerSettings(spinningScytheSpawnerSettings, spinningAttackPrefab, spinningScytheAttackSettings);
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

