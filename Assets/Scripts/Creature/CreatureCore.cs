using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities;


[RequireComponent(typeof(Collider2D))]
public class CreatureCore : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    /* ---------- Inspector ---------- */
    [Header("Stage")] public CreatureStage stage;
    [SerializeField] private CreatureCore nextPrefab;

    [Header("Economy")] public int zuzPerSecond = 1;

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

    private void Update()
    {
        if (!isDragging) HandleIdle();
        GameEvents.OnMoneyChanged?.Invoke(zuzPerSecond);

        AnimateWalking();
        ProduceMoney();
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
    private void ProduceMoney()
    {
        moneyTimer += Time.deltaTime;
        if (moneyTimer >= 1f)
        {
            GameManager.Zuzim += zuzPerSecond;
            moneyTimer = 0f;
        }
    }

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
        if (nextPrefab == null) yield break;

        // Scan only colliders on the Creature layer
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position,
            mergeRadius,
            creatureMask);

        var others = new List<CreatureCore>();

        foreach (var h in hits)
            if (h.TryGetComponent(out CreatureCore c) &&
                c != this && // ← skip self
                !c.isDragging && // not being dragged
                c.stage == stage) // same enum stage
                others.Add(c);

        int participants = others.Count + 1; // +1 because 'this' is part of the merge

        if (participants < mergeNeeded) yield break;

        /* --- play evolve animation on ALL participants (self + others) --- */
        if (anim) anim.CrossFade("Evolve", 0f, 0);
        foreach (var c in others)
            if (c.anim)
                c.anim.CrossFade("Evolve", 0f, 0);

        yield return new WaitForSeconds(evolveAnimTime);

        /* --- spawn the next stage at the average position --- */
        Vector3 avg = transform.position;
        foreach (var c in others) avg += c.transform.position;
        avg /= participants;

        // Instantiate(nextPrefab, avg, Quaternion.identity);
        CreatureCore newCreature =
            Instantiate(nextPrefab, avg, Quaternion.identity);
        GameEvents.OnCreatureMerged?.Invoke(newCreature.stage);

        /* --- destroy the old creatures --- */
        foreach (var c in others) Destroy(c.gameObject);
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