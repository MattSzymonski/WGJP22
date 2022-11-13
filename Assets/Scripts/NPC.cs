using Mighty;
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


    public TransformJuicer tjPosition;
    public TransformJuicer tjScale;
    public bool inStop = false;

    SkinnedMeshRenderer red;
    ValueStorage npc;




    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        Utils.ResetTimer(out stateChangeTimer, "StateChangeTimer", minStateChangeTime, maxStateChangeTime);

        red = transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
        npc = transform.GetChild(0).GetComponent<ValueStorage>();

        TransformJuicer[] juicers = transform.GetComponentsInChildren<TransformJuicer>();
        tjPosition = juicers[0];
        tjScale = juicers[1];
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
            Mighty.MightyTimersManager.Instance.RemoveTimer(stateChangeTimer);
            Utils.ResetTimer(out stateChangeTimer, "StateChangeTimer", minStateChangeTime, maxStateChangeTime);
            // assign probabilities?
            int thresh = Random.Range(0, 100);

            if (thresh < movementThreshold || inStop) // don't entr stop several times
            {
                animator.SetBool(Utils.IDLESTRING, false);
                animator.SetBool(Utils.STOPSTRING, false);
                inStop = false;
            } else if (thresh < stopThreshold)
            {
                animator.SetBool(Utils.IDLESTRING, false);
                animator.SetBool(Utils.STOPSTRING, true);
                inStop = true;
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
