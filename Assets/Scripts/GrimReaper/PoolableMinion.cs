using System;
using System.Collections;
using Audio;
using Pool;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities;
using Random = UnityEngine.Random;

public class PoolableMinion : MonoBehaviour, IPoolable, IPointerDownHandler
{
    private Transform _target;

    [Header("Minion Settings")] 
    [SerializeField] private float speed;
    // [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private int minionStartingHealth = 5;
    [SerializeField] private string[] movementClipNames;
    [SerializeField] private string destructionClipName = "MinionHit";
    [SerializeField] private float timeBetweenClips = 1.3f;
    private readonly float _clipDiviation = 2.5f;
    private Rigidbody2D _rb;
    private Animator _animator;

    [Header("Counter attack parameters")] 
    [SerializeField] private float knockbackForce;
    [SerializeField] private float knockbackDurationSeconds = 0.5f;
    [SerializeField] private int damageAgainstMinion = 1;

    private bool _shouldMove = false;
    private bool _isInvincible = false;
    private bool _isDead;

    private int _minionHealth;
    private bool _shouldFlip;
    private bool _isFrozen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _isDead = false;
    }

    private void OnEnable()
    {
        GameEvents.StartMinionsAttack += OnStartMinionsAttack;
        GameEvents.StopAllEnemies += OnStopAllEnemies;
        GameEvents.DestroyAllEnemies += OnDestroyAllEnemies;
        _minionHealth = minionStartingHealth;
        // OnStartMinionsAttack();
    }

    private void OnDisable()
    {
        GameEvents.StartMinionsAttack -= OnStartMinionsAttack;
        GameEvents.StopAllEnemies -= OnStopAllEnemies;
        GameEvents.DestroyAllEnemies -= OnDestroyAllEnemies;
        // Reset();
    }

    private void OnStopAllEnemies()
    {
        _isFrozen = true;
        StopAllCoroutines();
        _rb.linearVelocity = Vector2.zero;
        _animator.speed = 0;
        // StopSounds();
    }
    
    private void OnDestroyAllEnemies()
    {
        OnStopAllEnemies();
        HandleMinionDestruction();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!_shouldMove || _isDead || _isFrozen) return;
        MoveMinion();
    }

    private void OnStartMinionsAttack()
    {
        FindCosestCreature();
        SetShouldMove(true);
        // MoveMinion();
        StartCoroutine(MovementSoundRoutine());
    }

    private IEnumerator MovementSoundRoutine()
    {
        int numberOfClips = movementClipNames.Length;
        // Random random = new Random();
        while (!_isDead)
        {
            int clipIndex = Random.Range(0, numberOfClips);
            AudioManager.Instance.PlaySound(transform.position, movementClipNames[clipIndex], Random.Range(0.5f, 0.7f), Random.Range(0.8f, 1.2f));
            yield return new WaitForSeconds(timeBetweenClips + Random.Range(1f, _clipDiviation));
        }
    }

    private void MoveMinion()
    {
        // if (_target == null)
        // {
        //     FindCosestCreature();
        //     if (_target == null) return;
        // }
        FindCosestCreature();
        if (_target == null)
        {
            SetIsInvincible(true);
            
            _rb.linearVelocity = Vector2.zero;
            return;
        }
        SetIsInvincible(false);
        var direction = ((Vector2)_target.position - _rb.position).normalized;
        _rb.linearVelocity = direction * speed;
        _shouldFlip = direction.x < 0;
        HandleFlipping();
        // if i want to rotate the minion towards the target and then move
        // var rotateAmount = Vector3.Cross(transform.up, direction).z;
        // _rb.angularVelocity = rotateAmount * rotationSpeed;
        // _rb.linearVelocity = transform.up * speed;
    }

    private void HandleFlipping()
    {
        if (_shouldFlip)
        {
            var rotation = transform.rotation;
            rotation.y = 180f;
            transform.rotation = rotation;
        }
        else
        {
            var rotation = transform.rotation;
            rotation.y = 0f;
            transform.rotation = rotation;
        }
    }

    public void Reset()
    {
        _target = null;
        _shouldMove = false;
        _isInvincible = false;
        _isDead = false;
        _isFrozen = false;
        _minionHealth = minionStartingHealth;
    }

    private void SetTarget(Transform target)
    {
        if (target == null) return;
        _target = target;
    }
    
    private void FindCosestCreature()
    {
        CreatureCore closestCreature = null;
        var closestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        var allObjects = FindObjectsByType<CreatureCore>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            var distance = Vector3.Distance(currentPosition, obj.transform.position);
            if (!(distance < closestDistance)) continue;
            closestDistance = distance;
            closestCreature = obj;
        }

        if (closestCreature != null) SetTarget(closestCreature.gameObject.transform);
    }
    

    private void SetShouldMove(bool shouldMove)
    {
        _shouldMove = shouldMove;
    }
    
    private void SetIsInvincible(bool isInvincible)
    {
        _isInvincible = isInvincible;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CreatureCore creature = other.gameObject.GetComponent<CreatureCore>();
        if (creature != null)
        {
            // apply damage to the creature
            creature.Death();
        }
    }

    // How to handle the minions
    public void OnPointerDown(PointerEventData eventData)
    {
        HandleClick();
    }

    private void HandleClick()
    {
        // yield return null;
        TakeDamage(damageAgainstMinion);
    }
    

    private void TakeDamage(int damage)
    {
        // if(_isInvincible) return;
        _minionHealth -= damage;
        // Debug.Log("Minion took damage, health remaining: " + _minionHealth);
        // apply knockback to the minion
        KnockbackMinion();
        if (_minionHealth <= 0)
        {
            // call for minion destruction
            HandleMinionDestruction();
        }
    }

    private void KnockbackMinion()
    {
        if (_target != null)
        {
            Vector2 knockbackDirection = ((Vector2)transform.position - (Vector2)_target.position).normalized;
            // _rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            StartCoroutine(ApplyKnockback(knockbackDirection));
        }

        // StopAllCoroutines();
        // StartCoroutine(ResumeMovementAfterKnockback());
    }
    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        yield return new WaitForEndOfFrame(); // Wait for the next frame
        _rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        // StopCoroutine(ResumeMovementAfterKnockback());
        // yield return new WaitForEndOfFrame();
        StartCoroutine(ResumeMovementAfterKnockback());
    }

    private IEnumerator ResumeMovementAfterKnockback()
    {
        SetShouldMove(false);
        // wait for a short duration before resuming movement
        yield return new WaitForSeconds(knockbackDurationSeconds);
        SetShouldMove(true);
    }

    private void HandleMinionDestruction()
    {
        StopAllCoroutines();
        // if(_audioSource != null)
        //     _audioSource.StopAudio();
        // _audioSource.StopAudio();
        // AudioManager.Instance.StopAllSoundWithName(movementClipName);
        AudioManager.Instance.PlaySound(transform.position, destructionClipName);
        MinionPool.Instance.Return(this);
    }
    public void SetMinionSettings(MinionSettings minionSettings, MinionCounterAttackSettings minionCounterAttackSettings)
    {
        speed = minionSettings.speed;
        _minionHealth = minionSettings.minionStartingHealth;
        knockbackForce = minionCounterAttackSettings.knockbackForce;
        knockbackDurationSeconds = minionCounterAttackSettings.knockbackDurationSeconds;
        damageAgainstMinion = minionCounterAttackSettings.damageAgainstMinion;
        movementClipNames = minionSettings.movementClipNames;
        destructionClipName = minionSettings.distructionClipName;
        timeBetweenClips = minionSettings.timeBetweenClips;
    }
}