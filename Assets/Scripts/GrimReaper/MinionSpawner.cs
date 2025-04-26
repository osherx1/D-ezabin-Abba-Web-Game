using UnityEngine;

public class MinionSpawner : MonoBehaviour
{
    [Header("Minions Settings")]
    [SerializeField] private int minionCount = 5;
    [SerializeField] private float minionSpawnRadius = 5f;
    [SerializeField] private float minionSpeed = 2f;
    [SerializeField] private LayerMask creatureLayer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
