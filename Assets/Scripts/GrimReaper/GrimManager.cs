using System.Collections;
using Audio;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

public class GrimManager : MonoBehaviour
{
    [SerializeField] private GameObject cloudAttack;
    [SerializeField] private string cloudAttackAudioName = "CloudAttack";
    [Header("Board Settings")]
    [SerializeField] private Vector2 boardMinBounds;
    [SerializeField] private Vector2 boardMaxBounds;
    
    [Header("Grim Reaper Settings")]
    [SerializeField] private float timeBetweenAttacks = 45f;
    // [SerializeField] private float timeBeforeFirstAttack = 25f;
    // [SerializeField] private float timeDiviation = 3f;
    [SerializeField] private float timeToStartAudioBeforeAttack = 4f;
    // private float _timeBetweenAttacks;
    
    // [Header("Swiping Attack Settings")]
    // [SerializeField] private GameObject swipingAttackPrefab;
    // [SerializeField] private SwipingAttackSettings swipingAttackSettings;
    // [SerializeField] private SwipingCounterAttackSettings swipingAttackCounterAttackSettings;

    [Header("Spinning Attack Settings")] 
    [SerializeField] private GameObject spinningAttackSpawnerPrefab;
    [SerializeField] private SpinningScytheSpawnerSettings spinningScytheSpawnerSettings;
    [SerializeField] private GameObject spinningAttackPrefab;
    [SerializeField] private SpinningScytheAttackSettings spinningScytheAttackSettings;
    [SerializeField] private string spinningScytheSummoningAudioName;
    
    [Header("Minions Attack Settings")] 
    [SerializeField] private GameObject spawnerPrefab;
    [SerializeField] private MinionSpawnerSettings minionSpawnerSettings;
    [SerializeField] private MinionSettings minionSettings;
    [SerializeField] private MinionCounterAttackSettings minionCounterAttackSettings;
   [SerializeField] private string minionSummoningAudioName;
    
    [Header("Difficulty Settings")]
    [SerializeField] private DifficultySettings[] difficultySettings;
    private DifficultySettings _currentDifficultySettings;
    private float _timer;

    private CreatureStage _bestCreatureStage = CreatureStage.Goat;

    private void OnEnable()
    {
        // _timeBetweenAttacks = timeBetweenAttacks;
        // StartCoroutine(AttacksRoutine());
        GameEvents.OnCreatureMerged += HandleCreatureMerged;
        GameEvents.OnSwordActivated += HandleSwordActivated;
        GameEvents.OnOxButcherMerged += HandleShcoyechSound;
        _currentDifficultySettings = difficultySettings[0];
        
    }
    
    private void OnDisable()
    {
        GameEvents.OnCreatureMerged -= HandleCreatureMerged;
        GameEvents.OnSwordActivated -= HandleSwordActivated;
        GameEvents.OnOxButcherMerged -= HandleShcoyechSound;
    }

    private void HandleShcoyechSound()
    {
        GameEvents.DestroyAllEnemies?.Invoke();
        AudioManager.Instance.PauseUnpauseBackgroundMusic();
        AudioManager.Instance.PlaySound(transform.position, "Shcoyech");
        StartCoroutine(ResumeBackgroundMusic());
        _timer = 0;
    }

    private IEnumerator ResumeBackgroundMusic()
    {
        yield return new WaitForSeconds(10.5f);
        AudioManager.Instance.PauseUnpauseBackgroundMusic();
    }

    private void HandleSwordActivated()
    {
        AudioManager.Instance.PlaySound(transform.position, cloudAttackAudioName);
        Instantiate(cloudAttack);
    }

    private void LateUpdate()
    {
        _timer += Time.deltaTime;
        if (_timer < timeBetweenAttacks) return;
        StartCoroutine(DoRandomAttack());
        _timer = 0f;
    }

    private void HandleCreatureMerged(CreatureStage stage)
    {
        // if (_bestCreatureStage == CreatureStage.Goat)
        // {
        //     StartCoroutine(AttacksRoutine());
        // }
        if (_bestCreatureStage >= stage) return;
        _bestCreatureStage = stage;
        IncreaseDifficulty();
        // Debug.Log("Best creature stage: " + _bestCreatureStage);
        // CallMinionsAttack();
    }

    // private IEnumerator AttacksRoutine()
    // {
    //     yield return new WaitForSeconds(timeBeforeFirstAttack);
    //     while (true)
    //     {
    //         int attackType = Random.Range(0, 2);
    //         switch (attackType)
    //         {
    //             case 0:
    //                 AudioManager.Instance.PlaySound(transform.position, spinningScytheSummoningAudioName);
    //                 yield return new WaitForSeconds(timeToStartAudioBeforeAttack);
    //                 StartCoroutine(StartSpinningAttack());
    //                 break;
    //             case 1:
    //                 AudioManager.Instance.PlaySound(transform.position, minionSummoningAudioName);
    //                 yield return new WaitForSeconds(timeToStartAudioBeforeAttack);
    //                 StartCoroutine(StartMinionAttack());
    //                 break;
    //         }
    //         yield return new WaitForSeconds(timeBetweenAttacks);
    //     }
    // }
    
    private IEnumerator DoRandomAttack()
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
    }

    private void IncreaseDifficulty()
    {
        var newDifficultySettings = GetDifficultySettings(_bestCreatureStage);
        if (newDifficultySettings != _currentDifficultySettings)
        {
            SetDifficultySetting(newDifficultySettings);
            _currentDifficultySettings = newDifficultySettings;
        }
    }
    
    // private void IncreaseDifficulty()
    // {
    //     // float randomTimeDeviation = Random.Range(-timeDiviation, timeDiviation);
    //     switch (_bestCreatureStage)
    //     {
    //         case CreatureStage.Goat:
    //             return;
    //         case CreatureStage.GoatCat:
    //             minionSpawnerSettings.minionCount++;
    //             break;
    //         case CreatureStage.Cat:
    //             minionSpawnerSettings.minionCount++;
    //             spinningScytheSpawnerSettings.spinningScytheAmount++;
    //             timeBetweenAttacks -= 5f;
    //             break;
    //         case CreatureStage.CatDog:
    //             minionSpawnerSettings.minionCount++;
    //             minionSettings.speed += 0.5f;
    //             spinningScytheAttackSettings.spinningScytheSpeed += 0.5f;
    //             break;
    //         case CreatureStage.Dog:
    //             timeBetweenAttacks -= 5f;
    //             minionSpawnerSettings.minionCount++;
    //             spinningScytheSpawnerSettings.spinningScytheAmount++;
    //             break;
    //         case CreatureStage.DogOx:
    //             timeBetweenAttacks -= 5f;
    //             minionSpawnerSettings.minionCount++;
    //             minionSettings.speed += 0.5f;
    //             spinningScytheAttackSettings.spinningScytheSpeed += 0.5f;
    //             spinningScytheAttackSettings.spinningScytheHealth++;
    //             break;
    //         case CreatureStage.Ox:
    //             timeBetweenAttacks -= 5f;
    //             minionSpawnerSettings.minionCount++;
    //             spinningScytheSpawnerSettings.spinningScytheAmount++;
    //             minionSettings.speed += 0.5f;
    //             spinningScytheAttackSettings.spinningScytheSpeed += 0.5f;
    //             spinningScytheAttackSettings.spinningScytheHealth++;
    //             break;
    //     }
    //     
    // }

    // private void LateUpdate()
    // {
    //     
    // }

    private DifficultySettings GetDifficultySettings(CreatureStage stage)
    {
        foreach (var d in difficultySettings)
        {
            if (d.creatureStage == stage)
            {
                return d;
            }
        }

        return difficultySettings[0];
    }

    private void SetDifficultySetting(DifficultySettings newDifficultySettings)
    {
        minionSpawnerSettings.minionCount = newDifficultySettings.minionCount;
        minionSettings.speed = newDifficultySettings.minionSpeed;
        minionSettings.minionStartingHealth = newDifficultySettings.minionStartingHealth;
        spinningScytheAttackSettings.spinningScytheSpeed = newDifficultySettings.spinningScytheSpeed;
        spinningScytheAttackSettings.spinningScytheHealth = newDifficultySettings.spinningScytheHealth;
        spinningScytheSpawnerSettings.spinningScytheAmount = newDifficultySettings.spinningScytheAmount;
        timeBetweenAttacks = newDifficultySettings.timeBetweenAttacks;
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

    // private IEnumerator SetupSwipingAttack()
    // {
    //     yield return new WaitForEndOfFrame();
    //     var swipingAttackGameObject = Instantiate(swipingAttackPrefab, swipingAttackSettings.swipingAttackRightPosition, Quaternion.identity);
    //     var swipingAttack = swipingAttackGameObject.GetComponent<SwipingAttack>();
    //     swipingAttack.SetSwipingAttackSpeed(swipingAttackSettings.swipingAttackSpeed);
    //     swipingAttack.SetSwipingAttackHealth(swipingAttackSettings.swipingAttackHealth);
    //     swipingAttack.SetSwipingAttackRightPosition(swipingAttackSettings.swipingAttackRightPosition);
    //     swipingAttack.SetSwipingAttackLeftPosition(swipingAttackSettings.swipingAttackLeftPosition);
    //     swipingAttack.SetKnockbackForce(swipingAttackCounterAttackSettings.knockbackForce);
    //     swipingAttack.SetKnockbackDurationSeconds(swipingAttackCounterAttackSettings.knockbackDurationSeconds);
    //     swipingAttack.SetDamageAgainstSwipingAttack(swipingAttackCounterAttackSettings.damageAgainstSwipingAttack);
    // }
    
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

