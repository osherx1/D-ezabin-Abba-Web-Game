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

    /* ---------- Private ---------- */
    private bool isDragging;
    private Vector3 dragOffset;
    private Vector3 idleTarget;
    private float idleTimer, moneyTimer;
    private Camera cam;
    [SerializeField] private Animator anim;
    private Vector3 lastPosition;
    private float lastXDirection = 1f;


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
        MoneyManager.Instance.RegisterIncome(zuzPerSecond);
    }
    
    private void OnDestroy()
    {
        MoneyManager.Instance.UnregisterIncome(zuzPerSecond); 
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
        Vector2 r = Random.insideUnitCircle * idleRadius;
        idleTarget = transform.position + new Vector3(r.x, r.y, 0);
    }

    /* ---------- Animation ---------- */

    private void AnimateWalking()
    {
        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - lastPosition;

        bool isMoving = delta != Vector3.zero;

        if (anim)
            anim.SetBool("IsWalking", isMoving);

        if (isMoving && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            lastXDirection = Mathf.Sign(delta.x); // update direction only on horizontal move
        }

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

        // (8) Immediately destroy the other creatures
        foreach (var c in others)
            Destroy(c.gameObject);

        // (9) Wait until this creature’s Evolve state is done
        yield return new WaitUntil(() => {
            var st = anim.GetCurrentAnimatorStateInfo(0);
            return st.IsName("Evolve") && st.normalizedTime >= 1f;
        });

        // (10) Instantiate the merged creature prefab
        Instantiate(nextPrefab, avg, Quaternion.identity);

        // (11) Destroy this original creature
        Destroy(gameObject);
    }

    private bool _isDead;

    public void Death()
    {
        if (_isDead) return; // avoid double-kill
        _isDead = true;

        // 1) optional: play death animation / SFX here
        // if (anim) anim.CrossFade("Death", 0f, 0);

        // 2) optional: tell game manager, drop coins, etc.
        // GameManager.Zuzim += bonusOnDeath;
        // MoneyManager.Instance.UnregisterIncome(zuzPerSecond); 
        Destroy(gameObject, 0.05f); // destroy after one frame
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mergeRadius);
    }
#endif
}