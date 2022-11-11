using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrollingBehaviour : StateMachineBehaviour
{
    public float movementSpeed = 10f;
    public float maxDistance = 10f;
    public float turningSpeed = 1f;

    private int frameNr = 0;
    public Vector3 targetDirection;

    public NPC npc;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Entered Strolling!");
        npc = animator.GetComponent<NPC>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // find a random direction each few frames
        frameNr += 1;
        if (frameNr > 19) 
        {
            frameNr = 0;
            targetDirection = new Vector3(Random.Range(-maxDistance, maxDistance), animator.transform.position.y, Random.Range(-maxDistance, maxDistance));
        }
        else if (npc.Colliding())// won't it trigger many times?
        {
            // calculate the reflection vector
        // if just collided, find new position
        }
        // continue moving to the random direction along some path
        // if near a wall or other obstacle, calculate new reflection direction
        Vector3 tmpDest = new Vector3(animator.transform.position.x + targetDirection.x, animator.transform.position.y, animator.transform.position.z + targetDirection.z);

        animator.transform.position = Vector3.Lerp(animator.transform.position, tmpDest, Time.deltaTime * movementSpeed);

        // TODO: add rotations
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
