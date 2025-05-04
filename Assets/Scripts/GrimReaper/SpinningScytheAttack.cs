using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class SpinningScytheAttack : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private int health = 3;
    [SerializeField] private float speed = 3f;
    private Rigidbody2D _rb;

    private readonly Vector2[] _directions =
    {
        new Vector2(-1, 1).normalized, // Up-left
        new Vector2(1, 1).normalized, // Up-right
        new Vector2(-1, -1).normalized, // Down-left
        new Vector2(1, -1).normalized // Down-right
    };
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void StartAttack()
    {
        // Vector2 direction = Random.insideUnitCircle.normalized;
        StartCoroutine(StartMovement());
    }

    private IEnumerator StartMovement()
    {
        yield return new WaitForEndOfFrame();
        Vector2 direction = _directions[Random.Range(0, _directions.Length)];
        _rb.linearVelocity = direction * speed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            health--;
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }
        else if (other.gameObject.CompareTag("Creatures"))
        {
            // Handle damage to the creature
            var creature = other.gameObject.GetComponent<CreatureCore>();
            if (creature != null)
            {
                creature.Death();
            }
            // Destroy(gameObject);
        }
    }

    private void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            HandleDestruction();
        }
    }

    private void HandleDestruction()
    {
        Destroy(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Handle click on the scythe
        // Call the method to destroy the scythe
        //
        TakeDamage(1);
    }
}
