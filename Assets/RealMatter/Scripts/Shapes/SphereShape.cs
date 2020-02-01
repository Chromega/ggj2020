using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[AddComponentMenu("Soft Body Physics/Shapes/Sphere")]
public class SphereShape : BodyShape
{
    //What are the dimensions?
    public int diameter = 3;

    public override List<Vector3> getLatticePoints()
    {
        List<Vector3> LatticePoints = new List<Vector3>();
        float radius = diameter / 2.0f;
        for (int x = 0; x < diameter; x++)
        {
            for (int y = 0; y < diameter; y++)
            {
                for (int z = 0; z < diameter; z++)
                {

                    if ( (x+.5f-radius)*(x+.5f-radius) + (y+.5f-radius) * (y+.5f-radius) + (z+.5f-radius) * (z+.5f-radius) < radius * radius)
                    {
                        LatticePoints.Add(new Vector3(x, y, z));
                    }
                }
            }
        }
        return LatticePoints;
    }

    public override Vector3 getCenter()
    {
        float radius = diameter / 2.0f;
        return new Vector3(radius-.5f,radius-.5f,radius-.5f);
    }
}