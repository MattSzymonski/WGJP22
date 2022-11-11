using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    private bool collided = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        collided = true;
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
