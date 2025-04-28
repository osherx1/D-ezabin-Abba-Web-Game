using Pool;
using UnityEngine;

public class PoolableMinion : MonoBehaviour, IPoolable
{
    private Transform _target;
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed = 200f;
    private Rigidbody2D _rb;
    [SerializeField]private LayerMask _creatureLayer;

    private bool _shouldMove;
    private bool _isDead;

    private int _creatureLayerMaskValue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _creatureLayerMaskValue = _creatureLayer.value;
        _shouldMove = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!_shouldMove || _isDead) return;
        if (_target == null)
        {
            FindCosestCreature(_creatureLayer);
        }
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

    public void FindCosestCreature(LayerMask creatureLayer)
    {
        // if (creatureLayer == _creatureLayer) return;
        GameObject closestCreature = FindClosestObjectWithLayer(creatureLayer);
        SetTarget(closestCreature.transform);
    }
    
    private GameObject FindClosestObjectWithLayer(LayerMask layer)
    {
        GameObject closestObject = null;
        float closestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (((1 << obj.layer ) & layer.value) != 0)
            {
                float distance = Vector3.Distance(currentPosition, obj.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = obj;
                }
            }
        }

        return closestObject;
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
}
