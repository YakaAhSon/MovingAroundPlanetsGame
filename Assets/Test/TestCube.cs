using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCube : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        transform.position = transform.position - new Vector3(0, 1, 0) * Time.fixedDeltaTime;
        //transform.Rotate(new Vector3(1, 1, 0), Time.fixedDeltaTime * 360 / 3);
    }
}
