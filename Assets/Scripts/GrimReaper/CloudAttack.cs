using UnityEngine;
using Utilities;

public class CloudAttack : MonoBehaviour
{

    private void CallDestroyEnemies()
    {
        GameEvents.DestroyAllEnemies?.Invoke();
    }
    
    private void CallStopEnemies()
    {
        GameEvents.StopAllEnemies?.Invoke();
    }
    
    private void SelftDestruct()
    {
        Destroy(gameObject);
    }
}
