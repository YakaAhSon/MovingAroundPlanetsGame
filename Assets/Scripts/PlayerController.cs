using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

    [UnityEngine.SerializeField]
    public TouchInput m_TouchInput;
    [UnityEngine.SerializeField]
    public CameraKitController m_CameraKit;// always agrees transform.up and camera.left

    [UnityEngine.SerializeField]
    public AircraftController m_Aircraft;

    private PlanetController m_CurrentPlanet;

    private bool m_Pilotting = false;
    private bool m_AutoNavigating = false;
    private Vector3 m_ANDestinationOnCurrentPlanet;

    public void Initialize(GameObject character) {
        m_animator = character.GetComponent<Animator>();
        m_rigidBody = character.GetComponent<Rigidbody>();
    }

    [SerializeField] private float m_moveSpeed = 2;
    [SerializeField] private float m_turnSpeed = 200;
    [SerializeField] private float m_jumpForce = 4;

    [SerializeField] private Animator m_animator;
    [SerializeField] private Rigidbody m_rigidBody;

    private float m_currentV = 0;
    private float m_currentH = 0;

    private readonly float m_interpolation = 10;
    private readonly float m_walkScale = 0.33f;
    private readonly float m_backwardsWalkScale = 0.16f;
    private readonly float m_backwardRunScale = 0.66f;

    private bool m_wasGrounded;
    private Vector3 m_currentDirection = Vector3.zero;

    private float m_jumpTimeStamp = 0;
    private float m_minJumpInterval = 0.25f;

    private bool m_isGrounded;

    private List<Collider> m_collisions = new List<Collider>();

    void Awake() {

        if (!m_animator) { gameObject.GetComponent<Animator>(); }
        if (!m_rigidBody) { gameObject.GetComponent<Rigidbody>(); }
    }

    private void OnCollisionEnter(Collision collision) {
        ContactPoint[] contactPoints = collision.contacts;
        for (int i = 0; i < contactPoints.Length; i++) {
            if (Vector3.Dot(contactPoints[i].normal, transform.up) > 0.5f) {
                if (!m_collisions.Contains(collision.collider)) {
                    m_collisions.Add(collision.collider);
                }
                m_isGrounded = true;
            }
        }
    }

    private void OnCollisionStay(Collision collision) {
        ContactPoint[] contactPoints = collision.contacts;
        bool validSurfaceNormal = false;
        for (int i = 0; i < contactPoints.Length; i++) {
            if (Vector3.Dot(contactPoints[i].normal, transform.up) > 0.5f) {
                validSurfaceNormal = true; break;
            }
        }

        if (validSurfaceNormal) {
            m_isGrounded = true;
            if (!m_collisions.Contains(collision.collider)) {
                m_collisions.Add(collision.collider);
            }
        }
        else {
            if (m_collisions.Contains(collision.collider)) {
                m_collisions.Remove(collision.collider);
            }
            if (m_collisions.Count == 0) { m_isGrounded = false; }
        }
    }

    private void OnCollisionExit(Collision collision) {
        if (m_collisions.Contains(collision.collider)) {
            m_collisions.Remove(collision.collider);
        }
        if (m_collisions.Count == 0) { m_isGrounded = false; }
    }

    void FixedUpdate() {


        if (!m_Pilotting) {
            // must be in an planet
            GetComponent<Rigidbody>().AddForce((-transform.position + m_CurrentPlanet.transform.position).normalized * 9, ForceMode.Acceleration);
            UpdateWalking();
        }
        else {
            transform.localPosition = new Vector3(0, 0.4f, 0f);
        }
        m_animator.SetBool("Grounded", m_isGrounded);
        m_wasGrounded = m_isGrounded;
    }


    private void UpdateWalking() {

        m_CameraKit.ClearHeading_fixedCameraDirection();
        
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        if (m_TouchInput.autoNavigate.touched) {
            UpdateAutoNavigate(m_TouchInput.autoNavigate.value);
        }

        if (m_TouchInput.leftJoyStick.touched) {
            h = m_TouchInput.leftJoyStick.value.x / 150;
            v = m_TouchInput.leftJoyStick.value.y / 150;
            m_AutoNavigating = false;
        }

        if (m_AutoNavigating) {
            Vector3 dst = m_CurrentPlanet.transform.TransformPoint(m_ANDestinationOnCurrentPlanet);
            if ((dst - transform.position).magnitude > 0.3) {
                Vector3 delta = dst - transform.position;
                delta.Normalize();

                delta = m_CameraKit.transform.InverseTransformDirection(delta);
                v = delta.z;
                h = delta.x;
            }
            else {
                m_AutoNavigating = false;
            }
        }

        m_currentV = Mathf.Lerp(m_currentV, v, Time.fixedDeltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.fixedDeltaTime * m_interpolation);

        Vector3 direction = m_CameraKit.transform.forward * m_currentV + m_CameraKit.transform.right * m_currentH;

        if (direction.magnitude > 1e-5) {
            transform.rotation = Quaternion.LookRotation(direction.normalized, transform.up);
        }

        float directionLength = direction.magnitude;
        //direction.y = 0;
        direction = direction.normalized * directionLength;

        if (direction != Vector3.zero) {
            m_currentDirection = Vector3.Slerp(m_currentDirection, direction, Time.deltaTime * m_interpolation);

            //transform.rotation = Quaternion.LookRotation(m_currentDirection,transform.up);
            transform.position += m_currentDirection * m_moveSpeed * Time.deltaTime;

            m_animator.SetFloat("MoveSpeed", direction.magnitude);
        }

        JumpingAndLanding();
        StandUp();
        m_CameraKit.transform.position = transform.position;
        m_CameraKit.transform.rotation = Quaternion.FromToRotation(m_CameraKit.transform.up, transform.up) * m_CameraKit.transform.rotation;
    }

    private void JumpingAndLanding() {
        bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

        if (jumpCooldownOver && m_isGrounded && Input.GetKey(KeyCode.Space)) {
            m_jumpTimeStamp = Time.time;
            m_rigidBody.AddForce(transform.up * m_jumpForce, ForceMode.Impulse);
        }

        if (!m_wasGrounded && m_isGrounded) {
            m_animator.SetTrigger("Land");
        }

        if (!m_isGrounded && m_wasGrounded) {
            m_animator.SetTrigger("Jump");
        }
    }

    private void StandUp() {
        Vector3 lookAt = Vector3.Cross(transform.right, transform.position - m_CurrentPlanet.transform.position);
        transform.rotation = Quaternion.LookRotation(lookAt, transform.position - m_CurrentPlanet.transform.position);
    }

    public void SetCurrentPlanet(PlanetController planet) {
        m_CurrentPlanet = planet;
        if (planet != null) {
            transform.SetParent(planet.transform, true);
        }
        else {
            transform.SetParent(null,true);
        }
        transform.localPosition = new Vector3(60, 60, 0);
    }

    public void GetInAirCraft() {
        if (m_Pilotting) {
            return;
        }
        m_Pilotting = true;
        GetComponent<Rigidbody>().isKinematic = true;
        m_CurrentPlanet = null;
        transform.SetParent(m_Aircraft.transform, true);
        transform.localPosition = new Vector3(0, 0.4f, 0f);
        transform.localRotation = Quaternion.identity;

    }
    public void GetOutAirCraft() {
        if (!m_Pilotting) {
            return;
        }

        GetComponent<Rigidbody>().isKinematic = false;
        m_Pilotting = false;
        transform.localPosition = new Vector3(0, 60f, 60f);
        transform.SetParent(m_CurrentPlanet.transform, true);
    }

    private void UpdateAutoNavigate(Vector2 screenPos) {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 64)) {
            m_AutoNavigating = true;
            // refresh av dst
            m_ANDestinationOnCurrentPlanet = m_CurrentPlanet.transform.InverseTransformPoint(hit.point);
        }
    }
}
