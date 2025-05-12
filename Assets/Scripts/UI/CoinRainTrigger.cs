using System.Collections;
using UnityEngine;
using Utilities;

namespace UI
{
    public class CoinRainTrigger : MonoBehaviour
    {
        [SerializeField] private float autoDisableAfter = 12f;  // seconds
        private Animator animator;
    
        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.enabled = false;
        }

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
            animator.enabled = true;
            // if (gameObject.activeInHierarchy) return;

            // gameObject.SetActive(true);              // show → animation begins
            StartCoroutine(AutoDisable());
        }

        private IEnumerator AutoDisable()
        {
            yield return new WaitForSeconds(autoDisableAfter);
            animator.enabled = false;                // stop animation
            // gameObject.SetActive(false);             // hide until next boost
        }
    }
}