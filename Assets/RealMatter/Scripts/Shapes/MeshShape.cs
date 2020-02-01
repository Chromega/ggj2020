#define PARALLEL_SIMULATION
//#define USE_DEFORMABLE_MESH

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

[AddComponentMenu("Soft Body Physics/Shapes/Mesh")]
public class MeshShape : BodyShape
{
    //Is the object hollow or filled?
    public bool filled = true;
    //Should we ignore intersection and add particles everywhere in the bounding box?
    public bool floodFill = false;
    //Stores the center of the mesh if gizmo drawing is used
    private Vector3 center = new Vector3(0, 0, 0);
    //Stores the offset of the mesh's center so that proper vertex weights can be computing
    private Vector3 centerDistance = new Vector3(0, 0, 0);
    //Array indexing vertex number to corresponding cell
    private RMCell[] vertexCellMapping;
    //Array indexing vertex number to array (length 8) of vertex weights
    private float[][] vertexWeightsMapping;
    //Mesh, Unity
    public Mesh mesh;
    //Discretization of mesh, RealMatter
    private Voxelization voxelization;
    //Which body are we?  We need to know the spacing
    private RMBody body;

    //Handle rendering fractures
    private RMDeformableMesh deformableMesh;

    //Store these during lattice point generation
    private List<Vector3> latticePoints;
    private ATPoint3 resolution;
    private ATBoundingBox3f boundingBox;

    public override List<Vector3> getLatticePoints()
    {
       latticePoints = new List<Vector3>();

        //First, give RealMatter the trimesh

        Mesh mesh = GetComponentInChildren<MeshFilter>().sharedMesh;
        Vector3[] meshVertices = mesh.vertices;
        int[] meshTriangles = mesh.triangles;

        ATTriMesh trimesh = new ATTriMesh();
        ATTriMesh.ATSubmesh submesh = trimesh.CreateSubmesh();

        foreach (Vector3 vertex in meshVertices) {
            trimesh.CreateVertex(Vector3.Scale(vertex,transform.localScale));
        }

        for (int i = 0; i < meshTriangles.Length; i += 3 )
        {
            ATTriMesh.ATTriangle triangle = submesh.CreateTriangle();
            triangle.v[0] = trimesh.vertices[meshTriangles[i]];
            triangle.v[1] = trimesh.vertices[meshTriangles[i + 1]];
            triangle.v[2] = trimesh.vertices[meshTriangles[i + 2]];
        }

        //We want to adhere to the spacing the user specifies, however, the bounding box might not be an integer multiple of this spacing
        //This is corrected here by rounding up the amount of cells required, and then expanding the bounding box symmetrically.
        //So, in a 1D case, say you want a spacing of 2, and your mesh is 3 wide.  We round up to two cells required.
        //The bounding box is then expanded by .5 on the left and the right.  This is what the "difference" parameter does.
        Vector3 spacing = GetComponent<BodyScript>().spacing;
        boundingBox = trimesh.CalculateBoundingBox();
        ATVector3f side = boundingBox.Side;
        resolution = new ATPoint3((int)Mathf.Ceil(side[0] / spacing[0]), (int)Mathf.Ceil(side[1] / spacing[1]), (int)Mathf.Ceil(side[2] / spacing[2]));
        Vector3 newSide = new Vector3(resolution[0] * spacing[0], resolution[1] * spacing[1], resolution[2] * spacing[2]);
        ATVector3f difference = new ATVector3f(newSide[0] - side[0], newSide[1] - side[1], newSide[2] - side[2]);

        voxelization = Voxelization.FromMesh(trimesh, resolution, filled, difference, floodFill);

        center = voxelization.MeshSpaceToLatticeIndexFloat(new ATVector3f(0, 0, 0));
        centerDistance = voxelization.vertexOffset;

        //This function ultimately returns which cells we need, so retrieve those voxels here
        foreach (Voxelization.Voxel v in voxelization.lattice) {
            if (v != null) {
                latticePoints.Add(new Vector3(v.index[0], v.index[1], v.index[2]));
            }
        }

        return latticePoints;
    }

    public override Vector3 getCenter()
    {
        return center;
    }

    public override void finishInitialization(RMBody b) {
        // Start the stopwatch
        //Stopwatch sw = Stopwatch.StartNew();

        //We're ready for the real deal.  Store the mesh and the body, and do some array initializations
        mesh = GetComponent<MeshFilter>().mesh;
        body = b;
        Vector3[] vertices = mesh.vertices;
        vertexCellMapping = new RMCell[vertices.Length];
        vertexWeightsMapping = new float[vertices.Length][];
//#if USE_DEFORMABLE_MESH
        if (body.fracturing)
        {

            //mesh.subMeshCount += 1;
            //UnityEngine.Debug.Log(mesh.subMeshCount);
            //Material[] newMaterials = new Material[renderer.materials.Length+1];
            //renderer.materials.CopyTo(newMaterials, 0);
            //newMaterials[0] = renderer.materials[0];
            //newMaterials[1] = renderer.materials[0];
            //renderer.materials = newMaterials;
            //UnityEngine.Debug.Log(renderer.materials.Length);
            deformableMesh = new RMDeformableMesh();
            deformableMesh.GenerateFromMesh(mesh, body, latticePoints, resolution, voxelization.meshBoundingBox, transform.localScale, gameObject);
            //Debug.Log("GFM");
            //Debug.Log(deformableMesh.lattice);
            //Debug.Log("GFM: " + deformableMesh.lattice[0, 0, 0].faces[0].intersectingTriangles.Count);
//#endif
        }


        // Stop the stopwatch
        //sw.Stop();

        // Report the results
        //UnityEngine.Debug.Log("Time used (float): " + sw.Elapsed.TotalMilliseconds);
        Vector3 localScale = transform.localScale; //Do this ahead of time so that threading doesn't complain
#if PARALLEL_SIMULATION
        RMParallel.For(vertices.Length, delegate(int i)
#else
        for (int i = 0; i < vertices.Length; i++)
#endif
        {
            Vector3 position = Vector3.Scale(vertices[i],localScale);
            ATPoint3 latticeIndex = voxelization.MeshSpaceToLatticeIndex(position);
            RMCell cell = body.lattice[latticeIndex];
            vertexCellMapping[i] = cell;
            vertexWeightsMapping[i] = cell.WorldPosToWeights(position-centerDistance);
        }
#if PARALLEL_SIMULATION
        );
#endif
//#if USE_DEFORMABLE_MESH
        if (body.fracturing)
        {
            //Don't need this whole block if not fracturing

            //Debug.Log("POSTIF1: " + deformableMesh.lattice[0, 0, 0].faces[0].intersectingTriangles.Count);
            deformableMesh.corners = new RMDeformableCellCorner[resolution.x + 1, resolution.y + 1, resolution.z + 1];
            for (int x = 0; x <= resolution.x; x++) for (int y = 0; y <= resolution.y; y++) for (int z = 0; z <= resolution.z; z++)
                    {
                        RMDeformableCellCorner c = new RMDeformableCellCorner();
                        c.index = new ATPoint3(x, y, z);
                        c.originalPosition = body.spacing * (new ATVector3f(x, y, z)) + (ATVector3f)centerDistance;
                        //ATVector3f randVect = new ATVector3f((Random.value * 100) / 100.0f - 0.5f, (Random.value * 100) / 100.0f - 0.5f, (Random.value * 100) / 100.0f - 0.5f); // [-0.5, 0.5]
                        c.shiftedPosition = c.originalPosition;// +(ATVector3f)Vector3.Scale(randVect, body.spacing) / 2.0f;
                        c.mesh = deformableMesh;
                        c.DetermineInMesh();
                        deformableMesh.corners[x, y, z] = c;
                        c.CreateGraph();
                    }
            //UnityEngine.Debug.Log("C:" + centerDistance);
            //Debug.Log(deformableMesh.lattice);
            //Do not need if not fracturing
            foreach (RMDeformableCell c in deformableMesh.lattice)
            {
                if (c != null) c.InitializeFaces2();
            }
//#endif
        }

        // Report the results
        //UnityEngine.Debug.Log("Time used (float): " + sw.Elapsed.TotalMilliseconds);
    }
    public override void updateMesh() {
//#if USE_DEFORMABLE_MESH
        if (body.fracturing) {
        deformableMesh.UpdateVertices();
        //if (!GetComponent<BodyScript>().hasFractured)
        //{
        //    body.FractureCells(body.cells[1], body.cells[0]);
            //body.FractureCells(body.cells[1], body.cells[3]);
            //body.FractureCells(body.cells[3], body.cells[1]);
        //}
        deformableMesh.UpdateFractures();
        //mesh.RecalculateNormals();
//#else
        } else {
            //Called every render frame.  Update the vertex positions, and don't forget about those normals and BB

            //Mesh mesh = GetComponentInChildren<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;

            //Optimization: Inverse transform cell copy instead?
            Matrix4x4 mat = transform.worldToLocalMatrix;
#if PARALLEL_SIMULATION
            RMParallel.For(vertices.Length, delegate(int i)
#else
            for (int i = 0; i < vertices.Length; i++)
#endif
            {
                RMCell cell = vertexCellMapping[i];
                float[] weights = vertexWeightsMapping[i];
                //vertices[i] = transform.InverseTransformPoint(cell.TrilinearlyInterpolateWorldPos(weights));
                vertices[i] = mat.MultiplyPoint3x4(cell.TrilinearlyInterpolateWorldPos(weights));
            }
#if PARALLEL_SIMULATION
    );
#endif
            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
//#endif
        }
    }
}
