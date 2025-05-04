using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Utilities;
using Random = UnityEngine.Random;

namespace GrimReaper
{
    public class SwipingAttack : MonoBehaviour, IPointerClickHandler
    {
        [Header("Target and Blocker layers")] 
        [SerializeField] private LayerMask creatureLayerMask;
        private int _creatureLayerMaskValue;
        [SerializeField] private LayerMask blockerLayerMask;
        private int _blockerLayerMaskValue;
        
        [Header("Attack parameters")] 
        [SerializeField] private float swipingSpeed = 1f;
        [SerializeField] private int swipingHealth = 10;
        [SerializeField] private Vector2 swipingRightPosition;
        [SerializeField] private Vector2 swipingLeftPosition;
        private int _currentSwipingHealth;
        private Vector2 _swipingStartPosition;
        private Vector2 _swipingEndPosition;
        private Vector2 _direction;

        [Header("Counter attack parameters")] 
        [SerializeField] private float knockbackForce = 1f;
        [SerializeField] private float knockbackDurationSeconds = 0.5f;
        [SerializeField] private int damageAgainstSwiping = 1;

        private Rigidbody2D _rb;
        private bool _isAttacking;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            // OnStartSwipingAttackAttack(); // for now, starts the SwipingAttack attack immediately
            _creatureLayerMaskValue = creatureLayerMask.value;
            _blockerLayerMaskValue = blockerLayerMask.value;
            _swipingStartPosition = swipingRightPosition;
            _swipingEndPosition = swipingLeftPosition;
            ResetSwiping(); // set the SwipingAttack to its start position
        }

        private void OnEnable()
        {
            GameEvents.StartSwipingAttack += OnStartSwipingAttack;
        }
        
        private void OnDisable()
        {
            GameEvents.StartSwipingAttack -= OnStartSwipingAttack;
        }

        private void OnStartSwipingAttack()
        {
            if(_isAttacking) return;
            // reset SwipingAttack position and health
            SetSwipingDirectionRandomly(); // needs to be set outside from the grim manager
            SetSwipingStartPosition();
            ResetSwiping();
            // make SwipingAttack visible?
            _isAttacking = true;
            // start movement
            MoveSwiping();
        }

        private void MoveSwiping()
        { 
            _rb.linearVelocity = _direction * swipingSpeed;
            
            // check if the SwipingAttack has reached the end position
            if (Vector2.Distance(transform.position, _swipingEndPosition) < 0.1f)
            {
                // _rb.linearVelocity = Vector2.zero;
                ResetSwiping();
            }
        }

        private void SetSwipingDirectionRandomly()
        {
            int dir = Random.Range(0, 2);
            _direction = dir == 0 ? (swipingRightPosition - swipingLeftPosition).normalized : (swipingLeftPosition - swipingRightPosition).normalized;
        }
        
        public void SetSwipingDirection(Vector2 direction)
        {
            // set the SwipingAttack direction to the given direction
            _direction = direction.normalized;
        }
        
        private void SetSwipingStartPosition()
        {
            // set the SwipingAttack start position to the current position
            if(_direction.x > 0)
            {
                _swipingStartPosition = swipingLeftPosition;
                _swipingEndPosition = swipingRightPosition;
            }
            else
            {
                _swipingStartPosition = swipingRightPosition;
                _swipingEndPosition = swipingLeftPosition;
            }
        }

        private void ResetSwiping()
        {
            // reset the SwipingAttack to its start position
            // reset the SwipingAttack health
            _isAttacking = false;
            _currentSwipingHealth = swipingHealth;
            transform.position = _swipingStartPosition;
            _rb.linearVelocity = Vector2.zero;
            StopAllCoroutines();
        }

        private void TakeDamage(int damage)
        {
            _currentSwipingHealth -= damage;
            KnockbackSwipingAttack();
            Debug.Log("SwipingAttack took damage: " + damage);
            if (_currentSwipingHealth <= 0)
            {
                // reset SwipingAttack
                HandleSwipingAttackDestruction();
            }
        }
        
        private void KnockbackSwipingAttack()
        {
            // apply knockback to the SwipingAttack
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
            MoveSwiping();
        }

        private void HandleSwipingAttackDestruction()
        {
            
            _isAttacking = false;
            Debug.Log("SwipingAttack destroyed");
            //play destruction animation
            ResetSwiping();
            // OnStartSwipingAttackAttack();
        }

        // This method is called when the player clicks on the SwipingAttack
        public void OnPointerClick(PointerEventData eventData)
        {
            TakeDamage(damageAgainstSwiping);
        }

        // This method is called when the SwipingAttack collides with a creature or blocker
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
                // after blocker took damage, the SwipingAttack needs to also take damage and get knocked back
            }
        }
        // Setters
        public void SetSwipingAttackSpeed(float speed)
        {
            swipingSpeed = speed;
        }
        public void SetSwipingAttackHealth(int health)
        {
            swipingHealth = health;
        }
        public void SetSwipingAttackRightPosition(Vector2 position)
        {
            swipingRightPosition = position;
        }
        public void SetSwipingAttackLeftPosition(Vector2 position)
        {
            swipingLeftPosition = position;
        }
        public void SetKnockbackForce(float force)
        {
            knockbackForce = force;
        }
        public void SetKnockbackDurationSeconds(float duration)
        {
            knockbackDurationSeconds = duration;
        }
        public void SetDamageAgainstSwipingAttack(int damage)
        {
            damageAgainstSwiping = damage;
        }
    }
}