using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class BodySimulation : MonoBehaviour
{
    public float dT;
    public float gravityConst;
    OrbitalBody[] bodies;
    // Start is called before the first frame update
    void Start()
    {
        bodies = FindObjectsOfType<OrbitalBody>();
    }


    private void FixedUpdate()
    {
        for(int i = 0; i < bodies.Length; i++)
        {
            bodies[i].updatePos(dT/2);
        }
        
        for(int i = 0; i < bodies.Length; i++)
        {
            bodies[i].updateSelfVelocity(bodies, dT, gravityConst);
        }
        
        for(int i = 0; i < bodies.Length; i++)
        {
            bodies[i].updatePos(dT/2);
        }


    }


    public Vector3 CalculateAcceleration (Vector3 point, OrbitalBody ignoreBody = null) {
        Vector3 acceleration = Vector3.zero;
        foreach (var body in bodies) {
            if (body != ignoreBody) {
                float sqrDst = (body.transform.position - point).sqrMagnitude;
                Vector3 forceDir = (body.transform.position - point).normalized;
                acceleration += forceDir * gravityConst * body.mass / sqrDst;
            }
        }

        return acceleration;
    }

}
