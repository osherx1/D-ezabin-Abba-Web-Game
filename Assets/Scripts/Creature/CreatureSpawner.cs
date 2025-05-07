using System.Collections;
using UnityEngine;

public class CreatureSpawner : MonoSingleton<CreatureSpawner>
{
    [Header("Random Spawn Bounds")]
    [Tooltip("Leftmost corner of the random-spawn area")]
    [SerializeField] private Vector2 bottomLeft = new Vector2(-5, -3);
    [Tooltip("Rightmost corner of the random-spawn area")]
    [SerializeField] private Vector2 topRight   = new Vector2( 5,  3);

    [Header("Spawn Effects by Stage")]
    [Tooltip("For each CreatureStage, pick the effect prefab and delay before spawning")]
    [SerializeField] private SpawnEffectEntry[] spawnEffects;

    [Tooltip("Where to play the Spawn effect and place the creature")]
    [SerializeField] private Transform specialSpawnPoint;

    /// <summary>
    /// Call this with any GameObject prefab.
    /// If it has a CreatureCore & there’s a matching entry in spawnEffects → special flow.
    /// Otherwise → random-bounds spawn.
    /// </summary>
    public void Spawn(GameObject prefab)
    {
        if (prefab == null) return;

        var core = prefab.GetComponent<CreatureCore>();
        if (core != null)
        {
            // find an entry for this CreatureStage
            for (int i = 0; i < spawnEffects.Length; i++)
            {
                var e = spawnEffects[i];
                if (e.stage == core.stage && e.effectPrefab != null)
                {
                    StartCoroutine(SpecialSpawn(prefab, e));
                    return;
                }
            }
        }

        // no special entry: fallback
        SimpleSpawn(prefab);
    }

    private void SimpleSpawn(GameObject prefab)
    {
        float x = Random.Range(bottomLeft.x, topRight.x);
        float y = Random.Range(bottomLeft.y, topRight.y);
        Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity);
    }

    private IEnumerator SpecialSpawn(GameObject prefab, SpawnEffectEntry entry)
    {
        // 1) play the effect
        Instantiate(entry.effectPrefab,
                    specialSpawnPoint.position,
                    Quaternion.identity);

        // 2) wait the configured delay
        yield return new WaitForSeconds(entry.spawnDelay);

        // 3) spawn the creature
        Instantiate(prefab,
                    specialSpawnPoint.position,
                    Quaternion.identity);
    }

    [System.Serializable]
    private struct SpawnEffectEntry
    {
        public CreatureStage stage;
        [Tooltip("The visual effect that plays before this creature appears")]
        public GameObject    effectPrefab;
        [Tooltip("Seconds to wait after effect before spawning the creature")]
        public float         spawnDelay;
    }
}
