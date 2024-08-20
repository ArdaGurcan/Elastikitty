using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate() {
        transform.position += new Vector3(0.1f*Time.fixedDeltaTime, 1f * Time.fixedDeltaTime, 0);
    }
}
