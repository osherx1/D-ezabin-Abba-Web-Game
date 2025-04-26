using Pool;
using UnityEngine;

public class PoolableMinion : MonoBehaviour, IPoolable
{
    private Transform _target;
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed = 200f;
    private Rigidbody2D _rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_target == null) return;
        Vector2 direction = ((Vector2)_target.position - _rb.position).normalized;
        float rotatAmount = Vector3.Cross(transform.up, direction).z;
        _rb.angularVelocity = rotatAmount * rotationSpeed;
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
}
