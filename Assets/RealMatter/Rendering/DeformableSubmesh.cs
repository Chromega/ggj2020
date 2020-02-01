using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RMDeformableSubmesh {
    public RMDeformableMesh mesh;

    public List<RMDeformableTriangle> triangles = new List<RMDeformableTriangle>();
    public List<RMDeformableTriangle> recentlyCreatedTriangles = new List<RMDeformableTriangle>();

    public int index = 0;

    public RMDeformableSubmesh() {;}

    public void GenerateDisplayList() {
        //TODO: figure out if this is needed
    }
    
}