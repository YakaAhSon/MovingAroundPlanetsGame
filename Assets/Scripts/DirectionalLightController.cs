using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalLightController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = Vector3.zero;
        transform.rotation = Quaternion.LookRotation(transform.position);
    }
}
