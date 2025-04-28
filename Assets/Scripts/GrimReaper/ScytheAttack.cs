using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities;
using Random = UnityEngine.Random;

namespace GrimReaper
{
    public class ScytheAttack : MonoBehaviour, IPointerClickHandler
    {
        [Header("Target and Blocker layers")] 
        [SerializeField] private LayerMask creatureLayerMask;
        private int _creatureLayerMaskValue;
        [SerializeField] private LayerMask blockerLayerMask;
        private int _blockerLayerMaskValue;

        [Header("Attack parameters")] 
        [SerializeField] private float scytheSpeed = 1f;
        [SerializeField] private int scytheHealth = 10;
        [SerializeField] private Vector2 scytheRightPosition;
        [SerializeField] private Vector2 scytheLeftPosition;
        private int _currentScytheHealth;
        private Vector2 _scytheStartPosition;
        private Vector2 _scytheEndPosition;
        private Vector2 _direction;

        [Header("Counter attack parameters")] 
        [SerializeField] private float knockbackForce = 1f;
        [SerializeField] private float knockbackDurationSeconds = 0.5f;
        [SerializeField] private int damageAgainstScythe = 1;

        private Rigidbody2D _rb;
        private bool _isAttacking;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>(); 
            ResetScythe(); // set the scythe to its start position
            OnStartScytheAttack(); // for now, starts the scythe attack immediately
            _creatureLayerMaskValue = creatureLayerMask.value;
            _blockerLayerMaskValue = blockerLayerMask.value;
            _scytheStartPosition = scytheRightPosition;
            _scytheEndPosition = scytheLeftPosition;
        }

        private void OnEnable()
        {
            GameEvents.StartScytheAttack += OnStartScytheAttack;
        }
        
        private void OnDisable()
        {
            GameEvents.StartScytheAttack -= OnStartScytheAttack;
        }

        private void OnStartScytheAttack()
        {
            if(_isAttacking) return;
            // reset scythe position and health
            SetScytheDirectionRandomly(); // needs to be set outside from the grim manager
            SetScytheStartPosition();
            ResetScythe();
            // make scythe visible?
            _isAttacking = true;
            // start movement
            MoveScythe();
        }

        private void MoveScythe()
        { 
            _rb.linearVelocity = _direction * scytheSpeed;
            
            // check if the scythe has reached the end position
            if (Vector2.Distance(transform.position, _scytheEndPosition) < 0.1f)
            {
                // _rb.linearVelocity = Vector2.zero;
                ResetScythe();
            }
        }

        private void SetScytheDirectionRandomly()
        {
            int dir = Random.Range(0, 2);
            _direction = dir == 0 ? (scytheRightPosition - scytheLeftPosition).normalized : (scytheLeftPosition - scytheRightPosition).normalized;
        }
        
        public void SetScytheDirection(Vector2 direction)
        {
            // set the scythe direction to the given direction
            _direction = direction.normalized;
        }
        
        private void SetScytheStartPosition()
        {
            // set the scythe start position to the current position
            if(_direction.x > 0)
            {
                _scytheStartPosition = scytheLeftPosition;
                _scytheEndPosition = scytheRightPosition;
            }
            else
            {
                _scytheStartPosition = scytheRightPosition;
                _scytheEndPosition = scytheLeftPosition;
            }
        }

        private void ResetScythe()
        {
            // reset the scythe to its start position
            // reset the scythe health
            _isAttacking = false;
            _currentScytheHealth = scytheHealth;
            transform.position = _scytheStartPosition;
            _rb.linearVelocity = Vector2.zero;
            StopAllCoroutines();
        }

        public void TakeDamage(int damage)
        {
            _currentScytheHealth -= damage;
            KnockbackScythe();
            Debug.Log("Scythe took damage: " + damage);
            if (_currentScytheHealth <= 0)
            {
                // reset scythe
                HandleScytheDestruction();
            }
        }
        
        private void KnockbackScythe()
        {
            // apply knockback to the scythe
            _rb.AddForce((-_direction) * knockbackForce, ForceMode2D.Impulse);
            StopAllCoroutines();
            StartCoroutine(ResumeMovementAfterKnockback());
        }

        private IEnumerator ResumeMovementAfterKnockback()
        {
            // _isAttacking = false;
            // _rb.linearVelocity = Vector2.zero;
            
            // wait for a short duration before resuming movement
            yield return new WaitForSeconds(knockbackDurationSeconds);
            
            // _isAttacking = true;
            MoveScythe();
        }

        private void HandleScytheDestruction()
        {
            
            _isAttacking = false;
            Debug.Log("Scythe destroyed");
            //play destruction animation
            ResetScythe();
            // OnStartScytheAttack();
        }

        // This method is called when the player clicks on the scythe
        public void OnPointerClick(PointerEventData eventData)
        {
            TakeDamage(damageAgainstScythe);
        }

        // This method is called when the scythe collides with a creature or blocker
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
            else if(((1<< other.gameObject.layer) & _blockerLayerMaskValue) != 0)
            {
                Debug.Log("hit blocker");
                // apply damage to the blocker
                // after blocker took damage, the scythe needs to also take damage and get knocked back
            }
        }
    }
}