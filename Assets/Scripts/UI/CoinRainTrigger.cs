using System.Collections;
using UnityEngine;
using Utilities;

public class CoinRainTrigger : MonoBehaviour
{
    [SerializeField] private float autoDisableAfter = 12f;  // seconds

    private void OnEnable()
    {
        GameEvents.OnDoubleUpCoinsActivated += PlayOnce;
    }

    private void OnDisable()
    {
        GameEvents.OnDoubleUpCoinsActivated -= PlayOnce;
    }

    private void PlayOnce()
    {
        // run only if not already playing
        // if (gameObject.activeInHierarchy) return;

        // gameObject.SetActive(true);              // show → animation begins
        StartCoroutine(AutoDisable());
    }

    private IEnumerator AutoDisable()
    {
        yield return new WaitForSeconds(autoDisableAfter);
        gameObject.SetActive(false);             // hide until next boost
    }
}