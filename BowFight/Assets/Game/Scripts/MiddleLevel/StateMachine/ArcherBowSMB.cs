using System;
using UnityEngine;

public class ArcherBowSMB : StateMachineBehaviour
{
    public BowConditionState condition;

    public BowActionType bowAction;

    public float delay;

    public float duration;

    public Action<float, float> OnEventLoadBow;
    public Action<float, float> OnEventShootArrow;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (condition == BowConditionState.OnEnter)
        {
            if (bowAction == BowActionType.Pull)
            {
                OnEventLoadBow?.Invoke(delay, duration);
            }
            else
            {
                OnEventShootArrow?.Invoke(delay, duration);
            }
        }
    }

    // OnStateExit is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (condition == BowConditionState.OnExit)
        {
            if (bowAction == BowActionType.Pull)
            {
                OnEventLoadBow?.Invoke(delay, duration);
            }
            else
            {
                OnEventShootArrow?.Invoke(delay, duration);
            }
        }
    }
}
