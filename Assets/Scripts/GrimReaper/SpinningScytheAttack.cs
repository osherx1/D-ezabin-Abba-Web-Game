using System.Collections;
using Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class SpinningScytheAttack : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private int health = 3;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float knockbackDurationSeconds = 0.5f;
    public string movementClipName = "SpinningScytheMovement";
    public string hitClipName = "SpinningScytheHit";
    public string deathClipName = "SpinningScytheDeath";
    private Rigidbody2D _rb;
    private Animator _animator;
    private Vector2 _originalVelocity;
    private bool _isKnockbackActive;

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
        _animator = GetComponent<Animator>();
        StartAttack();
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
        AudioManager.Instance.PlaySound(transform.position, movementClipName, 1f, 1f, true);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            TakeDamage(1);
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
        AudioManager.Instance.PlaySound(transform.position, hitClipName);
        if (health <= 0)
        {
            HandleDestruction();
        }
    }

    private void HandleDestruction()
    {
        StopAllCoroutines();
        AudioManager.Instance.StopSound(movementClipName);
        var position = transform.position;
        AudioManager.Instance.PlaySound(position, deathClipName);
        Destroy(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Handle click on the scythe
        // Call the method to destroy the scythe
        KnockbackScythe();
        TakeDamage(1);
        
    }


    private void KnockbackScythe()
    {
        if (_isKnockbackActive) return;

        // Save the current velocity
        _originalVelocity = _rb.linearVelocity;

        // Get the pointer position in world space
        Vector2 pointerPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Calculate the knockback direction (from pointer to the object)
        Vector2 knockbackDirection = ((Vector2)transform.position - pointerPosition).normalized;

        // Apply a knockback force
        float knockbackForce = 5f; // Adjust the force as needed
        _rb.linearVelocity = knockbackDirection * knockbackForce;

        // Set the knockback flag and start coroutine to restore velocity
        _isKnockbackActive = true;
        _animator.SetBool("Hit", true);
        AudioManager.Instance.StopSound(movementClipName);
        StartCoroutine(RestoreOriginalDirection());
    }

    private IEnumerator RestoreOriginalDirection()
    {
        // Wait for a short duration
        // float knockbackDuration = 0.5f; // Adjust the duration as needed
        
        yield return new WaitForSeconds(knockbackDurationSeconds);

        // Restore the original velocity and reset the flag
        _rb.linearVelocity = _originalVelocity;
        _isKnockbackActive = false;
        _animator.SetBool("Hit", false);
        AudioManager.Instance.PlaySound(transform.position, movementClipName, 1f, 1f, true);
    }

    public void SetHealth(int newHealth)
    {
        health = newHealth;
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    public void SetKnockbackDuration(float newKnockbackDuration)
    {
        knockbackDurationSeconds = newKnockbackDuration;
    }

    public void SetSpinningScytheAttackSettings(SpinningScytheAttackSettings newSettings)
    {
        SetHealth(newSettings.spinningScytheHealth);
        SetSpeed(newSettings.spinningScytheSpeed);
        SetKnockbackDuration(newSettings.knockbackDurationSeconds);
        movementClipName = newSettings.movementClipName;
        hitClipName = newSettings.hitClipName;
        deathClipName = newSettings.deathClipName;
    }
}
