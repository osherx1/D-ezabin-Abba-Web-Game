using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class SelfDestructOnAnimEnd : MonoBehaviour
{
    [Tooltip("The name of the animation state to wait for (e.g. 'Spawn')")]
    public string stateName = "";

    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(DieAtEnd());
    }

    IEnumerator DieAtEnd()
    {
        // wait until the named state is playing
        yield return new WaitUntil(() =>
            anim.GetCurrentAnimatorStateInfo(0).IsName(stateName));

        // then wait until that state finishes
        yield return new WaitUntil(() =>
            anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

        Destroy(gameObject);
    }
}