using System.Collections;
using Pool;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities;

public class PoolableMinion : MonoBehaviour, IPoolable, IPointerClickHandler
{
    private Transform _target;

    [Header("Minion Settings")] 
    [SerializeField] private float speed;
    // [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private int minionStartingHealth = 5;
    private Rigidbody2D _rb;

    [Header("Counter attack parameters")] 
    [SerializeField] private float knockbackForce;
    [SerializeField] private float knockbackDurationSeconds = 0.5f;
    [SerializeField] private int damageAgainstMinion = 1;

    private bool _shouldMove = false;
    private bool _isDead;

    private int _minionHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _isDead = false;
    }

    private void OnEnable()
    {
        GameEvents.StartMinionsAttack += OnStartMinionsAttack;
        _minionHealth = minionStartingHealth;
        OnStartMinionsAttack();
    }

    private void OnDisable()
    {
        GameEvents.StartMinionsAttack -= OnStartMinionsAttack;
        // Reset();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!_shouldMove || _isDead) return;
        MoveMinion();
    }

    private void OnStartMinionsAttack()
    {
        FindCosestCreature();
        SetShouldMove(true);
        // MoveMinion();
    }

    private void MoveMinion()
    {
        // if (_target == null)
        // {
        //     FindCosestCreature();
        //     if (_target == null) return;
        // }
        FindCosestCreature();
        if (_target == null) return;
        var direction = ((Vector2)_target.position - _rb.position).normalized;
        _rb.linearVelocity = direction * speed;

        // if i want to rotate the minion towards the target and then move
        // var rotateAmount = Vector3.Cross(transform.up, direction).z;
        // _rb.angularVelocity = rotateAmount * rotationSpeed;
        // _rb.linearVelocity = transform.up * speed;
    }

    public void Reset()
    {
        _target = null;
    }

    private void SetTarget(Transform target)
    {
        if (target == null) return;
        _target = target;
    }
    
    private float Speed
    {
        get => speed;
        set => speed = value;
    }

    private float KnockbackForce
    {
        get => knockbackForce;
        set => knockbackForce = value;
    }

    private float KnockbackDurationSeconds
    {
        get => knockbackDurationSeconds;
        set => knockbackDurationSeconds = value;
    }

    private int DamageAgainstMinion
    {
        get => damageAgainstMinion;
        set => damageAgainstMinion = value;
    }

    private int MinionHealth
    {
        get => _minionHealth;
        set => _minionHealth = value;
    }
    

    // public void SetRotationSpeed(float rotationSpeed)
    // {
    //     this.rotationSpeed = rotationSpeed;
    // }

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
    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(HandleClick());
    }

    private IEnumerator HandleClick()
    {
        yield return null;
        TakeDamage(damageAgainstMinion);
    }

    private void TakeDamage(int damage)
    {
        _minionHealth -= damage;
        Debug.Log("Minion took damage, health remaining: " + _minionHealth);
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
        if (_target != null && _target.gameObject.activeInHierarchy)
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
        StopCoroutine(ResumeMovementAfterKnockback());
        yield return new WaitForEndOfFrame();
        StartCoroutine(ResumeMovementAfterKnockback());
    }

    private IEnumerator ResumeMovementAfterKnockback()
    {
        SetShouldMove(false);
        // wait for a short duration before resuming movement
        yield return new WaitForSeconds(knockbackDurationSeconds);
        SetShouldMove(true);
        MoveMinion();
    }

    private void HandleMinionDestruction()
    {
        MinionPool.Instance.Return(this);
    }
    public void SetMinionSettings(MinionSettings minionSettings, MinionCounterAttackSettings minionCounterAttackSettings)
    {
        Speed = minionSettings.speed;
        MinionHealth = minionSettings.minionStartingHealth;
        KnockbackForce = minionCounterAttackSettings.knockbackForce;
        KnockbackDurationSeconds = minionCounterAttackSettings.knockbackDurationSeconds;
        DamageAgainstMinion = minionCounterAttackSettings.damageAgainstMinion;
    }
}