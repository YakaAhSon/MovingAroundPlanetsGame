using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class CubeBehaviour : MonoBehaviour
{
    public GameObject m_AlwaysStandUpObject;
    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 currentVelocity = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        CharacterController controller = GetComponent<CharacterController>();
        Vector3 localMoveDirection = m_AlwaysStandUpObject.transform.InverseTransformDirection(moveDirection);
        localMoveDirection.x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        localMoveDirection.z = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        if (Input.GetButton("Jump"))
            localMoveDirection.y += jumpSpeed * Time.deltaTime;

        localMoveDirection.y -= gravity * Time.deltaTime * Time.deltaTime;

        Vector3 origPosition = m_AlwaysStandUpObject.transform.position;
        Vector3 globalMoveDirection = m_AlwaysStandUpObject.transform.TransformDirection(localMoveDirection);
        controller.Move(globalMoveDirection);
        // standup
        Vector3 local_y = m_AlwaysStandUpObject.transform.TransformDirection(new Vector3(0, 1, 0));
        Vector3 dst = m_AlwaysStandUpObject.transform.position.normalized;
        Vector3 y_delta = dst - local_y;

        float y_delta_mag = y_delta.magnitude/2;
        float rad = Mathf.Asin(y_delta_mag);
        Debug.Log(rad*180/Mathf.PI);
        if (rad > 1e-5) {
            
            Vector3 RotAxis = Vector3.Cross(local_y, dst);
            RotAxis.Normalize();
            m_AlwaysStandUpObject.transform.Rotate(RotAxis, rad * 180 / Mathf.PI);
        }
        moveDirection = m_AlwaysStandUpObject.transform.position - origPosition;
        */
    }
}
