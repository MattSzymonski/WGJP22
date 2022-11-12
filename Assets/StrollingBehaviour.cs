using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrollingBehaviour : StateMachineBehaviour
{
    public float movementSpeed = 0.01f;
    public float targetDistance = 1f;
    public float rotationSpeed = 1f;
    public float raycastDistance = 5f;

    public float positionDampTime = 0.12f;

    private int nextDirectionChangeDelta = 0;
    public int directionChangeTimeMin = 1;
    public int directionChangeTimeMax = 10;

    private Mighty.MightyTimer directionChangeTimer;

    public Vector3 targetDestination;
    public Quaternion targetRotation;

    private Vector3 positionVelocity = Vector3.zero;
    private Rigidbody rb;

    public NPC npc;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Entered Strolling!");
        npc = animator.GetComponent<NPC>();
        rb = animator.GetComponent<Rigidbody>();
        targetDestination = animator.transform.position;
        nextDirectionChangeDelta = Random.Range(directionChangeTimeMin, directionChangeTimeMax);
        ResetTimer();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {/*
        // find a random direction each few frames
        frameNr += 1;
        if (frameNr > 10) 
        {
            frameNr = 0;
            // draw a vector in a random direction of particular length
            // draw a vector of targetDistance forward, rotate it random angle and add to animator.transform.position
            // lerp also the quaternion
            float rand = Random.Range(0, 360);
            targetRotation = Quaternion.Euler(animator.transform.rotation.x, rand, animator.transform.rotation.z);
            Vector3 followVector = Quaternion.AngleAxis(rand, Vector3.up) * Vector3.forward * targetDistance;
            // or just fan in the angle -45,45 and adjust rotation every frame

            //tmpDest = new Vector3(Random.Range(-maxDistance, maxDistance), animator.transform.position.y, Random.Range(-maxDistance, maxDistance));
            targetDestination = new Vector3(animator.transform.position.x + followVector.x, animator.transform.position.y, animator.transform.position.z + followVector.z);
        }
        else if (npc.Colliding())// won't it trigger many times?
        {
            // calculate the reflection vector
    
        }
        // continue moving to the random direction along some path
        // if near a wall or other obstacle, calculate new reflection direction 
        // raycast some distance ahead and do a sharp turn

        //animator.transform.position = Vector3.Lerp(animator.transform.position, targetDestination, Time.deltaTime * movementSpeed * 0.1f); // TODO: this looks very weird :D (brownian motion)
        Vector3 smoothedPosition = Vector3.SmoothDamp(animator.transform.position, targetDestination, ref positionVelocity, positionDampTime);
        animator.transform.position = smoothedPosition;
        animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        */

        // new approach
        // find a new target angle several frames and smooth interpolate between them (OPTIONAL)
        float rand;

        if (directionChangeTimer.finished)
        {
            Mighty.MightyTimersManager.Instance.RemoveTimer(directionChangeTimer);
            ResetTimer();

            rand = Random.Range(0, 360);
            targetRotation = Quaternion.Euler(animator.transform.rotation.x, rand, animator.transform.rotation.z);
        }
        else if (npc.Colliding()) // TODO: should we reset the timer on collisions?
        {
            // if just collided, find new position (very small deflection so that it looks like they are avoiding collisions?)
        } 
        else if (WallNear(animator))
        {
            Debug.Log("Wall Near!");
            // if near a wall or other obstacle, calculate new reflection direction 
            // raycast some distance ahead and do a sharp turn
        }

        // continue moving to the random direction along some path
        //animator.transform.position += animator.transform.forward * Time.deltaTime * movementSpeed;
        Vector3 movementDirection = new Vector3(animator.transform.forward.x, 0f, animator.transform.forward.z) * movementSpeed;
        float yVel = rb.velocity.y;
        rb.velocity = new Vector3(movementDirection.x, yVel, movementDirection.z);
        animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        DebugExtension.DebugArrow(animator.transform.position, animator.transform.forward * 3, Color.red);
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

    private bool WallNear(Animator animator)
    {
        return Physics.Raycast(animator.transform.position, animator.transform.forward, raycastDistance);
    }

    private void ResetTimer()
    {
        directionChangeTimer = Mighty.MightyTimersManager.Instance.CreateTimer("DirectionChangeTimer", nextDirectionChangeDelta, 1f, false, true); // Create new timer (Not looping, stopped on start)
        directionChangeTimer.RestartTimer();
        directionChangeTimer.PlayTimer();
    }
}
