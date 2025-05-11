using System.Collections;
using UnityEngine;
using Utilities;    // namespace for GameEvents

public class UIWinManager : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private GameObject canvasRoot;      // your disabled Canvas
    [SerializeField] private Animator  childAnimator;    // the Animator on the child

    [Header("Creature to spawn at (0,0)")]
    [SerializeField] private GameObject oxidButcherPrefab;

    private void OnEnable()
    {
        GameEvents.OnOxButcherMerged += HandleOxButcherMerged;
    }

    private void OnDisable()
    {
        GameEvents.OnOxButcherMerged -= HandleOxButcherMerged;
    }

    private void HandleOxButcherMerged()
    {
        StartCoroutine(ShowThenSpawn());
    }

    private IEnumerator ShowThenSpawn()
    {
        // 1) turn on the UI
        canvasRoot.SetActive(true);
        childAnimator.gameObject.SetActive(true);

        // 2) restart the animation to frame 0
        childAnimator.Play(
            childAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash,
            0,
            0f
        );

        // 3) wait for it to finish
        //    (assumes a single clip; if you have multiple, pick the right one)
        var clipInfo = childAnimator.GetCurrentAnimatorClipInfo(0);
        float animLength = clipInfo[0].clip.length;
        yield return new WaitForSeconds(animLength - 0.5f);

        // 4) turn the UI off
        canvasRoot.SetActive(false);

        // 5) now spawn the new creature at world‐zero
        Instantiate(oxidButcherPrefab, Vector3.zero, Quaternion.identity);
    }
}