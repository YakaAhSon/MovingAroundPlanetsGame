using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class AircraftController : MonoBehaviour
{
    [UnityEngine.SerializeField]
    private Transform m_TopBlade;
    [UnityEngine.SerializeField]
    private Transform m_SideBlade;

    private Transform m_CameraKit = null;

    private PlanetController m_CurrentPlanet = null;

    [UnityEngine.SerializeField]
    private TouchInput m_TouchInput;

    private Quaternion m_Gradienter = Quaternion.identity;

    private bool m_TurnedOn = false;

    private Rigidbody m_Rigidbody;

    private void Start() {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Gradienter = transform.rotation;
    }
    private void Update() {
        ControlBlades();
    }

    void FixedUpdate() {
        // apply gravity as constant speed
        // gravity speed = 3m/s
        if (m_CurrentPlanet != null) {
            ApplyGravity();
        }
        // controls
        if (m_TurnedOn) {
            ControlAircraft();
        }
        else {
            m_Rigidbody.velocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
        }
        ControlGradienter();
        if (m_CameraKit != null) {
            m_CameraKit.rotation = m_Gradienter;
            m_CameraKit.position = transform.position;
        }
    }
    private float m_CurrentGravitySpeed = 3;
    private void ApplyGravity() {
        if (!m_TurnedOn) {
            float dstGravitySpeed = 3;
            m_CurrentGravitySpeed = Mathf.Lerp(m_CurrentGravitySpeed, dstGravitySpeed, Time.fixedDeltaTime * 10);
            transform.position = transform.position -
                    (transform.position - m_CurrentPlanet.transform.position).normalized * Time.fixedDeltaTime * m_CurrentGravitySpeed;
        }
    }

    struct CTRL_VARIABLES {
        public float yaw_angle;// 20 degrees at most 
        public float lean_angle;// only when near planet
        public float vertical_speed;

        public float gradienter_roll_anglularspeed;
        public float gradienter_yaw_angularspeed;
    }
    CTRL_VARIABLES ctrl;
    private void getCtrlVariables() {
        ctrl.yaw_angle = 0;
        ctrl.lean_angle = 0;
        ctrl.vertical_speed = 0;
        ctrl.gradienter_roll_anglularspeed = 0;
        ctrl.gradienter_yaw_angularspeed = 0;

        if (m_TouchInput.leftJoyStick.touched) {
            ctrl.yaw_angle = m_TouchInput.leftJoyStick.value.y / 150 * 20;
            ctrl.lean_angle = -m_TouchInput.leftJoyStick.value.x / 150 * 20;
            ctrl.yaw_angle = Mathf.Clamp(ctrl.yaw_angle, -10, 20);
            ctrl.lean_angle = Mathf.Clamp(ctrl.lean_angle, -20, 20);
        }

        if (m_CurrentPlanet != null && m_TouchInput.yawSlide.touched) {
            ctrl.vertical_speed = m_TouchInput.yawSlide.value / 150 * 5;
            ctrl.vertical_speed = Mathf.Clamp(ctrl.vertical_speed, -6, 10);
        }

        if(m_CurrentPlanet==null && m_TouchInput.aircraftRoll.touched) {
            ctrl.gradienter_roll_anglularspeed = m_TouchInput.aircraftRoll.value < 0 ? 360 /10 : -360 / 10;
        }

        if (m_CurrentPlanet == null && m_TouchInput.yawSlide.touched) {
            ctrl.gradienter_yaw_angularspeed = - m_TouchInput.yawSlide.value / 150 * 360 / 10;
            ctrl.gradienter_yaw_angularspeed = Mathf.Clamp(ctrl.gradienter_yaw_angularspeed, -360 / 10, 360 * 10);
        }
    }

    private float yawBackToGradienter() {
        // get last yaw angle as the angle between two forwards
        float last_yaw_angle = Mathf.Asin(Vector3.Dot(
            transform.right,
            Vector3.Cross(m_Gradienter*Vector3.forward, transform.forward)
            )) * 180/Mathf.PI;
        // transform back yaw angle by rotating along rest right axis
        transform.Rotate(m_Gradienter*Vector3.right, -last_yaw_angle);
        return last_yaw_angle;
    }

    private float leanBackToGradienter() {
        // get last lean angle as the angle between two rights
        float last_lean_angle = Mathf.Asin(Vector3.Dot(
            transform.forward,
            Vector3.Cross(transform.right, m_Gradienter*Vector3.right)
            ))*180/Mathf.PI;
        transform.Rotate(m_Gradienter*Vector3.forward, -last_lean_angle);
        return last_lean_angle;
    }

    private void yawToAngle(float yaw_angle) {
        transform.Rotate(Vector3.right, yaw_angle);
    }
    private void leanToAngle(float lean_angle) {
        transform.Rotate(Vector3.forward, lean_angle);
    }

    private void ControlAircraft() {
        m_Rigidbody.angularVelocity = Vector3.zero;
        m_Rigidbody.velocity = Vector3.zero;

        getCtrlVariables();

        float last_lean_angle = leanBackToGradienter();
        float last_yaw_angle = yawBackToGradienter();

        // copy the rotation to avoid numericle error
        transform.rotation = m_Gradienter;

        float current_yaw_angle = Mathf.Lerp(last_yaw_angle, ctrl.yaw_angle, Time.fixedDeltaTime*5 );
        float current_lean_angle = Mathf.Lerp(last_lean_angle, ctrl.lean_angle, Time.fixedDeltaTime*5 );
        //current_lean_angle = ctrl.lean_angle;
        //current_yaw_angle = ctrl.yaw_angle;

        Vector3 dst_velocity_wrt_gradienter = new Vector3(
            0,// never has speed in x
            ctrl.vertical_speed,
            current_yaw_angle / 20 * 10
            );
        Vector3 dst_velocity = m_Gradienter * dst_velocity_wrt_gradienter;

        Vector3 delta_position = dst_velocity * Time.fixedDeltaTime;
        if (m_CurrentPlanet) {
            delta_position = m_CurrentPlanet.transform.InverseTransformDirection(delta_position);
        }
        transform.localPosition = transform.localPosition + delta_position;

        // turning
        float turnAngularSpeed = -ctrl.lean_angle / 20 * (360 / 10);

        transform.Rotate(Vector3.up, turnAngularSpeed * Time.fixedDeltaTime);
        m_Gradienter = transform.rotation;
        m_Gradienter.Normalize();
        yawToAngle(current_yaw_angle);
        leanToAngle(current_lean_angle);
    }

    void ControlGradienter() {
        if (m_CurrentPlanet != null) {
            // stand up
            Vector3 dst_up = (transform.position - m_CurrentPlanet.transform.position).normalized;
            Vector3 dst_forward = Vector3.Cross(m_Gradienter*Vector3.right, dst_up);
            if (Mathf.Abs(dst_forward.x) < 1e-5 && Mathf.Abs(dst_forward.y) < 1e-5 && Mathf.Abs(dst_forward.z) < 1e-5) {
                Vector3 tmp_right = Vector3.Cross(dst_up, m_Gradienter * Vector3.forward);
                dst_forward = Vector3.Cross(tmp_right, dst_up);
            }
            Quaternion dst_rotation = Quaternion.LookRotation(dst_forward, dst_up);
            m_Gradienter = Quaternion.Slerp(m_Gradienter, dst_rotation, Time.fixedDeltaTime * 10);
        }
        else {
            // control with rolling and yawling
            m_Gradienter = m_Gradienter
                * Quaternion.AngleAxis(ctrl.gradienter_roll_anglularspeed * Time.fixedDeltaTime, Vector3.forward)
                * Quaternion.AngleAxis(ctrl.gradienter_yaw_angularspeed * Time.fixedDeltaTime, Vector3.right);
        }
    }
    
    public void TurnOn() {
        m_TurnedOn = true;
    }
    public void TurnOff() {
        m_TurnedOn = false;
    }

    public void SetCamera(Transform camera_kit) {
        m_CameraKit = camera_kit;
    }

    public void SetCurrentPlanet(PlanetController cp) {
        m_CurrentPlanet = cp;
        if (m_CurrentPlanet != null) {
            transform.position = new Vector3(60, 60, 0) + m_CurrentPlanet.transform.position;
            transform.SetParent(cp.transform, true);
        }
        else {
            transform.SetParent(null, true);
        }
    }

    private float m_CurrentTopBladeAngle = 0;
    private float m_CurrentTopBladeRPS = 0;
    private float m_CurrentSideBladeAngle = 0;
    private float m_CurrentSideBladeRPS = 0;

    private void ControlBlades() {

    }
}

