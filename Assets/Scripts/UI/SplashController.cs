using UnityEngine;

public class SplashController : MonoBehaviour
{
    [Tooltip("Assign all splash panels here")]
    [SerializeField] private GameObject[] splashPanels;

    private void Awake()
    {
        // ensure they start hidden (if you want them hidden at launch)
        foreach (var panel in splashPanels)
            if (panel != null)
                panel.SetActive(false);
    }

    /// <summary>
    /// Call this from your Main‐Screen "Open Splash" button.
    /// Activates all assigned splash panels.
    /// </summary>
    public void OpenSplashPanels()
    {
        foreach (var panel in splashPanels)
            if (panel != null)
                panel.SetActive(true);
    }

    /// <summary>
    /// Call this from your splash "Close" button.
    /// Deactivates all assigned splash panels.
    /// </summary>
    public void CloseSplashPanels()
    {
        foreach (var panel in splashPanels)
            if (panel != null)
                panel.SetActive(false);
    }
}