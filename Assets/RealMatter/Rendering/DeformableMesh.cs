#define PARALLEL_SIMULATION

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;

public partial class RMDeformableMesh {
    bool initialized;

    RMBody body;
    bool fracturingEnabled;

    ATBoundingBox3f meshBoundingBox;

    ATPoint3 resolution;
    ATVector3f spacing;
    public RMDeformableCell[, ,] lattice;
    public RMDeformableCellCorner[, ,] corners;
    List<RMDeformableCell> cells = new List<RMDeformableCell>();

    List<RMDeformableSubmesh> submeshes = new List<RMDeformableSubmesh>();

    public List<RMDeformableVertex> vertices = new List<RMDeformableVertex>();
    public Dictionary<ATVector3f, RMDeformableVertexGroup> vertexGroups = new Dictionary<ATVector3f,RMDeformableVertexGroup>();

    RMDeformableSubmesh exposedSurface;

    private ATVector3f vertexOffset;

    private Mesh mesh;

    private GameObject gameObject;

    public int currentUnityIndex = 0;

    private int vertexArraySize = 0;
    private Vector3[] meshVertices = new Vector3[0];
    private Vector2[] meshUVs = new Vector2[0];
    private Vector2[] meshUV2s = new Vector2[0];

    bool useUV2 = false;
    //private Vector3[] meshNormals = new Vector3[0];

    public void GenerateFromMesh(Mesh mesh, RMBody body, List<Vector3> LatticePoints, ATPoint3 resolution, ATBoundingBox3f boundingBox, Vector3 localScale, GameObject gameObject)
    {
        // Start the stopwatch
        //Stopwatch sw = Stopwatch.StartNew();
        this.resolution = resolution;
        this.spacing = body.spacing;
        this.body = body;
        this.mesh = mesh;
        this.gameObject = gameObject;

        this.meshBoundingBox = boundingBox;

        //Offset everything so the mesh's bounding box min is at zero
        vertexOffset = meshBoundingBox.min;
        //meshBoundingBox.max += vertexOffset;
        //meshBoundingBox.min = ATVector3f.ZERO;

        //Split triangles that are too big to fit in the lattice cubes
        float minCubeDimension = float.MaxValue;
        if(resolution.x > 1)
            minCubeDimension = Math.Min(minCubeDimension, spacing.x);
        if (resolution.y > 1)
            minCubeDimension = Math.Min(minCubeDimension, spacing.y);
        if (resolution.x > 1)
            minCubeDimension = Math.Min(minCubeDimension, spacing.z);

        // Report the results
        //UnityEngine.Debug.Log("A: Time used (float): " + sw.Elapsed.TotalMilliseconds);

        //TODO: SPLIT LARGE TRIANGLES

        //Create the lattice
        lattice = new RMDeformableCell[resolution.x, resolution.y, resolution.z];
        for (int x = 0 ; x < resolution.x; x++) for (int y = 0; y < resolution.y; y++) for (int z = 0; z < resolution.z; z++)
            lattice[x,y,z] = null;
        
        //Create a voxel at each possible location, delete unneeded ones later
        //for (int x = 0 ; x < resolution.x; x++) for (int y = 0; y < resolution.y; y++) for (int z = 0; z < resolution.z; z++)
        //UnityEngine.Debug.Log("SP:" + spacing);
        foreach (Vector3 point in LatticePoints)
        {
            RMDeformableCell cell = new RMDeformableCell();
            cell.mesh = this;
            cell.index = new ATPoint3((int)point.x, (int)point.y, (int)point.z);
            cell.boundingBox.min = new ATVector3f((int)point.x, (int)point.y, (int)point.z) * spacing + vertexOffset;
            cell.boundingBox.max = cell.boundingBox.min + spacing;
            //Debug.Log(cell.boundingBox.Side);
            cell.InitializeFaces1();
            lattice[(int)point.x, (int)point.y, (int)point.z] = cell;
        }
        //UnityEngine.Debug.Log("VO:" + vertexOffset);
        //UnityEngine.Debug.Log("B: Time used (float): " + sw.Elapsed.TotalMilliseconds);
        //Load submeshes
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            //int[] triangles = mesh.triangles;
            int[] triangles = mesh.GetTriangles(i);
            Vector2[] uv = mesh.uv;
            Vector2[] uv2 = mesh.uv2;
            useUV2 = uv2.Length > 0;
            //Debug.Log(vertexOffset);
            //Debug.Log(localScale);
            //for (int qwer = 0; qwer < vertices.Length; qwer++)
            //{
            //    Debug.Log(vertices[qwer]);
            //}
            RMDeformableSubmesh ds = new RMDeformableSubmesh();
            ds.index = i;
            ds.mesh = this;
            submeshes.Add(ds);
            for (int ot = 0; ot < triangles.Length; ot += 3) {
                RMDeformableTriangle dt = new RMDeformableTriangle();
                dt.mesh = this;
                dt.submesh = ds;
                dt.n = ATVector3f.ZERO;
                for (int v = 0; v < 3; v++) {
                    dt.vI[v].p = (ATVector3f)(Vector3.Scale(localScale, vertices[triangles[ot + v]]));// +vertexOffset;
                    dt.vI[v].n = normals[triangles[ot + v]];
                    dt.vI[v].unityIndex = triangles[ot + v];
                    dt.textureCoordinates[v] = uv[triangles[ot + v]];
                    if (useUV2)
                        dt.textureCoordinates2[v] = uv2[triangles[ot + v]];
                }
                ds.triangles.Add(dt);
            }
        }
        //UnityEngine.Debug.Log("C: Time used (float): " + sw.Elapsed.TotalMilliseconds);
        
        //Generate all triangles
        foreach (RMDeformableSubmesh submesh in submeshes) foreach (RMDeformableTriangle t in submesh.triangles) {
            t.FindOwnerCell();
            t.FindOrGenerateVertices();
        }
        //UnityEngine.Debug.Log("D: Time used (float): " + sw.Elapsed.TotalMilliseconds);

        //Create the submesh for the exposed surface
        exposedSurface = new RMDeformableSubmesh();
        exposedSurface.mesh = this;
        exposedSurface.index = mesh.subMeshCount;
        mesh.subMeshCount += 1;
        Material[] newMaterials = new Material[gameObject.GetComponent<Renderer>().materials.Length+1];
        gameObject.GetComponent<Renderer>().materials.CopyTo(newMaterials, 0);
        newMaterials[newMaterials.Length - 1] = gameObject.GetComponent<BodyScript>().fractureMaterial;
        gameObject.GetComponent<Renderer>().materials = newMaterials;
        submeshes.Add(exposedSurface);
        
        //For each triangle, find which cells it intersects with and add it to their list of intersecting triangles
        foreach (RMDeformableSubmesh submesh in submeshes) 
//#if PARALLEL_SIMULATION
//        RMParallel.ForEach(submesh.triangles, delegate(RMDeformableTriangle t)
//#else
            foreach (RMDeformableTriangle t in submesh.triangles)
//#endif
        {
            //Debug.Log("Triangle " + t);
            //First, find the bounding box of the triangle
            ATBoundingBox3f bb = new ATBoundingBox3f();
            bb.AddPoint(t.vI[0].p);
            bb.AddPoint(t.vI[1].p);
            bb.AddPoint(t.vI[2].p);
            //Debug.Log("TRI: " + bb.Side + " " + bb.min + " " + bb.max);

            //Now find the max and min possible lattice cells
            ATPoint3 minCell = MeshSpaceToLatticeIndex(bb.min);
            ATPoint3 maxCell = MeshSpaceToLatticeIndex(bb.max);

            //for each possible cell, test intersection
            for (int x = minCell.x; x <= maxCell.x; x++) for (int y = minCell.y; y <= maxCell.y; y++) for (int z = minCell.z; z <= maxCell.z; z++)
            {
                RMDeformableCell c = lattice[x, y, z];
                if (c == null) continue;
                //Debug.Log(c.boundingBox);
                //Test it against the whole cell
                if (TriangleIntersectsBox(t.vI[0].p,t.vI[1].p,t.vI[2].p,c.boundingBox.min,c.boundingBox.max)) {
                    //Debug.Log(c);
                    c.intersectingTriangles.Add(t);
                    //Debug.Log("yes");

                    //bool intersectsAnyFace = false;
                    for(int face = 0; face < 6; face += 2) {
                        RMDeformableCellFace f = c.faces[face];
                        //Debug.Log(f);
                        //Debug.Log(f.boundingBox);
                        if(TriangleIntersectsBox(t.vI[0].p,t.vI[1].p,t.vI[2].p,f.boundingBox.min,f.boundingBox.max)) {
                            f.intersectingTriangles.Add(t);
                            //Debug.Log("yes" + face + " " + f.boundingBox.min + " " + f.boundingBox.max);
                            //intersectsAnyFace = true;
                        }
                    }
                }
            }
        }
//#if PARALLEL_SIMULATION
//        );
//#endif
        //UnityEngine.Debug.Log("E: Time used (float): " + sw.Elapsed.TotalMilliseconds);
        //Delete the unneeded cells
        for (int x = 0 ; x < resolution.x; x++) for (int y = 0; y < resolution.y; y++) for (int z = 0; z < resolution.z; z++)
        {
            if(lattice[x,y,z] != null) {
                cells.Add(lattice[x, y, z]);
            }
        }

        foreach (Vector3 point in LatticePoints) {
            lattice[(int)point.x, (int)point.y, (int)point.z].physicsCell = body.lattice[new ATPoint3((int)point.x, (int)point.y, (int)point.z)];
        }

        //Break any faces that do not have any intersecting triangles
        //TODO: Do not know if need!

        //Re-enumerate vertices and triangles

        //mesh.triangles = new int[0];

        foreach (RMDeformableSubmesh submesh in submeshes)
        {
            mesh.SetTriangles(new int[0], submesh.index);
        }
        //mesh.Clear();
        UpdateVertices();
        //mesh.triangles = meshTrianglesList.ToArray();
        foreach (RMDeformableSubmesh submesh in submeshes)
        {
            List<int> meshTrianglesList = new List<int>();
            foreach (RMDeformableTriangle tri in submesh.triangles)
            {
                meshTrianglesList.Add(tri.v[0].unityIndex);
                meshTrianglesList.Add(tri.v[1].unityIndex);
                meshTrianglesList.Add(tri.v[2].unityIndex);
            }
            mesh.SetTriangles(meshTrianglesList.ToArray(), submesh.index);
        }
        //UnityEngine.Debug.Log("F: Time used (float): " + sw.Elapsed.TotalMilliseconds);

    }

    public ATPoint3 MeshSpaceToLatticeIndex(ATVector3f pt)
    {
        ATVector3f indexV = (pt-vertexOffset) / spacing;
        ATPoint3 index = new ATPoint3((int)Math.Floor(indexV.x), (int)Math.Floor(indexV.y), (int)Math.Floor(indexV.z));
        if (index.x >= resolution.x) index.x = resolution.x - 1;
        if (index.y >= resolution.y) index.y = resolution.y - 1;
        if (index.z >= resolution.z) index.z = resolution.z - 1;
        return index;
    }

    public void UpdateFractures() {
        //Don't destroy recent fractures early
        //TODO: must be some Unity bookkeeping somewhere
        if (body.recentFractures.Count > 0) {
            foreach (RMBody.FractureInfo fracture in body.recentFractures) {
                ATPoint3 c1I = fracture.a.index;
                //UnityEngine.Debug.Log("FRACTURING");
                ATPoint3 c2I = fracture.b.index;
                RMDeformableCell c1 = lattice[c1I.x, c1I.y, c1I.z];
                RMDeformableCell c2 = lattice[c2I.x, c2I.y, c2I.z];
                SplitCells(c1, c2);
            }

            UpdateVertices();
            //int[] meshTriangles = mesh.triangles;
            //This could be optimized if we kept track of which triangles changed
            //It's not just recentlyCreatedTriangles, since we modify some old ones, too
            //meshTrianglesList.AddRange(meshTriangles);
            foreach (RMDeformableSubmesh submesh in submeshes) {
                List<int> meshTrianglesList = new List<int>();
                //foreach (RMDeformableTriangle tri in submesh.recentlyCreatedTriangles)
                foreach (RMDeformableTriangle tri in submesh.triangles)
                {
                    meshTrianglesList.Add(tri.v[0].unityIndex);
                    meshTrianglesList.Add(tri.v[1].unityIndex);
                    meshTrianglesList.Add(tri.v[2].unityIndex);
                }
                submesh.recentlyCreatedTriangles.Clear();
                mesh.SetTriangles(meshTrianglesList.ToArray(), submesh.index);
            }
            //mesh.triangles = meshTrianglesList.ToArray();
            //mesh.subMeshCount  = 2;
            //UnityEngine.Debug.Log(mesh.subMeshCount);
            //mesh.SetTriangles(new int[0], 1);
            //body.recentFractures.Clear();
        }
    }

    public void UpdateVertices() {
        //UnityEngine.Debug.Log(mesh.vertices.Length);
        //UnityEngine.Debug.Log(mesh.uv.Length);
        //Vector3[] meshVertices = mesh.vertices;
        //Vector3[] meshNormals = mesh.normals;
        //Store arrays so that we don't have to keep generating them!...weird, slows stuff down
        //memory is weird
        bool updateUVs = false;
        if (currentUnityIndex != vertexArraySize)
        {
            meshVertices = new Vector3[currentUnityIndex];
            meshUVs = new Vector2[currentUnityIndex];
            meshUV2s = new Vector2[currentUnityIndex];
            updateUVs = true;
            //Vector3[] meshNormals = new Vector3[currentUnityIndex];
            vertexArraySize = currentUnityIndex;
        }

        //Debug.Log(vertices.Count);
        Matrix4x4 mat = gameObject.transform.worldToLocalMatrix;
#if PARALLEL_SIMULATION
        RMParallel.ForEach(vertices, delegate(RMDeformableVertex v)
#else
        foreach (RMDeformableVertex v in vertices) 
#endif
        {
            v.UpdatePosition();
            //meshVertices[v.unityIndex] = gameObject.transform.InverseTransformPoint(v.p);
            meshVertices[v.unityIndex] = mat.MultiplyPoint3x4(v.p);
            if (updateUVs)
            {
                meshUVs[v.unityIndex] = v.textureCoordinates;
                meshUV2s[v.unityIndex] = v.textureCoordinates2;
            }
            //meshNormals[v.unityIndex] = v.n;
        }
#if PARALLEL_SIMULATION
        );
#endif
        mesh.vertices = meshVertices;
        if (updateUVs)
        {
            mesh.uv = meshUVs;
            if (useUV2)
                mesh.uv2 = meshUV2s;
        }
        //mesh.normals = meshNormals;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        //UnityEngine.Debug.Log(mesh.vertices.Length);
        //UnityEngine.Debug.Log(mesh.uv.Length);
    }

    public RMDeformableCell GetCheckedCell(ATPoint3 index) {
        if (index.x < 0 || index.x >= resolution.x || index.y < 0 || index.y >= resolution.y || index.z < 0 || index.z >= resolution.z) return null;
        return lattice[index.x, index.y, index.z];
    }

    void Reset() {;}

    public bool RayIntersectsModel(ATVector3f rayStart, ATVector3f rayDirection) {
        List<RMDeformableTriangle> potentialHitTriangles = new List<RMDeformableTriangle>();

        ATPoint3 index = MeshSpaceToLatticeIndex(rayStart);
        while(true) {
            RMDeformableCell c = GetCheckedCell(index);
            if (c == null) {
                break;
            }
            else {
                foreach (RMDeformableTriangle t in c.intersectingTriangles) {
                    potentialHitTriangles.Add(t);
                }
            }
            index += new ATPoint3((int)rayDirection.x, (int)rayDirection.y, (int)rayDirection.z);
        }

        foreach (RMDeformableTriangle t in potentialHitTriangles) {
            if (RayIntersectsTriangle(rayStart, rayDirection, t.vI[0].p, t.vI[1].p, t.vI[2].p))
                return true;
        }

        return false;
    }

    bool TriangleIntersectsBox(ATVector3f t0, ATVector3f t1, ATVector3f t2, ATVector3f bMin, ATVector3f bMax) {
        double[] boxHalfSize = { (bMax.x - bMin.x) / 2.0f, (bMax.y - bMin.y) / 2.0f, (bMax.z - bMin.z) / 2.0f };
        double[] boxCelter = { bMin.x + boxHalfSize[0], bMin.y + boxHalfSize[1], bMin.z + boxHalfSize[2] };

        double[][] triVerts = new double[][] {new double [] {t0.x,t0.y,t0.z},
                                              new double [] {t1.x,t1.y,t1.z},
                                              new double [] {t2.x,t2.y,t2.z}};
        return triBoxOverlap(boxCelter, boxHalfSize, triVerts);
    }

    bool triBoxOverlap(double[] boxcenter, double[] boxhalfsize, double[][] triverts) {
        double[] v0 = new double[3];
        double[] v1 = new double[3];
        double[] v2 = new double[3];
        double min, max, d, p0, p1, p2, rad, fex, fey, fez;
        double[] normal = new double[3];
        double[] e0 = new double[3];
        double[] e1 = new double[3];
        double[] e2 = new double[3];
        double a,b,fa,fb;

        SUB(v0, triverts[0], boxcenter);
        SUB(v1, triverts[1], boxcenter);
        SUB(v2, triverts[2], boxcenter);

        SUB(e0, v1, v0);
        SUB(e1, v2, v1);
        SUB(e2, v0, v2);

        fex = Math.Abs(e0[0]);
        fey = Math.Abs(e0[1]);
        fez = Math.Abs(e0[2]);

        //AXISTEXT_X01(e0[2],e0[1],fez,fey)
        a=e0[2];b=e0[1];fa=fez;fb=fey;
        p0 = a*v0[1] - b*v0[2];
        p2 = a*v2[1] - b*v2[2];
            if(p0<p2) {min=p0; max=p2;} else {min=p2; max=p0;}
        rad = fa * boxhalfsize[1] + fb * boxhalfsize[2];
        if(min>rad || max<-rad) return false;

        //AXISTEST_Y02(e0[Z], e0[X], fez, fex);
        a=e0[2];b=e0[0];fa=fez;fb=fex;
        p0 = -a*v0[0] + b*v0[2];
        p2 = -a*v2[0] + b*v2[2];
            if(p0<p2) {min=p0; max=p2;} else {min=p2; max=p0;}
        rad = fa * boxhalfsize[0] + fb * boxhalfsize[2];
        if(min>rad || max<-rad) return false;

        //AXISTEST_Z12(e0[Y], e0[X], fey, fex);
        a=e0[1];b=e0[0];fa=fey;fb=fex;
        p1 = a*v1[0] - b*v1[1];
        p2 = a*v2[0] - b*v2[1];
            if(p2<p1) {min=p2; max=p1;} else {min=p1; max=p2;}
        rad = fa * boxhalfsize[0] + fb * boxhalfsize[1];
        if(min>rad || max<-rad) return false;
         
        fex = Math.Abs(e1[0]);
        fey = Math.Abs(e1[1]);
        fez = Math.Abs(e1[2]);
           
        //AXISTEST_X01(e1[Z], e1[Y], fez, fey);
        a=e1[2];b=e1[1];fa=fez;fb=fey;
        p0 = a*v0[1] - b*v0[2];
        p2 = a*v2[1] - b*v2[2];
            if(p0<p2) {min=p0; max=p2;} else {min=p2; max=p0;}
        rad = fa * boxhalfsize[1] + fb * boxhalfsize[2];
        if(min>rad || max<-rad) return false;

        //AXISTEST_Y02(e1[Z], e1[X], fez, fex);
        a=e1[2];b=e1[0];fa=fez;fb=fex;
        p0 = -a*v0[0] + b*v0[2];
        p2 = -a*v2[0] + b*v2[2];
            if(p0<p2) {min=p0; max=p2;} else {min=p2; max=p0;}
        rad = fa * boxhalfsize[0] + fb * boxhalfsize[2];
        if(min>rad || max<-rad) return false;

        //AXISTEST_Z0(e1[Y], e1[X], fey, fex);
        a=e1[1];b=e1[0];fa=fey;fb=fex;
        p0 = a*v0[0] - b*v0[1];
        p1 = a*v1[0] - b*v1[1];
            if(p0<p1) {min=p0; max=p1;} else {min=p1; max=p0;}
        rad = fa * boxhalfsize[0] + fb * boxhalfsize[1];
        if(min>rad || max<-rad) return false;
         
        fex = Math.Abs(e2[0]);
        fey = Math.Abs(e2[1]);
        fez = Math.Abs(e2[2]);

        //AXISTEST_X2(e2[Z], e2[Y], fez, fey);
        a=e2[2];b=e2[1];fa=fez;fb=fey;
        p0 = a*v0[1] - b*v0[2];
        p1 = a*v1[1] - b*v1[2];
            if(p0<p1) {min=p0; max=p1;} else {min=p1; max=p0;}
        rad = fa * boxhalfsize[1] + fb * boxhalfsize[2];
        if(min>rad || max<-rad) return false;

        //AXISTEST_Y1(e2[Z], e2[X], fez, fex);
        a=e2[2];b=e2[0];fa=fez;fb=fex;
        p0 = -a*v0[0] + b*v0[2];
        p1 = -a*v1[0] + b*v1[2];
            if(p0<p1) {min=p0; max=p1;} else {min=p1; max=p0;}
        rad = fa * boxhalfsize[0] + fb * boxhalfsize[2];
        if(min>rad || max<-rad) return false;

        //AXISTEST_Z12(e2[Y], e2[X], fey, fex);
        a=e2[1];b=e2[0];fa=fey;fb=fex;
        p1 = a*v1[0] - b*v1[1];
        p2 = a*v2[0] - b*v2[1];
            if(p2<p1) {min=p2; max=p1;} else {min=p1; max=p2;}
        rad = fa * boxhalfsize[0] + fb * boxhalfsize[1];
        if(min>rad || max<-rad) return false;

        /* Bullet 1: */
        /*  first test overlap in the {x,y,z}-directions */
        /*  find min, max of the triangle each direction, and test for overlap in
        */
        /*  that direction -- this is equivalent to testing a minimal AABB around
        */
        /*  the triangle against the AABB */

        /* test in X-direction */
        FINDMINMAX(v0[0], v1[0], v2[0], ref min, ref max);
        if (min > boxhalfsize[0] || max < -boxhalfsize[0]) return false;

        /* test in Y-direction */
        FINDMINMAX(v0[1], v1[1], v2[1], ref min, ref max);
        if (min > boxhalfsize[1] || max < -boxhalfsize[1]) return false;

        /* test in Z-direction */
        FINDMINMAX(v0[2], v1[2], v2[2], ref min, ref max);
        if (min > boxhalfsize[2] || max < -boxhalfsize[2]) return false;

        /* Bullet 2: */
        /*  test if the box intersects the plane of the triangle */
        /*  compute plane equation of triangle: normal*x+d=0 */
        CROSS(normal, e0, e1);
        d = -DOT(normal, v0);  /* plane eq: normal.x+d=0 */
        if (!planeBoxOverlap(normal, d, boxhalfsize)) return false;

        return true;   /* box and triangle overlaps */
    }

    void SUB(double[] dest, double[] v1, double[] v2) {
        dest[0]=v1[0]-v2[0];
        dest[1]=v1[1]-v2[1];
        dest[2]=v1[2]-v2[2];
    }

    void FINDMINMAX(double x0, double x1, double x2, ref double min, ref double max) {
        min = max = x0;
        if(x1<min) min=x1;
        if(x1>max) max=x1;
        if(x2<min) min=x2;
        if(x2>max) max=x2;
    }

    void CROSS(double[] dest, double[] v1, double[] v2) {
        dest[0]=v1[1]*v2[2]-v1[2]*v2[1];
        dest[1]=v1[2]*v2[0]-v1[0]*v2[2];
        dest[2]=v1[0]*v2[1]-v1[1]*v2[0];
    }
    
    double DOT(double[] v1, double[] v2) {
        return (v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2]);
    }

    bool planeBoxOverlap(double[] normal,double d, double[] maxbox)
    {
      int q;
      double[] vmin = new double[3];
      double[] vmax = new double[3];
      for(q=0;q<=2;q++)
      {
        if(normal[q]>0.0f)
        {
          vmin[q]=-maxbox[q];
          vmax[q]=maxbox[q];
        }
        else
        {
          vmin[q]=maxbox[q];
          vmax[q]=-maxbox[q];
        }
      }
      if(DOT(normal,vmin)+d>0.0f) return false;
      if(DOT(normal,vmax)+d>=0.0f) return true;
     
      return false;
    }
    /*
    public void SplitLargeTriangles(float maxEdgeLength)
    {
	    // From Mueller et al., Physically-Based Simulation of Objects Represented by Surface Meshes

	    int totalBefore = 0;
	    int totalAfter = 0;

	    // Create lexicographically sorted list of all possible triples (v1, v2, t) where (v1, v2) is an
	    // edge of a triangle with index t and v1 > v2 are vertex indices.
	    List<EdgeTriple> edges;
	    foreach(ObjSubmesh *submesh in submeshes) foreach(ObjTriangle *t in submesh->triangles)
	    {
		    ConditionallyAddEdges(edges, t, maxEdgeLength);
		    totalBefore++;
	    }
	    sort(edges.begin(), edges.end());

	    // Traverse the list iteratively. If the current edge is shorter than size h skip the entry.
	    for(uint i = 0; i < edges.size(); i++)
	    {
		    EdgeTriple e = edges[i];
		    ObjVertex v0 = e.v0;
		    ObjVertex v1 = e.v1;

		    // Otherwise, there are two cases. Triple (v1, v2, t) is either followed by triple (v1, v2, t')
		    // or it is not. In the first case the edge is shared by two triangles t and t', while in the
		    // second case the edge belongs only to t.

		    // In both cases, a new vertex v3 near the center of the edge is generated. Triange t and, if
		    // present, triangle t' are each split into two new triangles containing v3.

		    // Create a new vertex in the middle of the edge
		    ObjVertex mid;
		    Vector2f midTex;
		    mid.p = (v0.p + v1.p) / 2.0f;
		    mid.n = (v0.n + v1.n).NormalizedCopy();
		    mid.index = vertices.size();
		    vertices.push_back(mid);

		    std::vector<EdgeTriple> newEdges;

		    // Create the new triangles
		    while(e.v0 == v0 && e.v1 == v1)
		    {
			    ObjTriangle *t = e.t;

			    pair<ObjTriangle*, ObjTriangle*> newTriangles = SplitTriangle(t, mid, v0, v1);

			    // Finally the list has to be updated. The edges of the newly generated triangles that contain v3
			    // are added at the end of the list. If the new entries are added in lexicographical order at the
			    // end of the list, the entire list remains sorted, because v3 is the largest vertex index of the
			    // mesh.

			    // There are only four new edges containing vMid
			    EdgeTriple a = EdgeTriple(newTriangles.first->v[0], newTriangles.first->v[1], newTriangles.first);
			    EdgeTriple b = EdgeTriple(newTriangles.first->v[2], newTriangles.first->v[0], newTriangles.first);
			    EdgeTriple c = EdgeTriple(newTriangles.second->v[0], newTriangles.second->v[1], newTriangles.second);
			    EdgeTriple d = EdgeTriple(newTriangles.second->v[2], newTriangles.second->v[0], newTriangles.second);

			    if((a.v0.p - a.v1.p).Length() >= maxEdgeLength)
				    newEdges.push_back(a);
			    if((b.v0.p - b.v1.p).Length() >= maxEdgeLength)
				    newEdges.push_back(b);
			    if((c.v0.p - c.v1.p).Length() >= maxEdgeLength)
				    newEdges.push_back(c);
			    if((d.v0.p - d.v1.p).Length() >= maxEdgeLength)
				    newEdges.push_back(d);

			    // We will insert them later to make sure that they are inserted in lexicographical order

			    // The edges of the newly generated triangles that do not contain v3 are already in the list
			    // IF THEY ARE LONG ENOUGH (-Alec -- unique to my version)
			    // but need to be updated since they contain the old triangles t and t'. They can be found in
			    // logarithmic time via a binary search on the sorted list.
			    EdgeTriple e1 = EdgeTriple(newTriangles.first->v[1], newTriangles.first->v[2], t);
			    if((e1.v0.p - e1.v1.p).Length() >= maxEdgeLength)
			    {
				    UpdateEdge(edges, e1, newTriangles.first);
			    }
			    EdgeTriple e2 = EdgeTriple(newTriangles.second->v[1], newTriangles.second->v[2], t);
			    if((e2.v0.p - e2.v1.p).Length() >= maxEdgeLength)
			    {
				    UpdateEdge(edges, e2, newTriangles.second);
			    }

			    i++;
			    if(i >= edges.size()) break;
			    e = edges[i];
		    }
		    // Back up, because we went one too far
		    i--;
		    e = edges[i];
    	
		    // We can only insert the new edges now, because only now are we sure that all new triangles
		    // containing mid have been created
		    sort(newEdges.begin(), newEdges.end());

		    foreach(EdgeTriple e in newEdges)
			    edges.push_back(e);
	    }

	    // Delete all split triangles
	    foreach(ObjSubmesh *submesh in submeshes)
	    {
		    std::vector<ObjTriangle*> notSplitTriangles;
		    for(uint i = 0; i < submesh->triangles.size(); i++)
		    {
			    ObjTriangle *t = submesh->triangles[i];
			    if(t->hasBeenSplit)
				    ;//delete t;
			    else
				    notSplitTriangles.push_back(t);
		    }
		    submesh->triangles = notSplitTriangles;
		    totalAfter += submesh->triangles.size();
	    }
    }*/
}
/*
class EdgeTriple
{
	public int v0, v1;
	public ObjTriangle *t;

	public static bool operator <(EdgeTriple q, EdgeTriple r)
	{
		if(q.v0 < r.v0) return true;
		else if(r.v0 < q.v0) return false;

		if(q.v1 < r.v1) return true;
		else if(r.v1 < q.v1) return false;

		if(q.t < r.t) return true;
		else if(r.t < q.t) return false;

		return false;		// They are equal
	}

	public static bool operator > (EdgeTriple q, EdgeTriple r)
	{
		if(q.v0 > r.v0) return true;
		else if(r.v0 > q.v0) return false;

		if(q.v1 > r.v1) return true;
		else if(r.v1 > q.v1) return false;

		if(q.t > r.t) return true;
		else if(r.t > q.t) return false;

		return false;		// They are equal
	}

	public static bool operator == (EdgeTriple q, EdgeTriple r)
	{
		return (q.v0 == r.v0 && q.v1 == r.v1 && q.t == r.t);
	}

	public static bool operator != (EdgeTriple q, EdgeTriple r)
	{
		return (v0 != r.v0 || v1 != r.v1 || t != r.t);
	}

	EdgeTriple(int v0, int v1, int t)
	{
        this.v0 = v0;
        this.v1 = v1;
        this.t = t;
		if(this.v1 > this.v0) {
            int temp = this.v0;
            this.v0 = this.v1;
            this.v1 = temp;
        }
	}
};*/