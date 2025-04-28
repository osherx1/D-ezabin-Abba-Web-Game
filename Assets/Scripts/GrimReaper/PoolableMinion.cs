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
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private int minionStartingHealth = 5;
    private Rigidbody2D _rb;
    
    [Header("Target and Blocker layers")]
    [SerializeField]private LayerMask _creatureLayer;
    
    [Header("Counter attack parameters")]
    [SerializeField] private float knockbackForce;
    [SerializeField] private float knockbackDurationSeconds = 0.5f;
    [SerializeField] private int damageAgainstMinion = 1;

    private bool _shouldMove;
    private bool _isDead;

    private int _creatureLayerMaskValue;
    private int _minionHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _creatureLayerMaskValue = _creatureLayer.value;
        // _shouldMove = true;
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
    void FixedUpdate()
    {
        if (!_shouldMove || _isDead) return;
        if (_target == null)
        {
            FindCosestCreature(_creatureLayer);
        }
        MoveMinion();
    }

    private void OnStartMinionsAttack()
    {
        FindCosestCreature(_creatureLayer);
        SetShouldMove(true);
        // MoveMinion();
    }

    private void MoveMinion()
    {
        if (_target == null) return;
        Vector2 direction = ((Vector2)_target.position - _rb.position).normalized;
        float rotateAmount = Vector3.Cross(transform.up, direction).z;
        _rb.angularVelocity = rotateAmount * rotationSpeed;
        _rb.linearVelocity = transform.up * speed;
    }

    public void Reset()
    {
        _target = null;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
    }
    
    private void SetTarget(Transform target)
    {
        if(target == null) return;
        _target = target;
    }
    
    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void SetRotationSpeed(float rotationSpeed)
    {
        this.rotationSpeed = rotationSpeed;
    }

    private void FindCosestCreature(LayerMask creatureLayer)
    {
        // if (creatureLayer == _creatureLayer) return;
        var closestCreature = FindClosestObjectWithLayer(creatureLayer);
        SetTarget(closestCreature.transform);
    }
    
    private GameObject FindClosestObjectWithLayer(LayerMask layer)
    {
        CreatureCore closestObject = null;
        float closestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        var allObjects = FindObjectsByType<CreatureCore>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var obj in allObjects)

        {
            // if (((1 << obj.layer ) & layer.value) != 0)
            {
                float distance = Vector3.Distance(currentPosition, obj.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = obj;
                }
            }
        }

        return closestObject?.gameObject;
    }
    
    public void SetCreatureLayer(LayerMask layer)
    {
        _creatureLayer = layer;
    }
    
    public void SetShouldMove(bool shouldMove)
    {
        _shouldMove = shouldMove;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & _creatureLayerMaskValue ) != 0)
        {
            Debug.Log("hit creature");
            CreatureCore creature = other.gameObject.GetComponent<CreatureCore>();
            if (creature != null)
            {
                // apply damage to the creature
                creature.Death();
            }
        }
    }

    // How to handle the minions
    public void OnPointerClick(PointerEventData eventData)
    {
        TakeDamage(damageAgainstMinion);
    }

    private void TakeDamage(int damage)
    {
        _minionHealth -= damage;
        Debug.Log("Minion took damage: " + damage);
        // apply knockback to the minion
        KnokcbackMinion();
        if (_minionHealth <= 0)
        {
            // reset minion
            HandleMinionDestruction();
        }
    }

    private void KnokcbackMinion()
    {
        // apply knockback to the minion
        _rb.AddForce((-_target.position) * knockbackForce, ForceMode2D.Impulse);
        StopAllCoroutines();
        StartCoroutine(ResumeMovementAfterKnockback());
    }

    private IEnumerator ResumeMovementAfterKnockback()
    { 
        SetShouldMove(false);
        // _rb.linearVelocity = Vector2.zero;
        
        // wait for a short duration before resuming movement
        yield return new WaitForSeconds(knockbackDurationSeconds);
        
        // _isAttacking = true;
        SetShouldMove(true);
        MoveMinion();
    }

    private void HandleMinionDestruction()
    {
        MinionPool.Instance.Return(this);
    }
}
