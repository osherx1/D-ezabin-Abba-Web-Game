using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities;
using Random = UnityEngine.Random;


[RequireComponent(typeof(Collider2D))]
public class CreatureCore : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    /* ---------- Inspector ---------- */
    [Header("Stage")] public CreatureStage stage;
    [SerializeField] private CreatureCore nextPrefab;
    [SerializeField] private GameObject evolveEffectPrefab;

    [Header("Economy")] public float zuzPerSecond = 0.5f;

    [Header("Idle Movement")] public float idleRadius = 0.4f;
    public float idleStepTime = 2f;

    [Header("Merge")] public int mergeNeeded = 2;
    public float mergeRadius = 0.6f;
    public float evolveAnimTime = 0.6f;
    
    [Header("Drag & Fall Bounds")]
    [Tooltip("Creatures must stay within this area; drop outside → fall.")]
    [SerializeField] private Vector2 allowedBottomLeft  = new Vector2(-5, -3);
    [SerializeField] private Vector2 allowedTopRight    = new Vector2( 5,  3);

    [Header("Fall Animation")]
    [Tooltip("Name of your Fall state in the Animator")]
    [SerializeField] private string   fallStateName     = "Fall";
    [SerializeField] private float    fallAnimTime      = 0.6f;


    /* ---------- Private ---------- */
    private bool isDragging;
    private Vector3 dragOffset;
    private Vector3 idleTarget;
    private float idleTimer, moneyTimer;
    private Camera cam;
    [SerializeField] private Animator anim;
    private Vector3 lastPosition;
    private float lastXDirection = 1f;
    private static bool _oxButcherMergeFired = false;
    private bool _hasFlippedOnBorder = false;

    /* ---------- MonoBehaviour ---------- */
    private void Start()
    {
        cam = Camera.main;
        anim = GetComponent<Animator>();
        lastPosition = transform.position;
        PickIdleTarget();
    }

    private void OnEnable()
    {
        MoneyManager.Instance?.RegisterIncome(zuzPerSecond);
    }
    
    private void OnDestroy()
    {
        MoneyManager.Instance?.UnregisterIncome(zuzPerSecond); 
    }

    private void Update()
    {
        if (!isDragging) HandleIdle();
        // GameEvents.OnMoneyChanged?.Invoke(zuzPerSecond);

        AnimateWalking();
        // ProduceMoney();
    }


    /* ---------- Idle Walk ---------- */
    private void HandleIdle()
    {
        idleTimer += Time.deltaTime;
        transform.position = Vector3.MoveTowards(
            transform.position,
            idleTarget,
            Time.deltaTime * idleRadius / idleStepTime);

        if (idleTimer >= idleStepTime)
        {
            PickIdleTarget();
            idleTimer = 0f;
        }
    }

    private void PickIdleTarget()
    {
        // pick a random point within idleRadius
        Vector2 r = Random.insideUnitCircle * idleRadius;
        Vector3 candidate = transform.position + new Vector3(r.x, r.y, 0);

        // clamp it to the allowed square
        candidate.x = Mathf.Clamp(candidate.x,
            allowedBottomLeft.x,
            allowedTopRight.x);
        candidate.y = Mathf.Clamp(candidate.y,
            allowedBottomLeft.y,
            allowedTopRight.y);

        idleTarget = candidate;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_hasFlippedOnBorder && collision.collider.CompareTag("Border"))
        {
            _hasFlippedOnBorder = true;

            // 1) Flip the sprite and direction
            FlipDirection();

            // 2) Pick a fresh idleTarget away from the wall
            PickIdleTargetInDirection(lastXDirection);

            // 3) Reset the idle timer so it moves immediately
            idleTimer = 0f;

            // 4) Reset lastPosition to avoid a bogus delta in AnimateWalking
            lastPosition = transform.position;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Border"))
            _hasFlippedOnBorder = false;
    }
    
    private void PickIdleTargetInDirection(float direction)
    {
        // random point in a circle
        Vector2 r = Random.insideUnitCircle * idleRadius;
        // force it to lie in the new horizontal direction
        r.x = Mathf.Abs(r.x) * direction;

        Vector3 candidate = transform.position + new Vector3(r.x, r.y, 0f);
        // clamp to your play‐area
        candidate.x = Mathf.Clamp(candidate.x,
            allowedBottomLeft.x,
            allowedTopRight.x);
        candidate.y = Mathf.Clamp(candidate.y,
            allowedBottomLeft.y,
            allowedTopRight.y);

        idleTarget = candidate;
    }
    
    private void FlipDirection()
    {
        lastXDirection *= -1f;
        Vector3 s = transform.localScale;
        s.x = lastXDirection * Mathf.Abs(s.x);
        transform.localScale = s;
    }



    /* ---------- Animation ---------- */

    private void AnimateWalking()
    {
        Vector3 currentPosition = transform.position;
        Vector3 delta           = currentPosition - lastPosition;

        bool isMoving = delta.sqrMagnitude > Mathf.Epsilon;
        if (anim) anim.SetBool("IsWalking", isMoving);

        // only flip on horizontal movement
        if (delta.x != 0f)
            lastXDirection = Mathf.Sign(delta.x);

        Vector3 scale = transform.localScale;
        scale.x = lastXDirection * Mathf.Abs(scale.x);
        transform.localScale = scale;

        lastPosition = currentPosition;
    }

    /* ---------- Passive Income ---------- */
    // private void ProduceMoney()
    // {
    //     moneyTimer += Time.deltaTime;
    //     if (moneyTimer >= 1f)
    //     {
    //         GameManager.Zuzim += zuzPerSecond;
    //         moneyTimer = 0f;
    //     }
    // }

    /* =====================================================================
       Drag-and-Drop – works with new/old Input System, desktop & mobile
       ===================================================================== */
    public void OnPointerDown(PointerEventData data)
    {
        isDragging = true;
        Vector3 world = cam.ScreenToWorldPoint(data.position);
        world.z = 0;
        dragOffset = transform.position - world;

        // — NEW: play lift animation
        if (anim != null)
            anim.CrossFade("Lifted", 0f, 0);
    }


    public void OnDrag(PointerEventData data)
    {
        if (!isDragging) return;

        Vector3 world = cam.ScreenToWorldPoint(data.position);
        world.z = 0;
        transform.position = world + dragOffset;
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (!isDragging) return;
        isDragging = false;

        // world drop position under the cursor
        Vector3 world = cam.ScreenToWorldPoint(data.position);
        world.z = 0;
        Vector3 dropPos = world + dragOffset;

        // clamp X/Y to your allowed bounds
        float clampedX = Mathf.Clamp(dropPos.x,
            allowedBottomLeft.x,
            allowedTopRight.x);
        float clampedY = Mathf.Clamp(dropPos.y,
            allowedBottomLeft.y,
            allowedTopRight.y);
        Vector3 clampedPos = new Vector3(clampedX, clampedY, transform.position.z);

        // move the creature back inside
        transform.position = clampedPos;

        // return to idle animation
        if (anim != null)
            anim.CrossFade("Idle", 0f, 0);

        // then attempt merge as before
        StartCoroutine(TryMerge());
    }



    /* ---------- Merge-Evolution ---------- */

    [Header("Layers")] [SerializeField] private LayerMask creatureMask; // set to "Creature" in the Inspector

    private IEnumerator TryMerge()
{
    // (1) Abort if missing references
    if (nextPrefab == null || evolveEffectPrefab == null)
        yield break;

    // (2) Find all CreatureCore colliders in range
    Collider2D[] hits = Physics2D.OverlapCircleAll(
        transform.position, mergeRadius, creatureMask);

    // (3) Filter valid partners
    var others = new List<CreatureCore>();
    foreach (var h in hits)
        if (h.TryGetComponent(out CreatureCore c) &&
            c != this &&
            !c.isDragging &&
            c.stage == stage)
            others.Add(c);

    // If more than one match, keep only the closest
    if (others.Count > 1)
    {
        CreatureCore closest = null;
        float minDist = float.MaxValue;
        foreach (var c in others)
        {
            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < minDist)
            {
                minDist = d;
                closest = c;
            }
        }
        others.Clear();
        if (closest != null) others.Add(closest);
    }

    // (4) Need at least mergeNeeded total (this + others)
    if (others.Count + 1 < mergeNeeded)
        yield break;

    // (5) Compute average spawn position
    Vector3 avg = transform.position;
    foreach (var c in others) avg += c.transform.position;
    avg /= (others.Count + 1);

    // (6) Play this creature’s evolve animation
    if (anim != null)
        anim.CrossFade("Evolve", 0f, 0);

// (7) Spawn the visual‐only effect at avg
    Instantiate(evolveEffectPrefab, avg, Quaternion.identity);

// ─── Hide/destroy the merge partners right away ───
    foreach (var c in others)
    {
        // Option A: instantly destroy them
        Destroy(c.gameObject);

        // — or Option B: just deactivate them so you can still
        //   reference them later if needed:
        // c.gameObject.SetActive(false);
    }

// (8) Wait until this creature’s Evolve state is done
    yield return new WaitUntil(() => {
        var st = anim.GetCurrentAnimatorStateInfo(0);
        return st.IsName("Evolve") && st.normalizedTime >= 0.3f;
    });

// (9) Pre-instantiate the merged creature inactive
    var merged = Instantiate(nextPrefab, avg, Quaternion.identity);
    merged.gameObject.SetActive(false);
    // (10) Handle one-time OxButcher UI event case
    if (stage == CreatureStage.OxButcher && !_oxButcherMergeFired)
    {
        _oxButcherMergeFired = true;
        GameEvents.OnOxButcherMerged?.Invoke();
        Destroy(gameObject);    // this original goes away
        yield break;            // skip the normal spawn activation
    }

    // ───────────────────────────────────────────────────────────────
    // (11) Activate the merged creature in place
    

    // (12) Fire the generic merge event
    merged.transform.position = avg;
    merged.gameObject.SetActive(true);
    GameEvents.OnCreatureMerged?.Invoke(merged.stage);

    // (13) Finally destroy this original
    Destroy(gameObject);
}

    private IEnumerator FallFromCursor()
    {
        // play fall animation
        if (anim != null)
            anim.CrossFade(fallStateName, 0f, 0);

        // wait for it to finish
        yield return new WaitForSeconds(fallAnimTime);

        Destroy(gameObject);
    }


    private bool _isDead;

    public void Death()
    {
        if (_isDead) return;
        _isDead = true;

        if (anim != null)
            StartCoroutine(PlayHitAndDie());
        else
            Destroy(gameObject, 0.05f);  // fallback
    }

    private IEnumerator PlayHitAndDie()
    {
        // 1) crossfade into the “Hit” state (layer 0, no transition time)
        anim.CrossFade("Hit", 0f, 0);

        // 2) wait until “Hit” has played through once
        yield return new WaitUntil(() => {
            var st = anim.GetCurrentAnimatorStateInfo(0);
            return st.IsName("Hit") && st.normalizedTime >= 1f;
        });

        // 3) now destroy the GameObject
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mergeRadius);
    }
#endif
}