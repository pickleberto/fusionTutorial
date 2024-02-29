using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static IEnumerator PlayAnimAndSetStateWhenFinished(GameObject parent, 
        Animator animator, string clipName, bool activeStateAtEnd = true)
    {
        animator.Play(clipName);
        var animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength);
        parent.SetActive(activeStateAtEnd);
    }
}
