using UnityEngine;

public class ArcherTriggerSMB : StateMachineBehaviour
{
    public string triggerToAdd;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetTrigger(triggerToAdd);
    }
}
