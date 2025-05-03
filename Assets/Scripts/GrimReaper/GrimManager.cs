using GrimReaper;
using UnityEngine;
using Utilities;

public class GrimManager : MonoBehaviour
{
    //TODO: attacks timing? 
    //todo: implement the Grim Reaper's attacks (Scythe, minions, etc...)
    //need : a way to find the most earning creature
    // todo: defense against the Grim Reaper's attacks, power ups?
    [Header("Scythe Settings")]
    [SerializeField] private ScytheAttack scythe;
    [SerializeField] private ScytheAttackSettings scytheAttackSettings;
    [SerializeField] private ScytheCounterAttackSettings scytheCounterAttackSettings;
    
    /// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void CallScytheAttack()
    {
        GameEvents.StartScytheAttack?.Invoke();
    }

    private void CallMinionsAttack(Transform traget)
    {
        // TODO: implement the minions attack
        // need: a way to find the most earning creature
    }
}

[System.Serializable]
public class ScytheAttackSettings
{
    public float scytheSpeed = 1f;
    public int scytheHealth = 10;
    public Vector2 scytheRightPosition;
    public Vector2 scytheLeftPosition;
}

[System.Serializable]
public class ScytheCounterAttackSettings
{
    public float knockbackForce = 1f;
    public float knockbackDurationSeconds = 0.5f;
    public int damageAgainstScythe = 1;
}
