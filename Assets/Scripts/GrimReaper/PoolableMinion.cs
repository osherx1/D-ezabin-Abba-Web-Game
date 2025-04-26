using Pool;
using UnityEngine;

public class PoolableMinion : MonoBehaviour, IPoolable
{
    private Transform _target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reset()
    {
        _target = null;
    }
}
