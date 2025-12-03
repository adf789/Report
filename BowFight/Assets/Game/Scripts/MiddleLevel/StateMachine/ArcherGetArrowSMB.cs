using UnityEngine;
using System;

public class ArcherGetArrowSMB : StateMachineBehaviour
{
    public float getArrowDelay = 0f;

    public Action<float> OnEventGetArrow;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        OnEventGetArrow?.Invoke(getArrowDelay);
    }
}
