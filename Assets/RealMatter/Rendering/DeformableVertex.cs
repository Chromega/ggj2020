using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RMDeformableVertexGroup {
    public ATVector3f p0;
    public List<RMDeformableVertex> vertices = new List<RMDeformableVertex>();
}

public class RMDeformableVertex {
    public RMDeformableMesh mesh;

    public ATVector3f p0;
    public ATVector3f n0;
    public ATVector3f p;
    public ATVector3f n;
    public ATVector2f textureCoordinates;
    public ATVector2f textureCoordinates2;

    public int unityIndex = -1;

    float[] weights = new float[8]; //8
    //public ATVector3f U, V;
    //float[] WNU = new float[8]; //8
    //float[] WNV = new float[8]; //8

    public RMDeformableCell ownerCell;

    int touchInt;

    public RMDeformableVertexGroup vertexGroup;
    //RMDeformableVertex positionArbiter = null;

    public void CalculateWeights() { 
        ATVector3f cubeCoordinates;

        cubeCoordinates = (p0 - ownerCell.boundingBox.min) / ownerCell.boundingBox.Side;

        weights[7] = (cubeCoordinates.x) * (cubeCoordinates.y) * (cubeCoordinates.z);
        weights[6] = (cubeCoordinates.x) * (cubeCoordinates.y) * (1 - cubeCoordinates.z);
        weights[5] = (cubeCoordinates.x) * (1 - cubeCoordinates.y) * (cubeCoordinates.z);
        weights[4] = (cubeCoordinates.x) * (1 - cubeCoordinates.y) * (1 - cubeCoordinates.z);
        weights[3] = (1 - cubeCoordinates.x) * (cubeCoordinates.y) * (cubeCoordinates.z);
        weights[2] = (1 - cubeCoordinates.x) * (cubeCoordinates.y) * (1 - cubeCoordinates.z);
        weights[1] = (1 - cubeCoordinates.x) * (1 - cubeCoordinates.y) * (cubeCoordinates.z);
        weights[0] = (1 - cubeCoordinates.x) * (1 - cubeCoordinates.y) * (1 - cubeCoordinates.z);

        //Debug.Log(ownerCell.boundingBox.Side);
        //Debug.Log(ownerCell.boundingBox.max);
        //Debug.Log(ownerCell.boundingBox.min);
        //Debug.Log(weights[0] + " " + weights[1] + " " + weights[2] + " " + weights[3] + " " + weights[4] + " " + weights[5] + " " + weights[6] + " " + weights[7]);
        /*
        U = new ATVector3f(Random.value, Random.value, Random.value);
        U += -U.Dot(n0) * n0;
        U.Normalize();
        V = n0.CrossProduct(U);

        ATVector3f ncc = cubeCoordinates;
        float xp = ncc.x; float xn = 1 - xp;
        float yp = ncc.y; float yn = 1 - yp;
        float zp = ncc.z; float zn = 1 - zp;
        float[] X = { xn, xp };
        float[] Y = { yn, yp };
        float[] Z = { zn, zp };
        float[] DX = { -1, 1 };
        float[] DY = { -1, 1 };
        float[] DZ = { -1, 1 };
        ATVector3f dPhi;
        int index = 0;
        for (int i = 0; i < 2; i++) 
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    //float wp = X[i] * Y[j] * Z[k];
                    dPhi = new ATVector3f(DX[i] * Y[j] * Z[k],
                                          X[i] * DY[j] * Z[k],
                                          X[i] * Y[j] * DZ[k]);
                    WNU[index] = (float)dPhi.Dot(U);
                    WNV[index] = (float)dPhi.Dot(V);
                    index++;
                }
            }
        }
         */
    }

    public void UpdatePosition() {
        p = ATVector3f.ZERO;
        for (int j = 0; j<8; j++) {
            p += ownerCell.physicsCell.vertices[j].x * weights[j];
        }
        /*
        ATVector3f u = ATVector3f.ZERO;
        ATVector3f v = ATVector3f.ZERO;
        for (int j = 0; j < 8; j++)
        {
            u += ownerCell.physicsCell.vertices[j].g * WNU[j];
            v += ownerCell.physicsCell.vertices[j].g * WNV[j];
        }
        n = u.CrossProduct(v).NormalizedCopy();*/
    
    }
    void DeterminePositionArbiter() { ;}
}