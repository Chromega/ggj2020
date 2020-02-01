using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RMDeformableCellCornerNode
{
    public RMDeformableCell c;
    public List<RMDeformableCellCornerNode> neighbors;
    public bool visited;
    public int index;
    public RMDeformableCellCornerNode target;

    /*public RMDeformableCellCornerNode()
    {
        c = null;
        neighbors = new List<RMDeformableCellCornerNode>();
        visited = false;
    }*/
}

public class RMDeformableCellCorner {
    public RMDeformableMesh mesh;

    public ATPoint3 index;
    public ATVector3f originalPosition;
    public ATVector3f shiftedPosition;
    public bool inMesh;
    public int numFractures = 0;
    //public bool initialized = false;

    //List<KeyValuePair<RMDeformableCell,RMDeformableCell>> edges = new List<KeyValuePair<RMDeformableCell,RMDeformableCell>>();
    List<RMDeformableCellCornerNode> nodes = new List<RMDeformableCellCornerNode>();
    //Arg, no hash set.  Hack it with a dict
    Dictionary<RMDeformableTriangle,bool> affectedTriangles = new Dictionary<RMDeformableTriangle,bool>();
    //public List<List<RMDeformableCell>> subgraphs = new List<List<RMDeformableCell>>();
    Dictionary<RMDeformableCell, RMDeformableCellCornerNode> cellToNodeMap = new Dictionary<RMDeformableCell, RMDeformableCellCornerNode>();

    public void addTriangle(RMDeformableTriangle t)
    {
        if (!affectedTriangles.ContainsKey(t))
            affectedTriangles[t] = true;
    }

    public RMDeformableCell getTarget(RMDeformableCell cell) {
        //Returns the leader of the subgraph to which cell belongs
        //Assume graph data is up to date
        //return cell;
        return cellToNodeMap[cell].target.c;
    }

    public void DetermineInMesh() {
        bool missingNeighbor;
        try
        {
            /*RMDeformableCell c000 = mesh.lattice[index[0], index[1], index[2]];
            RMDeformableCell c001 = mesh.lattice[index[0], index[1], index[2] + 1];
            RMDeformableCell c010 = mesh.lattice[index[0], index[1] + 1, index[2]];
            RMDeformableCell c011 = mesh.lattice[index[0], index[1] + 1, index[2] + 1];
            RMDeformableCell c100 = mesh.lattice[index[0] + 1, index[1], index[2]];
            RMDeformableCell c101 = mesh.lattice[index[0] + 1, index[1], index[2] + 1];
            RMDeformableCell c110 = mesh.lattice[index[0] + 1, index[1] + 1, index[2]];
            RMDeformableCell c111 = mesh.lattice[index[0] + 1, index[1] + 1, index[2] + 1];*/
            RMDeformableCell c000 = mesh.lattice[index[0]-1, index[1]-1, index[2]-1];
            RMDeformableCell c001 = mesh.lattice[index[0]-1, index[1]-1, index[2]];
            RMDeformableCell c010 = mesh.lattice[index[0]-1, index[1], index[2]-1];
            RMDeformableCell c011 = mesh.lattice[index[0]-1, index[1], index[2]];
            RMDeformableCell c100 = mesh.lattice[index[0], index[1]-1, index[2]-1];
            RMDeformableCell c101 = mesh.lattice[index[0], index[1]-1, index[2]];
            RMDeformableCell c110 = mesh.lattice[index[0], index[1], index[2]-1];
            RMDeformableCell c111 = mesh.lattice[index[0], index[1], index[2]];

            missingNeighbor = (c000 == null || c001 == null || c010 == null || c011 == null ||
                               c100 == null || c101 == null || c110 == null || c111 == null);
        }
        catch
        {
            missingNeighbor = true;
        }


         if (mesh.RayIntersectsModel(originalPosition,ATVector3f.UNIT_X) == false ||
             mesh.RayIntersectsModel(originalPosition,-ATVector3f.UNIT_X) == false ||
             mesh.RayIntersectsModel(originalPosition,ATVector3f.UNIT_Y) == false ||
             mesh.RayIntersectsModel(originalPosition,-ATVector3f.UNIT_Y) == false ||
             mesh.RayIntersectsModel(originalPosition,ATVector3f.UNIT_Z) == false ||
             mesh.RayIntersectsModel(originalPosition,-ATVector3f.UNIT_Z) == false ||
             missingNeighbor) 
         {
             inMesh = false;
         } else {
             inMesh = true;
         }
    }

    public void CornerFracture(RMDeformableCell c1, RMDeformableCell c2)
    {
        //Debug.Log(index);
        //Debug.Log(c1.index);
        //Debug.Log(c2.index);
        RMDeformableCellCornerNode cn1 = cellToNodeMap[c1];
        RMDeformableCellCornerNode cn2 = cellToNodeMap[c2];
        cn1.neighbors.Remove(cn2);
        cn2.neighbors.Remove(cn1);
        numFractures++;
        //Small optimization, we won't be disconnected unless we have at least 3 fractures
        if (numFractures >= 3)
            ComputeConnectivity();
        //Debug.Log("F");
    }

    public void ComputeConnectivity()
    {
        Dictionary<RMDeformableCellCornerNode,bool> unexploredCells = new Dictionary<RMDeformableCellCornerNode,bool>();
        foreach (RMDeformableCellCornerNode n in nodes) {
            unexploredCells.Add(n,true);
        }
        int count = 0;
        int nodeCount = 0;
        while (true)
        {
            List<RMDeformableCellCornerNode> subgraph = new List<RMDeformableCellCornerNode>();
            Queue<RMDeformableCellCornerNode> queue = new Queue<RMDeformableCellCornerNode>();
            RMDeformableCellCornerNode start = null;
            foreach (RMDeformableCellCornerNode n in unexploredCells.Keys)
            {
                start = n;
                break;
            }
            if (start == null)
            {
                //No unexplored nodes left, we're done
                break;
            }
            count++;
            int minIndex = start.index;
            RMDeformableCellCornerNode minNode = start;
            queue.Enqueue(start);
            start.visited = true;
            while (queue.Count != 0)
            {
                nodeCount++;
                RMDeformableCellCornerNode n = queue.Dequeue();
                //Debug.Log("I" + n.index);
                //Debug.Log(n.neighbors.Count);
                unexploredCells.Remove(n);
                subgraph.Add(n);
                if (n.index < minIndex)
                {
                    minIndex = n.index;
                    minNode = n;
                }
                foreach (RMDeformableCellCornerNode neighbor in n.neighbors) {
                    if (neighbor.visited) continue;
                    queue.Enqueue(neighbor);
                    neighbor.visited = true;
                }
            }
            //We've reached all reachable nodes, so we have a subgraph
            foreach (RMDeformableCellCornerNode n in subgraph)
            {
                n.target = minNode;
                n.visited = false;
            }
        }

        //Every past triangle associated with the corner may need this new information
        foreach (RMDeformableTriangle t in affectedTriangles.Keys)
        {
            t.FindOrGenerateVerticesFracturing();
        }
        //Debug.Log("Total subgraphs:" + count);
    }

    public RMDeformableCellCornerNode CreateCornerNode(RMDeformableCell c, int index) {
        RMDeformableCellCornerNode cn = new RMDeformableCellCornerNode();
        cn.c = c;
        cn.neighbors = new List<RMDeformableCellCornerNode>();
        cn.visited = false;
        cn.target = null;
        cn.index = index;
        return cn;
    }
    public void CreateCornerEdge(RMDeformableCellCornerNode c1, RMDeformableCellCornerNode c2)
    {
        c1.neighbors.Add(c2);
        c2.neighbors.Add(c1);
    }

    public void CreateGraph()
    {
        if (!inMesh) { return; }
        /*
        RMDeformableCell c000 = mesh.lattice[index[0], index[1], index[2]];
        RMDeformableCell c001 = mesh.lattice[index[0], index[1], index[2]+1];
        RMDeformableCell c010 = mesh.lattice[index[0], index[1]+1, index[2]];
        RMDeformableCell c011 = mesh.lattice[index[0], index[1]+1, index[2]+1];
        RMDeformableCell c100 = mesh.lattice[index[0]+1, index[1], index[2]];
        RMDeformableCell c101 = mesh.lattice[index[0]+1, index[1], index[2]+1];
        RMDeformableCell c110 = mesh.lattice[index[0]+1, index[1]+1, index[2]];
        RMDeformableCell c111 = mesh.lattice[index[0]+1, index[1]+1, index[2]+1];
        */
        RMDeformableCell c000 = mesh.lattice[index[0] - 1, index[1] - 1, index[2] - 1];
        RMDeformableCell c001 = mesh.lattice[index[0] - 1, index[1] - 1, index[2]];
        RMDeformableCell c010 = mesh.lattice[index[0] - 1, index[1], index[2] - 1];
        RMDeformableCell c011 = mesh.lattice[index[0] - 1, index[1], index[2]];
        RMDeformableCell c100 = mesh.lattice[index[0], index[1] - 1, index[2] - 1];
        RMDeformableCell c101 = mesh.lattice[index[0], index[1] - 1, index[2]];
        RMDeformableCell c110 = mesh.lattice[index[0], index[1], index[2] - 1];
        RMDeformableCell c111 = mesh.lattice[index[0], index[1], index[2]];

        //Debug.Log(mesh.lattice[0, 0, 0]);
        //Debug.Log(mesh.lattice[0, 0, 1]);

        //Create the graph nodes
        RMDeformableCellCornerNode cn000;
        cn000 = CreateCornerNode(c000,0);
        cellToNodeMap[c000] = cn000;
        nodes.Add(cn000);
        RMDeformableCellCornerNode cn001;
        cn001 = CreateCornerNode(c001,1);
        cellToNodeMap[c001] = cn001;
        nodes.Add(cn001);
        RMDeformableCellCornerNode cn010;
        cn010 = CreateCornerNode(c010,2);
        cellToNodeMap[c010] = cn010;
        nodes.Add(cn010);
        RMDeformableCellCornerNode cn011;
        cn011 = CreateCornerNode(c011,3);
        cellToNodeMap[c011] = cn011;
        nodes.Add(cn011);
        RMDeformableCellCornerNode cn100;
        cn100 = CreateCornerNode(c100,4);
        cellToNodeMap[c100] = cn100;
        nodes.Add(cn100);
        RMDeformableCellCornerNode cn101;
        cn101 = CreateCornerNode(c101,5);
        cellToNodeMap[c101] = cn101;
        nodes.Add(cn101);
        RMDeformableCellCornerNode cn110;
        cn110 = CreateCornerNode(c110,6);
        cellToNodeMap[c110] = cn110;
        nodes.Add(cn110);
        RMDeformableCellCornerNode cn111;
        cn111 = CreateCornerNode(c111,7);
        cellToNodeMap[c111] = cn111;
        nodes.Add(cn111);

        //Link up the edges
        CreateCornerEdge(cn000, cn001);
        CreateCornerEdge(cn000, cn010);
        CreateCornerEdge(cn000, cn100);
        CreateCornerEdge(cn001, cn011);
        CreateCornerEdge(cn001, cn101);
        CreateCornerEdge(cn010, cn011);
        CreateCornerEdge(cn010, cn110);
        CreateCornerEdge(cn011, cn111);
        CreateCornerEdge(cn100, cn101);
        CreateCornerEdge(cn100, cn110);
        CreateCornerEdge(cn101, cn111);
        CreateCornerEdge(cn110, cn111);

        ComputeConnectivity();
        //Debug.Log("Success?");
        //initialized = true;
    }
}

public class RMDeformableCellFace {
    public RMDeformableCellFace mirrorFace = null;
    public RMDeformableCell c1, c2;
    public RMDeformableCellCorner[] corners = new RMDeformableCellCorner[4];
    public ATVector3f normal;
    public int[] vertexNos = new int[4];
    public bool exposed = false;
    public bool exists = false;
    public ATBoundingBox3f boundingBox = new ATBoundingBox3f();
    public List<RMDeformableTriangle> intersectingTriangles = new List<RMDeformableTriangle>();
}

public class RMDeformableCell {
    public RMDeformableMesh mesh;

    public ATPoint3 index;
    public ATBoundingBox3f boundingBox = new ATBoundingBox3f();
    public RMDeformableCellFace[] faces = new RMDeformableCellFace[6];
    public List<RMDeformableTriangle> intersectingTriangles = new List<RMDeformableTriangle>();
    public List<RMDeformableTriangle> ownedTriangles = new List<RMDeformableTriangle>();
    public Dictionary<Pair<ATVector3f, ATVector2f>, RMDeformableVertex> vertices = new Dictionary<Pair<ATVector3f, ATVector2f>, RMDeformableVertex>();

    public RMCell physicsCell;

    public RMDeformableCellFace GetFace(RMDeformableCell c2) {
        if (c2.index.x == index.x + 1) return faces[0];
        if (c2.index.x == index.x - 1) return faces[1];
        if (c2.index.y == index.y + 1) return faces[2];
        if (c2.index.y == index.y - 1) return faces[3];
        if (c2.index.z == index.z + 1) return faces[4];
        return faces[5];
    }

    public void InitializeFaces1()
    {
        for (int i = 0; i < 6; i++)
        {
            faces[i] = new RMDeformableCellFace();
            faces[i].boundingBox = new ATBoundingBox3f();
            faces[i].boundingBox.min = boundingBox.min;
            faces[i].boundingBox.max = boundingBox.max;
        }
        faces[0].boundingBox.min.x = boundingBox.max.x;
        faces[1].boundingBox.max.x = boundingBox.min.x;
        faces[2].boundingBox.min.y = boundingBox.max.y;
        faces[3].boundingBox.max.y = boundingBox.min.y;
        faces[4].boundingBox.min.z = boundingBox.max.z;
        faces[5].boundingBox.max.z = boundingBox.min.z;

        //Debug.Log("IF1: " + faces[0].intersectingTriangles.Count);
    }

    public void InitializeFaces2()
    {
        RMDeformableCell c1 = this;
        faces[0].c2 = mesh.GetCheckedCell(index + new ATPoint3(1, 0, 0));
        faces[1].c2 = mesh.GetCheckedCell(index + new ATPoint3(-1, 0, 0));
        faces[2].c2 = mesh.GetCheckedCell(index + new ATPoint3(0, 1, 0));
        faces[3].c2 = mesh.GetCheckedCell(index + new ATPoint3(0, -1, 0));
        faces[4].c2 = mesh.GetCheckedCell(index + new ATPoint3(0, 0, 1));
        faces[5].c2 = mesh.GetCheckedCell(index + new ATPoint3(0, 0, -1));

        faces[0].corners[0] = mesh.corners[index.x + 1, index.y, index.z];
        faces[0].corners[1] = mesh.corners[index.x + 1, index.y + 1, index.z];
        faces[0].corners[2] = mesh.corners[index.x + 1, index.y + 1, index.z + 1];
        faces[0].corners[3] = mesh.corners[index.x + 1, index.y, index.z + 1];
        faces[0].vertexNos[0] = 4;
        faces[0].vertexNos[1] = 5;
        faces[0].vertexNos[2] = 6;
        faces[0].vertexNos[3] = 7;

        faces[1].corners[0] = mesh.corners[index.x, index.y, index.z];
        faces[1].corners[1] = mesh.corners[index.x, index.y + 1, index.z];
        faces[1].corners[2] = mesh.corners[index.x, index.y + 1, index.z + 1];
        faces[1].corners[3] = mesh.corners[index.x, index.y, index.z + 1];
        faces[1].vertexNos[0] = 0;
        faces[1].vertexNos[1] = 1;
        faces[1].vertexNos[2] = 2;
        faces[1].vertexNos[3] = 3;

        faces[2].corners[0] = mesh.corners[index.x, index.y + 1, index.z];
        faces[2].corners[1] = mesh.corners[index.x + 1, index.y + 1, index.z];
        faces[2].corners[2] = mesh.corners[index.x + 1, index.y + 1, index.z + 1];
        faces[2].corners[3] = mesh.corners[index.x, index.y + 1, index.z + 1];
        faces[2].vertexNos[0] = 2;
        faces[2].vertexNos[1] = 3;
        faces[2].vertexNos[2] = 6;
        faces[2].vertexNos[3] = 7;

        faces[3].corners[0] = mesh.corners[index.x, index.y, index.z];
        faces[3].corners[1] = mesh.corners[index.x + 1, index.y, index.z];
        faces[3].corners[2] = mesh.corners[index.x + 1, index.y, index.z + 1];
        faces[3].corners[3] = mesh.corners[index.x, index.y, index.z + 1];
        faces[3].vertexNos[0] = 0;
        faces[3].vertexNos[1] = 1;
        faces[3].vertexNos[2] = 4;
        faces[3].vertexNos[3] = 5;

        faces[4].corners[0] = mesh.corners[index.x, index.y, index.z + 1];
        faces[4].corners[1] = mesh.corners[index.x + 1, index.y, index.z + 1];
        faces[4].corners[2] = mesh.corners[index.x + 1, index.y + 1, index.z + 1];
        faces[4].corners[3] = mesh.corners[index.x, index.y + 1, index.z + 1];
        faces[4].vertexNos[0] = 1;
        faces[4].vertexNos[1] = 3;
        faces[4].vertexNos[2] = 5;
        faces[4].vertexNos[3] = 7;

        faces[5].corners[0] = mesh.corners[index.x, index.y, index.z];
        faces[5].corners[1] = mesh.corners[index.x + 1, index.y, index.z];
        faces[5].corners[2] = mesh.corners[index.x + 1, index.y + 1, index.z];
        faces[5].corners[3] = mesh.corners[index.x, index.y + 1, index.z];
        faces[5].vertexNos[0] = 0;
        faces[5].vertexNos[1] = 2;
        faces[5].vertexNos[2] = 4;
        faces[5].vertexNos[3] = 6;

        for (int i = 0; i < 6; i++)
        {
            faces[i].c1 = c1;
            if (faces[i].c2 == null)
            {
                faces[i].exposed = true;
                faces[i].exists = false;
            }
            else
            {
                faces[i].exists = true;
                faces[i].exposed = false;
                faces[i].mirrorFace = (faces[i].c2.faces[(i / 2) * 2 + 1 - (i % 2)]);
                ATPoint3 offset = faces[i].c2.index - faces[i].c1.index;
                faces[i].normal = new ATVector3f(offset.x, offset.y, offset.z);
            }
        }

        //Debug.Log("IF2: " + faces[0].intersectingTriangles.Count);
    }
}