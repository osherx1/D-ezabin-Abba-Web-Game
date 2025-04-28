using UnityEngine;

public class CreatureSpawner : MonoSingleton<CreatureSpawner>
{
    // [Header("Prefab")]
    // [SerializeField] CreatureCore basePrefab;

    // [Header("Timing")]
    // [SerializeField] float interval = 4f;

    [Header("Spawn Bounds")]
    [Tooltip("leftest corner of the area")]
    [SerializeField] Vector2 bottomLeft = new Vector2(-5, -3);

    [Tooltip("rightest corner of the area")]
    [SerializeField] Vector2 topRight   = new Vector2(5, 3);

    // void Start() => InvokeRepeating(nameof(Spawn), 0f, interval);

    public void Spawn(GameObject prefab)
    {
        float x = Random.Range(bottomLeft.x, topRight.x);
        float y = Random.Range(bottomLeft.y, topRight.y);
        Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 size = new Vector3(
            Mathf.Abs(topRight.x - bottomLeft.x),
            Mathf.Abs(topRight.y - bottomLeft.y),
            0.01f);
        Vector3 center = new Vector3(
            (topRight.x + bottomLeft.x) / 2f,
            (topRight.y + bottomLeft.y) / 2f,
            0f);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}