using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleScript : MonoBehaviour
{

    public RMParticle particle;
    public Vector3 startPosition;
    public int numConnectedCells;
    public ATPoint3 index;

    public void Initialize(RMParticle p)
    {
        particle = p;
        transform.position = startPosition;
        Vector3 velocity = particle.v;
        index = p.index;
        if (!InfNan(velocity) && !GetComponent<Rigidbody>().isKinematic)
            GetComponent<Rigidbody>().velocity = velocity;
    }

    public void RefreshStartPosition() {
        startPosition = transform.position;
        transform.rotation = Quaternion.identity;
    }

    // Update is called once per physicsframe
    public void BeforeBodySim()
    {
        particle.x = GetComponent<Rigidbody>().position;
        particle.v = GetComponent<Rigidbody>().velocity;
    }

    public void AfterBodySim()
    {
        Vector3 velocity = particle.v;
        if (!InfNan(velocity) && !GetComponent<Rigidbody>().isKinematic)
            GetComponent<Rigidbody>().velocity = velocity;
    }

    bool InfNan(Vector3 velocity)
    {
        return ((float.IsInfinity(velocity.x) || float.IsInfinity(velocity.y) || float.IsInfinity(velocity.z)
            || float.IsNaN(velocity.x) || float.IsNaN(velocity.y) || float.IsNaN(velocity.z)));
    }

    public bool DetermineInMesh() {
        return (numConnectedCells == 8);
    }

}