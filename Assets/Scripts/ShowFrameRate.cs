using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowFrameRate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float frame_rate = 1f / Time.deltaTime;
        GetComponent<Text>().text = frame_rate.ToString();
    }
}
