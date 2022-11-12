using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDying : MonoBehaviour
{
    private Mighty.MightyTimer dyingTimer;
    public float dyingTime = 1f;
    // Start is called before the first frame update
    void Start()
    {
        // play particles
        // schedule for deletion
        Utils.ResetTimer(out dyingTimer, "DyingTimer", dyingTime, dyingTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (dyingTimer.finished)
        {
            Destroy(gameObject);
        }
    }
}
