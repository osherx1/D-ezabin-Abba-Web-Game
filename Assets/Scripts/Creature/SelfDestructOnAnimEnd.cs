using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class SelfDestructOnAnimEnd : MonoBehaviour
{
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(DieAtEnd());
    }

    IEnumerator DieAtEnd()
    {
        // 1) Wait until the Evolve state is actually playing
        yield return new WaitUntil(() =>
            anim.GetCurrentAnimatorStateInfo(0).IsName("Evolve"));

        // 2) Then wait until that state's normalizedTime ≥ 1 (animation done)
        yield return new WaitUntil(() =>
            anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        
        Destroy(gameObject);
    }
}