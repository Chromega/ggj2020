using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RMDeformableVertexInfo
{
    public ATVector3f p, n;

    public int unityIndex;

    public bool isCorner;
    public RMDeformableCellCorner corner;

    static public bool operator >(RMDeformableVertexInfo t, RMDeformableVertexInfo r)
    {
        if (t.p.x > r.p.x) return true;
        else if (t.p.x < r.p.x) return false;

        if (t.p.y > r.p.y) return true;
        else if (t.p.y < r.p.y) return false;

        if (t.p.z > r.p.z) return true;
        else if (t.p.z < r.p.z) return false;

        return false;
    }

    static public bool operator <(RMDeformableVertexInfo t, RMDeformableVertexInfo r)
    {
        if (r == t) return false;
        return !(r > t);
    }

    static public bool operator ==(RMDeformableVertexInfo t, RMDeformableVertexInfo r)
    {
        return (t.p == r.p && t.n == r.n);
    }

    static public bool operator !=(RMDeformableVertexInfo t, RMDeformableVertexInfo r)
    {
        return (t.p != r.p || t.n != r.n);
    }

    public override bool Equals(object o)
    {
        if (o is RMDeformableVertexInfo)
        {
            return (this == (RMDeformableVertexInfo)o);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return p.GetHashCode();
    }
}

public class RMDeformableTriangle {
    public RMDeformableMesh mesh;
    public RMDeformableSubmesh submesh;
    public RMDeformableCell ownerCell;

    public RMDeformableVertexInfo[] vI = new RMDeformableVertexInfo[3]; //3
    public ATVector2f[] textureCoordinates = new ATVector2f[3]; //3
    public ATVector2f[] textureCoordinates2 = new ATVector2f[3]; //3

    public RMDeformableVertex[] v = new RMDeformableVertex[3]; //3
    public ATVector3f n;

    public DigraphNode[] mostRecentlyGeneratedNodes = new DigraphNode[2]; //2
    int touch;

    public RMDeformableTriangle() { ;}

    public void FindOwnerCell() { 
        ATVector3f center = (vI[0].p + vI[1].p + vI[2].p)/3.0f;
        ATPoint3 ownerIndex = mesh.MeshSpaceToLatticeIndex(center);
        ownerCell = mesh.lattice[ownerIndex.x, ownerIndex.y, ownerIndex.z];
        ownerCell.ownedTriangles.Add(this);
    }

    public void FindOrGenerateVertices() {
        //Yeah this block is important, don't remove it
        ATVector3f vertexNormalAverage = (vI[0].n + vI[1].n + vI[2].n).NormalizedCopy();

        ATVector3f e01 = vI[1].p - vI[0].p;
        ATVector3f e12 =  vI[2].p - vI[1].p;
        n = e01.CrossProduct(e12).NormalizedCopy();

        if (vertexNormalAverage.Dot(n)<0) {
            RMDeformableVertexInfo temp = vI[1];
            vI[1] = vI[2];
            vI[2] = temp;
            ATVector2f tempTexture = textureCoordinates[1];
            textureCoordinates[1] = textureCoordinates[2];
            textureCoordinates[2] = tempTexture; 
            tempTexture = textureCoordinates2[1];
            textureCoordinates2[1] = textureCoordinates2[2];
            textureCoordinates2[2] = tempTexture;
            n = -n;
        }
        //Debug.Log("NEWT");
        for (int i = 0; i < 3; i++) {
            v[i] = null;

            ATPoint3 ownerIndex = mesh.MeshSpaceToLatticeIndex(vI[i].p);
            RMDeformableCell vOwnerCell = mesh.lattice[ownerIndex.x, ownerIndex.y, ownerIndex.z];

            vOwnerCell.vertices.TryGetValue(new Pair<ATVector3f, ATVector2f>(vI[i].p, textureCoordinates[i]), out v[i]);

            if (v[i] == null) {
                //Debug.Log("NEW");
                v[i] = new RMDeformableVertex();
                v[i].p0 = vI[i].p;
                v[i].n0 = vI[i].n;
                v[i].textureCoordinates = textureCoordinates[i];
                v[i].textureCoordinates2 = textureCoordinates2[i];

                v[i].unityIndex = mesh.currentUnityIndex;
                vI[i].unityIndex = mesh.currentUnityIndex;
                //v[i].unityIndex =  vI[i].unityIndex;
 
                mesh.currentUnityIndex++;

                //TODO: switch this back when tri fits in a single cell
                v[i].ownerCell = vOwnerCell;

                //ATPoint3 ownerIndex = mesh.MeshSpaceToLatticeIndex(v[i].p0);
                //v[i].ownerCell = mesh.lattice[ownerIndex.x, ownerIndex.y, ownerIndex.z];

                v[i].mesh = mesh;
                vOwnerCell.vertices[new Pair<ATVector3f, ATVector2f>(vI[i].p, textureCoordinates[i])] = v[i];
                mesh.vertices.Add(v[i]);
                v[i].CalculateWeights();
            }
        }
    }
    public void FindOrGenerateVerticesFracturing()
    {
        //Yeah this block is important, don't remove it
        ATVector3f vertexNormalAverage = (vI[0].n + vI[1].n + vI[2].n).NormalizedCopy();

        ATVector3f e01 = vI[1].p - vI[0].p;
        ATVector3f e12 = vI[2].p - vI[1].p;
        n = e01.CrossProduct(e12).NormalizedCopy();

        if (vertexNormalAverage.Dot(n) < 0)
        {
            RMDeformableVertexInfo temp = vI[1];
            vI[1] = vI[2];
            vI[2] = temp;
            ATVector2f tempTexture = textureCoordinates[1];
            textureCoordinates[1] = textureCoordinates[2];
            textureCoordinates[2] = tempTexture;
            tempTexture = textureCoordinates2[1];
            textureCoordinates2[1] = textureCoordinates2[2];
            textureCoordinates2[2] = tempTexture;
            n = -n;
        }
        //Debug.Log("NEWT");
        for (int i = 0; i < 3; i++)
        {
            RMDeformableCell target = ownerCell;
            v[i] = null;
            //TODO: used to iterate through ownercell.vertices
            //Should update this after big triangle splitting is done
            //Debug.Log(vI[i].unityIndex);

            /*foreach (RMDeformableVertex check in mesh.vertices)
            {
                if (check.unityIndex == vI[i].unityIndex) {//check.p0 == vI[i].p && check.n0 == vI[i].n) {
            */

            /*foreach(RMDeformableVertex check in ownerCell.vertices.Values)
            {
                if(check.p0 == vI[i].p && check.n0 == vI[i].n) {

                    v[i] = check;
                    break;
                }
            }*/
            if (vI[i].isCorner)
            {
                //Debug.Log("CORNER");
                vI[i].corner.addTriangle(this);
                target = vI[i].corner.getTarget(ownerCell);
            }
            target.vertices.TryGetValue(new Pair<ATVector3f, ATVector2f>(vI[i].p, textureCoordinates[i]), out v[i]);

            if (v[i] == null)
            {
                //Debug.Log("NEW");
                v[i] = new RMDeformableVertex();
                v[i].p0 = vI[i].p;
                v[i].n0 = vI[i].n;
                v[i].textureCoordinates = textureCoordinates[i];
                v[i].textureCoordinates2 = textureCoordinates2[i];

                v[i].unityIndex = mesh.currentUnityIndex;
                vI[i].unityIndex = mesh.currentUnityIndex;
                //v[i].unityIndex =  vI[i].unityIndex;

                mesh.currentUnityIndex++;

                //TODO: switch this back when tri fits in a single cell
                v[i].ownerCell = target;

                //ATPoint3 ownerIndex = mesh.MeshSpaceToLatticeIndex(v[i].p0);
                //v[i].ownerCell = mesh.lattice[ownerIndex.x, ownerIndex.y, ownerIndex.z];

                v[i].mesh = mesh;
                target.vertices[new Pair<ATVector3f, ATVector2f>(vI[i].p, textureCoordinates[i])] = v[i];
                mesh.vertices.Add(v[i]);
                v[i].CalculateWeights();
                /*
                RMDeformableVertexGroup group;
                if (mesh.vertexGroups.ContainsKey(v[i].p0)) {
                    group = mesh.vertexGroups[v[0].p0];
                } else {
                    group = new RMDeformableVertexGroup();
                    group.p0 = v[i].p0;
                    mesh.vertexGroups[v[i].p0] = group;
                }
                v[i].vertexGroup = group;
                group.vertices.Add(v[i]);*/
            }
        }
    }
}