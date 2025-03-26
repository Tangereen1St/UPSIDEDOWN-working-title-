using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [Header("References")]
    public Rigidbody rb;
    public Transform head;
    public Camera playerCamera;



    [Header("Configurations")]
    public float walkSpeed = 6f;    // Default walking speed
    public float runSpeed = 12f;    // Default sprinting speed
    public float jumpSpeed = 5f;



    [Header("Runtime")]
    Vector3 newVelocity;
    Vector3 moveDirection;
    bool isGrounded = false;
    bool isJumping = false;



    // Start is called before the first frame update
    void Start() {
        //  Hide and lock the mouse cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }




    // Update is called once per frame
    void Update() {
        // Horizontal rotation
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * 2f);

        // Get WASD input
        moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        
        // Calculate movement speed (sprint with shift)
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        
        // Only allow movement when grounded
        Vector3 moveVector;
        if (isGrounded) {
            // Convert movement direction to world space when grounded
            moveVector = transform.TransformDirection(moveDirection) * currentSpeed;
        } else {
            // Keep horizontal velocity when in air
            moveVector = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }
        
        // Preserve vertical velocity (gravity/jumping)
        newVelocity = new Vector3(moveVector.x, rb.linearVelocity.y, moveVector.z);

        // Handle jumping
        if (isGrounded) {
            if (Input.GetKeyDown(KeyCode.Space) && !isJumping) {
                newVelocity.y = jumpSpeed;
                isJumping = true;
            }
        }

        // Apply movement
        rb.linearVelocity = newVelocity;
    }

    void FixedUpdate() {
        //  Shoot a ray of 1 unit towards the ground
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f)) {
            isGrounded = true;
        }
        else isGrounded = false;
    }

    void LateUpdate() {
        // Vertical rotation
        Vector3 e = head.eulerAngles;
        e.x -= Input.GetAxis("Mouse Y") * 2f;   //  Edit the multiplier to adjust the rotate speed
        e.x = RestrictAngle(e.x, -85f, 85f);    //  This is clamped to 85 degrees. You may edit this.
        head.eulerAngles = e;
    }




    //  This will be called constantly
    void OnCollisionStay(Collision col) {
        if (Vector3.Dot(col.GetContact(0).normal, Vector3.up) <= .6f)
            return;

        isGrounded = true;
        isJumping = false;
    }


    void OnCollisionExit(Collision col) {
        isGrounded = false;
    }




    //  A helper function
    //  Clamp the vertical head rotation (prevent bending backwards)
    public static float RestrictAngle(float angle, float angleMin, float angleMax) {
        if (angle > 180)
            angle -= 360;
        else if (angle < -180)
            angle += 360;

        if (angle > angleMax)
            angle = angleMax;
        if (angle < angleMin)
            angle = angleMin;

        return angle;
    }
}