using Mighty;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostEffects : MonoBehaviour
{
    //float rot;
    //public float speed;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TransformJuicer>().StartJuicing();
    }

    // Update is called once per frame
    void Update()
    {
        //rot += speed * Time.deltaTime;
        //transform.eulerAngles = new Vector3(0, rot, 0);
    }
}
