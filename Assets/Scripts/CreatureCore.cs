using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class CreatureCore : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    /* ---------- Inspector ---------- */
    [Header("Stage")]
    [Range(0, 12)] public int stageIndex;
    [SerializeField] private CreatureCore nextPrefab;

    [Header("Economy")]
    public int zuzPerSecond = 1;

    [Header("Idle Movement")]
    public float idleRadius   = 0.4f;
    public float idleStepTime = 2f;

    [Header("Merge")]
    public int   mergeNeeded    = 2;
    public float mergeRadius    = 0.6f;
    public float evolveAnimTime = 0.6f;

    /* ---------- Private ---------- */
    private bool    isDragging;
    private Vector3 dragOffset;
    private Vector3 idleTarget;
    private float   idleTimer, moneyTimer;
    private Camera  cam;
    [SerializeField] private Animator anim;

    /* ---------- MonoBehaviour ---------- */
    private void Start()
    {
        cam  = Camera.main;
        anim = GetComponent<Animator>();
        PickIdleTarget();
    }

    private void Update()
    {
        if (!isDragging)
            HandleIdle();      // Do not move randomly while being dragged

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
        //  ✕  this check blocks all physics hits
        //  if (EventSystem.current.IsPointerOverGameObject()) return;

        // optional: block only *graphics* (real UI), not physics objects
        // if (data.pointerEnter != null && data.pointerEnter.GetComponent<UnityEngine.UI.Graphic>())
        //     return;

        Debug.Log("PointerDown");                    // keep for testing

        isDragging = true;

        Vector3 world = cam.ScreenToWorldPoint(data.position);
        world.z = 0;
        dragOffset = transform.position - world;
    }

    public void OnDrag(PointerEventData data)
    {
        if (!isDragging) return;

        Debug.Log("Dragging");                       // test

        Vector3 world = cam.ScreenToWorldPoint(data.position);
        world.z = 0;
        transform.position = world + dragOffset;
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (!isDragging) return;

        Debug.Log("PointerUp");                      // test

        isDragging = false;
        StartCoroutine(TryMerge());
    }

    
    private void OnMouseDown() => Debug.Log("OnMouseDown-Physics-Hit");




    /* ---------- Merge-Evolution ---------- */
    private IEnumerator TryMerge()
    {
        if (nextPrefab == null) yield break;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, mergeRadius);
        var group = new List<CreatureCore>();

        foreach (var h in hits)
            if (h.TryGetComponent(out CreatureCore c) &&
                c.stageIndex == stageIndex && !c.isDragging)
                group.Add(c);

        if (group.Count < mergeNeeded) yield break;

        foreach (var c in group)
            if (c.anim) c.anim.CrossFade("Evolve", 0f, 0);

        yield return new WaitForSeconds(evolveAnimTime);

        Vector3 avg = Vector3.zero;
        foreach (var c in group) avg += c.transform.position;
        avg /= group.Count;

        Instantiate(nextPrefab, avg, Quaternion.identity);
        foreach (var c in group) Destroy(c.gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mergeRadius);
    }
#endif
}
