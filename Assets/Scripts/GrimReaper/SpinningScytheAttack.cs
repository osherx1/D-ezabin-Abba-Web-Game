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

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            wallsHitsBeforeDestroy--;
            if (wallsHitsBeforeDestroy <= 0)
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
}
