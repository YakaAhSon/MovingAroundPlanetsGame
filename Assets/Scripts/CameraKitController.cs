using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class CameraKitController : MonoBehaviour
{
    [UnityEngine.SerializeField]
    private TouchInput m_TouchInput;
    [UnityEngine.SerializeField]
    private Transform m_HeadingAxis;
    [UnityEngine.SerializeField]
    private Transform m_VirtualPole;
    [UnityEngine.SerializeField]
    private GameObject m_Camera;

    private float m_CurrentYaw = 0;
    private float m_CurrentHeading = 0;

    private bool m_AutoResumeEnabled = false;

    // Start is called before the first frame update
    void Start()
    {
        SetYawAngle(m_CurrentYaw);
        SetHeadingAngle(m_CurrentHeading);
    }

    public void SetHeadingAngle(float ha) {
        m_CurrentHeading = ha;
        m_HeadingAxis.localRotation = Quaternion.identity;
        m_HeadingAxis.Rotate(Vector3.up, m_CurrentHeading);
    }

    public void SetYawAngle(float ya) {
        ya = Mathf.Clamp(ya, -60, 60);
        m_CurrentYaw = ya;
        m_VirtualPole.localRotation = Quaternion.identity;
        m_VirtualPole.Rotate(Vector3.right, -m_CurrentYaw);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        // auto resume
        if (m_AutoResumeEnabled && !m_TouchInput.rightJoyStick.touched ) {
            if (m_CurrentYaw >-20) {
                m_CurrentYaw -= 5;
                m_CurrentYaw = Mathf.Max(-20, m_CurrentYaw);
            }
            if (m_CurrentYaw < -20) {
                m_CurrentYaw += 5;
                m_CurrentYaw = Mathf.Min(-20, m_CurrentYaw);
            }
            if (m_CurrentHeading > 0) {
                m_CurrentHeading -= 5;
                m_CurrentHeading = Mathf.Max(0, m_CurrentHeading);
            }
            if (m_CurrentHeading < 0) {
                m_CurrentHeading += 5;
                m_CurrentHeading = Mathf.Min(0, m_CurrentHeading);
            }
        }
        // handle heading
        if (m_TouchInput.rightJoyStick.touched) {
            m_CurrentHeading += m_TouchInput.rightJoyStick.delta.x / 100 * 30;
        }
        SetHeadingAngle(m_CurrentHeading);
        // collect control variables
        if (m_TouchInput.rightJoyStick.touched) {
            m_CurrentYaw  += m_TouchInput.rightJoyStick.delta.y / 100*30;
        }
        SetYawAngle(m_CurrentYaw);

        // handle yaw
        m_VirtualPole.transform.localPosition = new Vector3(0, 1.5f, 0);
        
        // handle distance
        Ray ray = new Ray(m_VirtualPole.transform.position, -m_VirtualPole.transform.forward);
        int layerMask = (1 << 9) | (1 << 10);
        layerMask = ~layerMask;
        RaycastHit hit;
        float current_distance = 10;
        if (Physics.Raycast(ray, out hit, 10, layerMask)) {
            current_distance = hit.distance-0.5f;
        }

        current_distance = Mathf.Clamp(current_distance, 1, 10);

        Vector3 v = m_Camera.transform.localPosition;
        //Debug.Log(current_distance);
        v.x = 0;
        v.y = 0;
        v.z = -current_distance;
        m_Camera.transform.localPosition = v;
    }

    public void SetExtraHeadingRotation(float angle) {
        transform.Rotate(Vector3.up, angle);
    }

    public Vector3 GetCameraHorizontalForward() {
        return m_HeadingAxis.forward;
    }

    public void ClearHeading_fixedCameraDirection() {
        transform.Rotate(Vector3.up, m_CurrentHeading);
        SetHeadingAngle(0);
    }

    public void EnableAutoResume() {
        m_AutoResumeEnabled = true;
    }
    public void DisableAutoResume() {
        m_AutoResumeEnabled = false;
    }
}
