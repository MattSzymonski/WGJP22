using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrollingBehaviour : StateMachineBehaviour
{
    public float movementSpeed = 0.01f;
    public float targetDistance = 1f;
    public float rotationSpeed = 0.1f;
    public float raycastDistance = 5f;
    public float wallCollisionDeflectionAngle = 30f;
    public float collisionDeflectonAngle = 15f;

    public float positionDampTime = 0.12f;

    public int directionChangeTimeMin = 30;
    public int directionChangeTimeMax = 100;

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
        animator.transform.rotation = Quaternion.Euler(animator.transform.rotation.x, Random.Range(0, 360), animator.transform.rotation.z);
        if (!npc.isPosessed)
        {
            targetDestination = animator.transform.position;
            Utils.ResetTimer(out directionChangeTimer, directionChangeTimeMin, directionChangeTimeMax);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (npc.isPosessed)
            return;

        // new approach
        // find a new target angle several frames and smooth interpolate between them (OPTIONAL) TODO:
        float rand;
        RaycastHit outRayHit;

        if (directionChangeTimer.finished)
        {
            Mighty.MightyTimersManager.Instance.RemoveTimer(directionChangeTimer);
            Utils.ResetTimer(out directionChangeTimer, directionChangeTimeMin, directionChangeTimeMax);
            rand = Random.Range(0, 360);
            targetRotation = Quaternion.Euler(animator.transform.rotation.x, rand, animator.transform.rotation.z);
        }
        else if (npc.Colliding()) // TODO: should we reset the timer on collisions? // TODO: FIX!
        {
            Debug.Log("NPC near");
            // if just collided, find new position (very small deflection so that it looks like they are avoiding collisions?)
            //float rotAngle = Random.Range(-collisionDeflectonAngle, collisionDeflectonAngle);
            //Debug.Log(rotAngle);
            //targetRotation = Quaternion.AngleAxis(rotAngle, Vector3.up);
            //Debug.Log(targetRotation.eulerAngles);
        } 
        else if (WallNear(animator, out outRayHit))
        {
            Debug.Log("Wall Near!");
            // if near a wall or other obstacle, calculate new reflection direction 
            //float angle = animator.transform.rotation.eulerAngles.y;
            Vector3 reflectVec = Vector3.Reflect(animator.transform.forward, outRayHit.normal);
            // rotate the vector in a -30,30 degree fan
            float rotAngle = Random.Range(-wallCollisionDeflectionAngle, wallCollisionDeflectionAngle);
            Vector3 rotated = Quaternion.AngleAxis(rotAngle, Vector3.up) * reflectVec;

            float angle = Vector3.Angle(Vector3.forward, rotated);

            targetRotation = Quaternion.AngleAxis(angle, Vector3.up);
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
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb.velocity = Vector3.zero;
        Debug.Log("Exit STROLLING");
    }

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

    private bool WallNear(Animator animator, out RaycastHit outRayHit)
    {
        bool hit = Physics.Raycast(animator.transform.position, animator.transform.forward, out outRayHit, raycastDistance);
        if (hit)
        {
            return !IsNPC(outRayHit.transform);
        }
        return false;
    }

    private bool IsNPC(Transform trans)
    {
        return trans.tag.Contains("Player") || trans.tag.Contains("NPC");
    }
}
