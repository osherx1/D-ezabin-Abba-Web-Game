using System.Collections;
using UnityEngine;
using Utilities;

// for GameEvents

namespace UI
{
    public class ShieldEffectController : MonoBehaviour
    {
        [Tooltip("The layer your creatures are on (e.g. “Creatures”)")]
        [SerializeField] private LayerMask creatureLayer;

        private void OnEnable()
        {
            GameEvents.OnShieldActivated += HandleShieldActivated;
        }

        private void OnDisable()
        {
            GameEvents.OnShieldActivated -= HandleShieldActivated;
        }

        private void HandleShieldActivated(float duration)
        {
            StartCoroutine(ActivateShieldCircles(duration));
        }

        // /// <summary>
        // /// Call this from your UI Button's OnClick.
        // /// </summary>
        // public void TriggerShield()
        // {
        //     Debug.Log("Shield triggered");
        //     GameEvents.OnShieldActivated?.Invoke();
        // }

        private IEnumerator ActivateShieldCircles(float duration)
        {
            // 1) Find all CreatureCore components in the scene
            var allCores = FindObjectsOfType<CreatureCore>();

            // 2) For each one whose GameObject is on the chosen layer, enable its "circle" child
            foreach (var core in allCores)
            {
                var go = core.gameObject;
                if (((1 << go.layer) & creatureLayer) != 0)
                {
                    var circle = go.transform.Find("Circle");
                    if (circle != null)
                        circle.gameObject.SetActive(true);
                }
            }

            // 3) Wait 5 seconds
            yield return new WaitForSeconds(duration);

            // 4) Disable them again
            foreach (var core in allCores)
            {
                var go = core.gameObject;
                if (((1 << go.layer) & creatureLayer) != 0)
                {
                    var circle = go.transform.Find("Circle");
                    if (circle != null)
                        circle.gameObject.SetActive(false);
                }
            }
        }
    }
}