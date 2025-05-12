using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
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
    private bool _isClamping = false;
    private Rigidbody2D _rb;
    private const float SAFE_MARGIN_X = 3f;
    private const float SAFE_MARGIN_Y = 2f;
    private bool _incomeRegistered = false;

    /* ---------- MonoBehaviour ---------- */
    
    private void Awake()
    {
        if (!_incomeRegistered)
        {
            MoneyManager.Instance?.RegisterIncome(zuzPerSecond);
            _incomeRegistered = true;
        }
    }
    
    private void Start()
    {
        cam = Camera.main;
        anim = GetComponent<Animator>();
        _rb  = GetComponent<Rigidbody2D>();
        lastPosition = transform.position;
        PickIdleTarget();
    }
    
    private void OnDestroy()
    {
        if (_incomeRegistered)
        {
            MoneyManager.Instance?.UnregisterIncome(zuzPerSecond);
            _incomeRegistered = false;
        }
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
        // compute “safe” rectangle
        float minX = allowedBottomLeft.x + SAFE_MARGIN_X;
        float maxX = allowedTopRight.x   - SAFE_MARGIN_X;
        float minY = allowedBottomLeft.y + SAFE_MARGIN_Y;
        float maxY = allowedTopRight.y   - SAFE_MARGIN_Y;

        Vector3 candidate;
        int attempts = 0;

        // keep sampling until we land in the safe zone (or bail after 10 tries)
        do
        {
            Vector2 r = Random.insideUnitCircle * idleRadius;
            candidate = transform.position + new Vector3(r.x, r.y, 0f);

            // clamp into the safe rectangle
            candidate.x = Mathf.Clamp(candidate.x, minX, maxX);
            candidate.y = Mathf.Clamp(candidate.y, minY, maxY);
            attempts++;
        }
        while ((candidate.x <= minX || candidate.x >= maxX ||
                candidate.y <= minY || candidate.y >= maxY)
               && attempts < 10);

        idleTarget = candidate;
    }

    private void PickIdleTargetInDirection(float direction)
    {
        // compute “safe” rectangle
        float minX = allowedBottomLeft.x + SAFE_MARGIN_X;
        float maxX = allowedTopRight.x   - SAFE_MARGIN_X;
        float minY = allowedBottomLeft.y + SAFE_MARGIN_Y;
        float maxY = allowedTopRight.y   - SAFE_MARGIN_Y;

        Vector3 candidate;
        int attempts = 0;

        do
        {
            Vector2 r = Random.insideUnitCircle * idleRadius;
            // force x to be in the “direction” (±)
            r.x = Mathf.Abs(r.x) * direction;
            candidate = transform.position + new Vector3(r.x, r.y, 0f);

            // clamp into the safe rectangle
            candidate.x = Mathf.Clamp(candidate.x, minX, maxX);
            candidate.y = Mathf.Clamp(candidate.y, minY, maxY);
            attempts++;
        }
        while ((candidate.x <= minX || candidate.x >= maxX ||
                candidate.y <= minY || candidate.y >= maxY)
               && attempts < 10);

        idleTarget = candidate;
    }
    
    private bool IsWithinAllowedBounds()
    {
        return transform.position.x >= allowedBottomLeft.x -1  &&
               transform.position.x <= allowedTopRight.x -1  &&
               transform.position.y >= allowedBottomLeft.y -1 &&
               transform.position.y <= allowedTopRight.y -1;
    }
    
    private void FlipDirection()
    {
        lastXDirection *= -1f;
        Vector3 s = transform.localScale;
        s.x = lastXDirection * Mathf.Abs(s.x);
        transform.localScale = s;
    }
    
    private Vector3 GetNearestPointInsideBounds(Vector3 worldPos)
    {
        float x = Mathf.Clamp(worldPos.x,
            allowedBottomLeft.x,
            allowedTopRight.x);
        float y = Mathf.Clamp(worldPos.y,
            allowedBottomLeft.y,
            allowedTopRight.y);
        return new Vector3(x, y, worldPos.z);
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

        if (!IsWithinAllowedBounds())
        {
            transform.position = GetNearestPointInsideBounds(transform.position);
        }
        
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

    AudioManager.Instance.PlaySound(
        transform.position,
        "MergingCreatures");
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
    if (stage == CreatureStage.OxMonster && !_oxButcherMergeFired)
    {
        _oxButcherMergeFired = true;
        GameEvents.OnCreatureMerged?.Invoke(merged.stage);
        Debug.Log("Merged stage:" + merged.stage);
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
    Debug.Log("Merged stage:" + merged.stage);

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
    
    
    private void LateUpdate()
    {
        if (!_isClamping)
            CheckAndClampBounds();
    }

    private void CheckAndClampBounds()
    {
        Vector3 pos = transform.position;
        bool   hitX = false;

        // detect horizontal overflow
        if (pos.x < allowedBottomLeft.x)
        {
            pos.x = allowedBottomLeft.x;
            hitX  = true;
        }
        else if (pos.x > allowedTopRight.x)
        {
            pos.x = allowedTopRight.x;
            hitX  = true;
        }

        // always clamp Y too
        pos.y = Mathf.Clamp(pos.y,
            allowedBottomLeft.y,
            allowedTopRight.y);

        if (hitX)
        {
            // snap into bounds and start the clamp-flip coroutine
            transform.position = pos;
            StartCoroutine(ClampFlipAndBounce(pos));
        }
        else
        {
            // just apply the Y‐clamp
            transform.position = pos;
        }
    }

    private IEnumerator ClampFlipAndBounce(Vector3 clampedPos)
    {
        _isClamping = true;

        // 1) Turn off physics so it won't immediately re‐hit the wall
        if (_rb != null) _rb.simulated = false;

        // 2) Flip your facing direction (180°)
        lastXDirection *= -1f;
        var s = transform.localScale;
        s.x = lastXDirection * Mathf.Abs(s.x);
        transform.localScale = s;

        // 3) Immediately pick a new idle target in that flipped direction
        //    and reset the timer so HandleIdle() moves right away.
        PickIdleTargetInDirection(lastXDirection);
        idleTimer = idleStepTime;     // force immediate PickIdleTarget in next HandleIdle
        lastPosition = clampedPos;    // avoid any delta‐flip back

        // 4) Wait two physics frames to clear any overlap
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // 5) Turn physics back on and zero out velocity
        if (_rb != null)
        {
            _rb.simulated = true;
            _rb.linearVelocity  = Vector2.zero;
        }

        _isClamping = false;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Border"))
            return;

        // 1) Kill any physics velocity so we don’t keep sliding into the wall
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }

        // 2) Flip facing direction 180°
        lastXDirection = -lastXDirection;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * lastXDirection;
        transform.localScale = s;

        // 3) Pick a *fixed* bounce‐away target directly horizontal
        Vector3 bounceTarget = transform.position 
                               + new Vector3(idleRadius * lastXDirection, 0f, 0f);

        // clamp it just inside your bounds
        bounceTarget.x = Mathf.Clamp(bounceTarget.x, 
            allowedBottomLeft.x, 
            allowedTopRight.x);
        bounceTarget.y = Mathf.Clamp(bounceTarget.y, 
            allowedBottomLeft.y, 
            allowedTopRight.y);

        // 4) Immediately move toward that bounce point
        idleTarget = bounceTarget;
        idleTimer  = idleStepTime;      // force instant recalc in HandleIdle
        lastPosition = transform.position;
    }

    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mergeRadius);
    }
#endif
}