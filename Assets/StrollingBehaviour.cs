using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrollingBehaviour : StateMachineBehaviour
{
    public float movementSpeed = 0.01f;
    public float targetDistance = 1f;
    public float turningSpeed = 1f;

    public float positionDampTime = 0.12f;

    private int frameNr = 0;
    public Vector3 targetDestination;
    private Vector3 positionVelocity = Vector3.zero;

    public NPC npc;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Entered Strolling!");
        npc = animator.GetComponent<NPC>();
        targetDestination = animator.transform.position;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // find a random direction each few frames
        frameNr += 1;
        if (frameNr > 10) 
        {
            frameNr = 0;
            // draw a vector in a random direction of particular length
            // draw a vector of targetDistance forward, rotate it random angle and add to animator.transform.position
            // lerp also the quaternion
            Vector3 followVector = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * Vector3.forward * targetDistance;
            // or just fan in the angle -45,45 and adjust rotation every frame

            //tmpDest = new Vector3(Random.Range(-maxDistance, maxDistance), animator.transform.position.y, Random.Range(-maxDistance, maxDistance));
            targetDestination = new Vector3(animator.transform.position.x + followVector.x, animator.transform.position.y, animator.transform.position.z + followVector.z);
        }
        else if (npc.Colliding())// won't it trigger many times?
        {
            // calculate the reflection vector
        // if just collided, find new position
        }
        // continue moving to the random direction along some path
        // if near a wall or other obstacle, calculate new reflection direction

        //animator.transform.position = Vector3.Lerp(animator.transform.position, targetDestination, Time.deltaTime * movementSpeed * 0.1f); // TODO: this looks very weird :D (brownian motion)
        Vector3 smoothedPosition = Vector3.SmoothDamp(animator.transform.position, targetDestination, ref positionVelocity, positionDampTime);
        animator.transform.position = smoothedPosition;
        // TODO: add rotations
        // TODO: the target should be at a constant distance from the player so it won't reach it and will always keep on following it
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
