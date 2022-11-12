using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoppedBehaviour : StateMachineBehaviour
{
    public NPC npc; 
    private Mighty.MightyTimer stopDurationTimer;

    public int stopDurationTimeMin = 10;
    public int stopDurationTimeMax = 15;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        npc = animator.GetComponent<NPC>();
        Debug.Log("Entered STOP");
        /*
        if (!npc.isPosessed)
        {
            Utils.ResetTimer(out stopDurationTimer, stopDurationTimeMin, stopDurationTimeMax);
        }*/
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (npc.isPosessed)
            return;

        /*if (stopDurationTimer.finished)
        {
            animator.SetBool(Utils.STOPSTRING, false);
        }*/
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
  }
