using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Rendering;

public class Ship : MonoBehaviour {

    public Transform seatPosition;

    public float orbitalInfluenceBoundary = 200f;
    public float flySpeed = 8;
    public float boostSpeed = 14;
    public float jumpForce = 20;
    public float smoothTime = 0.5f;

    public bool lockCursor;
    public float mass = 70;
    Rigidbody rb;

    public float mouseSensitivity = 10;
    public Vector2 pitchMinMax = new Vector2 (-40, 85);

    public float yaw;
    public float pitch;
    public float roll;


    public Vector3 targetVelocity;
    Vector3 smoothVelocity;
    Vector3 smoothVRef;

    GameObject referenceBody;

    Camera cam;
    PlayerController playerController;

    public BodySimulation bodySimulation;
    private float gravityConst;
    private float dT;

    public Transform landingCollider;

    

    void Awake () {
        dT = FindObjectOfType<BodySimulation>().dT;

        playerController = FindObjectOfType<PlayerController>();

        cam = GetComponentInChildren<Camera> ();

        InitRigidbody ();

        if (lockCursor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        gravityConst = bodySimulation.gravityConst;
    }

    void InitRigidbody () {
        rb = GetComponent<Rigidbody> ();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.mass = mass;
    }

    void Update () {
        if(playerController.flyingShip) HandleMovement ();
        
        
    }

    void HandleMovement () {

        // Pitch / roll Look input
        yaw = Input.GetAxisRaw ("Mouse X") * mouseSensitivity;
        pitch = Input.GetAxisRaw ("Mouse Y") * mouseSensitivity;
        roll = 0;
     
        roll = Input.GetKey(KeyCode.Q) ? (Input.GetAxis("Mouse X") * mouseSensitivity) : roll = 0;
        yaw = (roll == 0) ? yaw : 0;


        // these are split to make the ship not rotate on z as well 
        // The pitch must be relative to self, but yaw is relative to world
        transform.Rotate(new Vector3(pitch, 0, 0), Space.Self);
        transform.Rotate(new Vector3(0, yaw, roll), Space.World);


        // Movement
        Vector3 input = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0 , Input.GetAxisRaw ("Vertical"));
        float currentSpeed = Input.GetKey (KeyCode.LeftShift) ? boostSpeed : flySpeed;
        targetVelocity = transform.TransformDirection (input.normalized) * currentSpeed;
        smoothVelocity = Vector3.SmoothDamp (smoothVelocity, targetVelocity, ref smoothVRef,  smoothTime);

        rb.AddRelativeForce(new Vector3 (0, Input.GetAxisRaw("VertFlying"), 0)  * jumpForce, ForceMode.Impulse);
        //Debug.Log(new Vector3 (0, Input.GetAxisRaw("VertFlying"), 0)  * jumpForce);
        
        // if (Input.GetKeyDown (KeyCode.Space)) {
        //     rb.AddForce (transform.up * jumpForce, ForceMode.VelocityChange);
        // }
        // if (Input.GetKeyDown (KeyCode.LeftControl)) {
        //    rb.AddForce (-transform.up * jumpForce, ForceMode.VelocityChange);
        // }
    }

   
    void FixedUpdate () {
        if(transform.root){
            OrbitalBody[] bodies = FindObjectsOfType<OrbitalBody> ();
            Vector3 strongestGravitationalPull = Vector3.zero;

            // Gravity
            Vector3 thisPosition = transform.position;
            for(int i = 0; i < bodies.Length; i++)
            {
                OrbitalBody otherBody = bodies[i];
                Vector3 otherPosition = otherBody.transform.position;

                float sqrtMag = (otherPosition - thisPosition).sqrMagnitude;
                Vector3 forceDirection = (otherPosition - thisPosition).normalized;

                float Force = gravityConst * mass * otherBody.mass / sqrtMag;
                Vector3 accel = forceDirection * Force / mass;

                rb.velocity += accel * dT;
                
                if (accel.sqrMagnitude > strongestGravitationalPull.sqrMagnitude) {
                    strongestGravitationalPull = accel;
                    referenceBody = bodies[i].gameObject;
                }
            }

                // Find body with strongest gravitational pull 

            float dist = (referenceBody.transform.position - rb.position).magnitude;

            orbitalInfluenceBoundary = referenceBody.transform.localScale.x + 100;
            // Rotate to align with gravity up || IF DISTANCE IS PAST A THRESHOLD, DO NOT DO THIS
            if(dist < orbitalInfluenceBoundary)
            {
                Vector3 gravityUp = -strongestGravitationalPull.normalized;
                rb.rotation = Quaternion.FromToRotation (transform.up, gravityUp) * rb.rotation;
            }
            // Move
            rb.MovePosition (rb.position + smoothVelocity * Time.fixedDeltaTime);
        }
        Debug.Log(IsGrounded());
        if(IsGrounded()) transform.SetParent(referenceBody.transform, false);
        if(!IsGrounded()) transform.SetParent(null, false);
    }

    public void SetVelocity (Vector3 velocity) {
        rb.velocity = velocity;
    }

    bool IsGrounded () {
        // // Sphere must not overlay terrain at origin otherwise no collision will be detected
        // // so rayRadius should not be larger than controller's capsule collider radius
        const float rayRadius = .3f;
        bool grounded = false;

        if (referenceBody) {
            var relativeVelocity = rb.velocity - (referenceBody.GetComponent<Rigidbody>().velocity);
            // Don't cast ray down if player is jumping up from surface
                
            Vector3 offsetToFeet = (landingCollider.position - transform.position);
            Vector3 rayOrigin = rb.position + offsetToFeet + transform.up * rayRadius;
            Vector3 rayDir = -transform.up;

            grounded = Physics.Raycast(transform.position, rayDir, Vector3.Distance(transform.position, landingCollider.position) + 2);
            Debug.DrawRay(transform.position, rayDir, Color.red);
        }
        
        return grounded;
    }

    public Rigidbody Rigidbody {
        get {
            return rb;
        }
    }

}