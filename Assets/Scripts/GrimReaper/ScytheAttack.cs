using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GrimReaper
{
    public class ScytheAttack : MonoBehaviour, IPointerClickHandler
    {
        [Header("Target and Blocker layers")] 
        [SerializeField] private LayerMask creatureLayerMask;
        [SerializeField] private LayerMask blockerLayerMask;

        [Header("Attack parameters")] 
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private int scytheHealth = 10;
        [SerializeField] private Vector2 scytheStartPosition;
        [SerializeField] private Vector2 scytheEndPosition;
        private int _currentScytheHealth;

        [Header("Counter attack parameters")] 
        [SerializeField] private float knockbackForce = 1f;
        [SerializeField] private float knockbackDurationSeconds = 0.5f;
        [SerializeField] private int damageAgainstScythe = 1;

        private Rigidbody2D _rb;
        private bool _isAttacking = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _rb = GetComponent<Rigidbody2D>(); 
            ResetScythe();
            _isAttacking = true; // start the attack
            MoveScythe();
            // set the scythe to its start position
            
        }
        // Update is called once per frame
        void Update()
        {
            // if (_isAttacking)
            // {
            //     MoveScythe();
            // }
        }

        private void StartScytheAttack()
        {
            // reset scythe position + health
            // make scythe visible
            // start movement
            // when clicked on, the scythe should take damage and get knocked back
            // if the scythe hit a creature, it should kill the creature
        }

        private void MoveScythe()
        {
            Vector2 direction = (scytheEndPosition - scytheStartPosition).normalized;
            _rb.linearVelocity = direction * attackSpeed;
            
            // check if the scythe has reached the end position
            if (Vector2.Distance(transform.position, scytheEndPosition) < 0.1f)
            {
                _isAttacking = false;
                // _rb.linearVelocity = Vector2.zero;
                ResetScythe();
            }
        }

        private void ResetScythe()
        {
            // reset the scythe to its original position
            // reset the scythe health
            _currentScytheHealth = scytheHealth;
            transform.position = scytheStartPosition;
            _rb.linearVelocity = Vector2.zero;
        }

        private void TakeDamage(int damage)
        {
            _currentScytheHealth -= damage;
            KnockbackScythe(Vector2.right);
            Debug.Log("Scythe took damage: " + damage);
            if (_currentScytheHealth <= 0)
            {
                // reset scythe
                HandleScytheDestruction();
            }
        }
        
        private void KnockbackScythe(Vector2 direction)
        {
            // apply knockback to the scythe
            _rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
            StartCoroutine(ResumeMovementAfterKnockback());
        }

        private IEnumerator ResumeMovementAfterKnockback()
        {
            _isAttacking = false;
            _rb.linearVelocity = Vector2.zero;
            
            // wait for a short duration before resuming movement
            yield return new WaitForSeconds(knockbackDurationSeconds);
            
            _isAttacking = true;
            MoveScythe();
        }

        private void HandleScytheDestruction()
        {
            
            _isAttacking = false;
            _rb.linearVelocity = Vector2.zero;
            Debug.Log("Scythe destroyed");
            //play destruction animation
            ResetScythe();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TakeDamage(damageAgainstScythe);
            // Vector2 knockbackDirection = (Vector2)transform.position - (Vector2)eventData.pointerPressRaycast.worldPosition;
            // _rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == creatureLayerMask)
            {
                Debug.Log("hit creature");
            }
        }
    }
}