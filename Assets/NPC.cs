using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public bool isPosessed = false;

    private int nextStateChangeDelta = 0;
    public int minStateChangeTime = 200;
    public int maxStateChangeTime = 600;

    private Mighty.MightyTimer directionChangeTimer;

    private bool collided = false;
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        //Utils.ResetTimer();
    }

    // Update is called once per frame
    void Update() // randomly trigger states in the animator after set time elapsed
    {
        if (isPosessed)
        {
            animator.SetBool("isIDLE", true);  // isIDLE
        }
        // overarching state machine - changes state every N seconds
        
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
