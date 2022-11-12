using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public bool isPosessed = false;

    public int minStateChangeTime = 10;
    public int maxStateChangeTime = 30;

    // probability thresholds
    public int movementThreshold = 50; 
    public int stopThreshold = 100;

    private Mighty.MightyTimer stateChangeTimer;

    private bool collided = false;
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        Utils.ResetTimer(out stateChangeTimer, minStateChangeTime, maxStateChangeTime);
    }

    // Update is called once per frame
    void Update() // randomly trigger states in the animator after set time elapsed
    {
        if (isPosessed)
        {
            animator.SetBool(Utils.IDLESTRING, true);  // isIDLE
            return;
        }
        // overarching state machine - changes state every N seconds
        if (stateChangeTimer.finished)
        {
            // assign probabilities?
            int thresh = Random.Range(0, 100);

            if (thresh < movementThreshold)
            {
                animator.SetBool(Utils.IDLESTRING, false);
                animator.SetBool(Utils.STOPSTRING, false);
            } else if (thresh < stopThreshold)
            {
                animator.SetBool(Utils.IDLESTRING, false);
                animator.SetBool(Utils.STOPSTRING, true);
            }

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        collided = true; // TODO: what when we keep receiving collision info? introduce a cooldown?
    }
    private void OnCollisionExit(Collision collision)
    {
        collided = false;
    }

    public bool Colliding()
    {
        return collided;
    }
}
