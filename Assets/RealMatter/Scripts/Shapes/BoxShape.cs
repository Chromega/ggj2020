using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Soft Body Physics/Shapes/Box")]
public class BoxShape : BodyShape {
    //What are the dimensions?
    public Vector3 dimensions = new Vector3(2,2,2);

    private int width, height, depth;

    public override List<Vector3> getLatticePoints()
    {
        List<Vector3> LatticePoints=new List<Vector3>();
        width = (int)dimensions[0];
        height = (int)dimensions[1];
        depth = (int)dimensions[2];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    LatticePoints.Add(new Vector3(x,y,z));
                }
            }
        }
        return LatticePoints;
    }

    public override Vector3 getCenter() {
        return new Vector3((width-1) / 2.0f, (height-1) / 2.0f, (depth-1) / 2.0f);
    }
}
