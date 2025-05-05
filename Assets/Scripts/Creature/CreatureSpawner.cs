using System.Collections;
using UnityEngine;

public class CreatureSpawner : MonoSingleton<CreatureSpawner>
{
    [Header("Random Spawn Bounds")]
    [Tooltip("Leftmost corner of the random-spawn area")]
    [SerializeField] private Vector2 bottomLeft  = new Vector2(-5, -3);
    [Tooltip("Rightmost corner of the random-spawn area")]
    [SerializeField] private Vector2 topRight    = new Vector2( 5,  3);

    [Header("Special Goat Spawn")]
    [Tooltip("Which CreatureStage should use the special flow?")]
    [SerializeField] private CreatureStage specialStage = CreatureStage.Goat;
    [Tooltip("Where to play the spawn effect and place the goat")]
    [SerializeField] private Transform   specialSpawnPoint;
    [Tooltip("Prefab with the ‘Spawn’ animation (plays then self-destructs)")]
    [SerializeField] private GameObject  spawnEffectPrefab;
    [Tooltip("Total duration of the spawnEffectPrefab’s animation (sec)")]
    [SerializeField] private float       spawnEffectDuration = 1f;
    [SerializeField] private float spawnDelay = 0.6f;

    /// <summary>
    /// Call this to spawn any prefab.
    /// If it has a CreatureCore and its .stage == specialStage → special sequence.
    /// Otherwise → simple random-bounds spawn.
    /// </summary>
    public void Spawn(GameObject prefab)
    {
        if (prefab == null) return;

        // see if this prefab carries a CreatureCore and what its stage is
        var core = prefab.GetComponent<CreatureCore>();
        if (core != null && core.stage == specialStage)
            StartCoroutine(SpecialGoatSpawn(prefab));
        else
            SimpleSpawn(prefab);
    }

    /// <summary>
    /// The old behavior: pick a random point inside the rectangle and instantiate.
    /// </summary>
    private void SimpleSpawn(GameObject prefab)
    {
        float x = Random.Range(bottomLeft.x, topRight.x);
        float y = Random.Range(bottomLeft.y, topRight.y);
        Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity);
    }

    /// <summary>
    /// Special flow for goats:
    /// 1) Spawn the visual effect prefab at the designated point.
    /// 2) Wait half its animation duration.
    /// 3) Instantiate the goat prefab exactly there.
    /// (Optional: wait the rest of the effect so it cleans itself up nicely.)
    /// </summary>
    private IEnumerator SpecialGoatSpawn(GameObject prefab)
    {
        Debug.Log("SpecialGoatSpawn: enter, effectPrefab=" + spawnEffectPrefab);

        
            // 1) Play your “spawn” effect
            Debug.Log("SpecialGoatSpawn: instantiating effect at " + specialSpawnPoint.position);
            Instantiate(spawnEffectPrefab,
                specialSpawnPoint.position,
                Quaternion.identity);

            // 2) wait half the effect’s duration
            yield return new WaitForSeconds(spawnDelay);

        // 3) now actually spawn the goat
        Debug.Log("SpecialGoatSpawn: instantiating goat prefab");
        Instantiate(prefab,
            specialSpawnPoint.position,
            Quaternion.identity);

        // 4) optionally wait the remainder
        // yield return new WaitForSeconds(spawnEffectDuration * 0.5f);
    }

}
