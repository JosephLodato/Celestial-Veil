using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalBody : MonoBehaviour
{

    public float mass;
    public float radius;
    public float surfaceGravity;
    private BodySimulation simFile;
    private float dT;
    public Vector3 initalVelocity;

    
    public Vector3 velocity;
    // Start is called before the first frame update
    void Start()
    {
        velocity = initalVelocity;
    }

    public void updateSelfVelocity(OrbitalBody[] allBodies, float dT, float gravityConst)
    {
        if(this.name == "Sun")
        {
            return;
        }
        Vector3 thisPosition = transform.position;
        for(int i = 0; i < allBodies.Length; i++)
        {
            if(allBodies[i] != this)
            {
                OrbitalBody otherBody = allBodies[i];
                Vector3 otherPosition = otherBody.transform.position;

                float sqrtMag = (otherPosition - thisPosition).sqrMagnitude;
                Vector3 forceDirection = (otherPosition - thisPosition).normalized;

                float Force = gravityConst * mass * otherBody.mass / sqrtMag;
                Vector3 accel = forceDirection * Force / mass;

                velocity += accel * dT;
            }
        }

    }

    public void updatePos(float dT)
    {
        if(this.name == "Sun")
        {
            return;
        }
        transform.position += velocity * dT;
    } 
}
