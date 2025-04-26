using UnityEngine;
using Utilities;

public class GrimManager : MonoBehaviour
{
    //TODO: attacks timing? 
    //todo: implement the Grim Reaper's attacks (Scythe, minions, etc...)
    //need : a way to find the most earning creature
    // todo: defense against the Grim Reaper's attacks, power ups?
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
