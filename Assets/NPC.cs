using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public bool isPosessed = false;

    private int nextStateChangeDelta = 0;
    public int minStateChangeTime = 200;
    public int maxStateChangeTime = 600;

    //public int 

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
            animator.SetBool("isIDLE", true);  // isIDLE
            return;
        }
        // overarching state machine - changes state every N seconds
        if (stateChangeTimer.finished)
        {
            // assign probabilities?

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
