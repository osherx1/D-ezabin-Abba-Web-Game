using System.Collections;
using UnityEngine;
using Utilities;

namespace UI
{
    public class CoinRainTrigger : MonoBehaviour
    {
        [SerializeField] private float autoDisableAfter = 12f;  // seconds
        private Animator _animator;
    
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animator.enabled = false;
        }

        private void OnEnable()
        {
            GameEvents.OnCoinBoost += PlayOnce;
        }

        private void OnDisable()
        {
            GameEvents.OnCoinBoost -= PlayOnce;
        }

        private void PlayOnce(float duration)
        {
            // run only if not already playing
            _animator.enabled = true;
            // if (gameObject.activeInHierarchy) return;

            // gameObject.SetActive(true);              // show → animation begins
            StartCoroutine(AutoDisable());
        }

        private IEnumerator AutoDisable()
        {
            yield return new WaitForSeconds(autoDisableAfter);
            _animator.enabled = false;                // stop animation
            // gameObject.SetActive(false);             // hide until next boost
        }
    }
}