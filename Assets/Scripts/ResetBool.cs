using UnityEngine;

public class ResetBool : StateMachineBehaviour
{
    [SerializeField] private string isInteractingBool;
    [SerializeField] private bool isInteractingStatus;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(isInteractingBool, isInteractingStatus);
    }
}
