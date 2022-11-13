using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDying : MonoBehaviour
{
    SkinnedMeshRenderer red;
    ValueStorage npc;

    private Mighty.MightyTimer dyingTimer;
    public float dyingTime = 3f;
    // Start is called before the first frame update
    void Start()
    {
        red = transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
        npc = transform.GetChild(0).GetComponent<ValueStorage>();
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

        red.materials[0].SetFloat("_NoiseStrength", npc.dissolveStrength);
    }
}
