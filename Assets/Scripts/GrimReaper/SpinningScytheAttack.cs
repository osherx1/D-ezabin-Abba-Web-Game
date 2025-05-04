using System.Collections;
using UnityEngine;

public class SpinningScytheAttack : MonoBehaviour
{
    [SerializeField] private int wallsHitsBeforeDestroy = 3;
    [SerializeField] private float speed = 3f;
    private Rigidbody2D _rb;
    
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
        Vector2 direction = Random.insideUnitCircle.normalized;
        _rb.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            wallsHitsBeforeDestroy--;
            if (wallsHitsBeforeDestroy <= 0)
            {
                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("Creatures"))
        {
            // Handle damage to the creature
            var creature = other.GetComponent<CreatureCore>();
            if (creature != null)
            {
                creature.Death();
            }
            // Destroy(gameObject);
        }
    }
}
