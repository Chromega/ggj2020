//#define PROJECTIVE_RENDER
#define PARALLEL_SIMULATION

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class RMDeformableMesh
{
    enum FaceType { XPos, XNeg, YPos, YNeg, ZPos, ZNeg };

    const double SMALL_NUM = .00000001;

    static bool LineSegmentFaceTest(ATVector3f l1, ATVector3f l2, ATBoundingBox3f bb, ATPoint3 offset, ref ATVector3f pt)
    {
        float frac = 0.0f;
        ATVector3f swapTemp;

        if (offset.x != 0)
        {
            if (l2.x < l1.x) { swapTemp = l1; l1 = l2; l2 = swapTemp; }
            frac = ((bb.min.x - l1.x) / (l2.x - l1.x));
        }
        else if (offset.y != 0)
        {
            if (l2.y < l1.y) { swapTemp = l1; l1 = l2; l2 = swapTemp; }
            frac = ((bb.min.y - l1.y) / (l2.y - l1.y));
        }
        else if (offset.z != 0)
        {
            if (l2.z < l1.z) { swapTemp = l1; l1 = l2; l2 = swapTemp; }
            frac = ((bb.min.z - l1.z) / (l2.z - l1.z));
        }
        if (frac < 0 || frac > 1) return false;
        pt = l1 + (l2 - l1) * frac;

        if (offset.x != 0)
        {
            pt.x = bb.min.x;
        }
        else if (offset.y != 0)
        {
            pt.y = bb.min.y;
        }
        else if (offset.z != 0)
        {
            pt.z = bb.min.z;
        }

        if (!(bb.ContainsPoint(pt))) return false;

        return true;
    }

    static bool LineSegmentFaceTestVerbose(ATVector3f l1, ATVector3f l2, ATBoundingBox3f bb, ATPoint3 offset, ref ATVector3f pt)
    {
        float frac = 0.0f;
        ATVector3f swapTemp;

        if (offset.x != 0)
        {
            if (l2.x < l1.x) { swapTemp = l1; l1 = l2; l2 = swapTemp; }
            frac = ((bb.min.x - l1.x) / (l2.x - l1.x));
        }
        else if (offset.y != 0)
        {
            if (l2.y < l1.y) { swapTemp = l1; l1 = l2; l2 = swapTemp; }
            frac = ((bb.min.y - l1.y) / (l2.y - l1.y));
        }
        else if (offset.z != 0)
        {
            if (l2.z < l1.z) { swapTemp = l1; l1 = l2; l2 = swapTemp; }
            frac = ((bb.min.z - l1.z) / (l2.z - l1.z));
        }
        if (frac < 0 || frac > 1) return false;
        pt = l1 + (l2 - l1) * frac;

        if (offset.x != 0)
        {
            pt.x = bb.min.x;
        }
        else if (offset.y != 0)
        {
            pt.y = bb.min.y;
        }
        else if (offset.z != 0)
        {
            pt.z = bb.min.z;
        }
        Debug.Log(pt);
        if (!(bb.ContainsPoint(pt))) return false;

        return true;
    }

    public static int LineSegmentTriangleIntersection(ATVector3f l0, ATVector3f l1, ATVector3f t0, ATVector3f t1, ATVector3f t2, ref ATVector3f pt) {
        ATVector3f u, v, n;
        ATVector3f dir, w0, w;
        float r, a, b;

        u = t1 - t0;
        v = t2 - t0;
        n = u.CrossProduct(v);
        //if (n == new ATVector3f(0, 0, 0))
        if (n.Length() < .000001)
        {
            return -1;
        }

        dir = l1 - l0;
        w0 = l0 - t0;
        a = -n.Dot(w0);
        b = n.Dot(dir);
        if (Math.Abs(b) < SMALL_NUM)
        {
            //if (a == 0)
            if (Math.Abs(a) < SMALL_NUM)
                return 2;
            else return 0;
        }

        r = a / b;
        if (r < 0.0)
            return 0;

        pt = l0 + r * dir;

        float uu, uv, vv, wu, wv, D;
        uu = u.Dot(u);
        uv = u.Dot(v);
        vv = v.Dot(v);
        w = pt - t0;
        wu = w.Dot(u);
        wv = w.Dot(v);
        D = uv * uv - uu * vv;

        float s, t;
        s = (uv * wv - vv * wu) / D;
        if (s < 0.0 || s > 1.0)
            return 0;
        t = (uv * wu - uu * wv) / D;
        if (t < 0.0 || (s+t) > 1.0)
            return 0;

        return 1;
    }

    static bool RayIntersectsTriangle(ATVector3f p, ATVector3f d, ATVector3f v0, ATVector3f v1, ATVector3f v2) {
        ATVector3f e1, e2, h, s, q;
        float a, f, u, v;

        e1 = v1 - v0;
        e2 = v2 - v0;
        h = d.CrossProduct(e2);
        a = e1.Dot(h);

        if (a > -0.000001f && a < .000001f)
            return false;

        f = 1 / a;
        s = p - v0;
        u = f * (s.Dot(h));

        if (u < 0.0f || u > 1.0f)
            return false;

        q = s.CrossProduct(e1);
        v = f * d.Dot(q);
        if (v < 0.0f || u + v > 1.0f)
            return false;
        float t = f * e2.Dot(q);
        if (t > -.000001f)
            return true;
        else
            return false;
    }

    public void SplitCells(RMDeformableCell c1, RMDeformableCell c2)
    {
           //OK, here's the big one.
           //First, separate the surface.
           //We do this by finding all vertices that have at least one parent triangle in one cell and at least one other parent triangle in another cell.
           //HACK. We will deal with this later. For now, all vertices will be unwelded.
           //Second, generate the closing surface.
    /*
          1. Generate directed graph of nodes.
                1a. Go through the set of polygons and test them against face f.
                1b. If they intersect, generate nodes n1 and n2 either when an edge of t intersects f or when an edge of f intersects t.
                1c. Generate a directed edge between n1 and n2, according to the rule n1->n2 iff [nt x (n2 - n1)].nf > 0; nf is normal of face with respect to c1.
                1d. Connect coincident nodes from the node without an outgoing edge to the one with one.
                1e. Generate four nodes at the corners of f.
                1f. Walk along the edges of f in the positive direction, connecting sequential nodes iff the first node has no outgoing edge yet
                1g. Detect cycles, pruning branch edges and unused points
          2. Extend the nodes outward to connect to the surface mesh. (That is, close the mesh.)
                2a. Assign to each node a vertex of the surface mesh.
                2b. Move each pair of nodes n1, n2 generated by triangle t towards each other by one third of the segment length (to make sure the triangulation generates everything).
                2c. Delete all consecutive nodes on the formed cycles that refer to the same surface vertex.
                2d. Use the original locations of the nodes on f to compute a 2D trianglulation using ear-cutting.
                2e. Span the generated triangles across the assigned surface vertices.
                2f. For each generated triangle t, create a duplicate triangle t' with reversed orientation. Assign t to c1 and t' to c2.
    */

     
           //Assert that the two cubes are immediate neighbors
          //ATVector3f vertexOffset = gameObject.GetComponent<BodyShape>().getCenter();
          //Debug.Log(vertexOffset);
          ATPoint3 offset = c2.index - c1.index;
          //Debug.Log(c1.index);
          //Debug.Log(c2.index);
          //Debug.Log(offset);
          //if(offset.x == 0) return;
     
           //Find the face
          RMDeformableCellFace face = c1.GetFace(c2);
          //for (uint i = 0; i < 6; i++)
          //  Debug.Log(lattice[0,0,0].faces[i].intersectingTriangles.Count);
         
           //1a. Go through the set of polygons and test them against face f.
           //1b. If they intersect, generate nodes n1 and n2 either when an edge of t intersects f or when an edge of f intersects t.
           //Now, it seems to me we could take the set of triangles intersecting f as the set that intersect both c1 && c2, but that is not how the paper does it so it is not how we will do it either.
          Digraph faceGraph = new Digraph();
          List<SubDigraph> cycles = new List<SubDigraph>();
          List<RMDeformableTriangle> intersectingTriangles = new List<RMDeformableTriangle>();
          List<DigraphNode> faceEdgeNodes = new List<DigraphNode>();
          faceGraph.nodes.Clear();
          faceGraph.edges.Clear();
          cycles.Clear();
          List<RMDeformableTriangle> faceIntersectingTriangles;
          if(face.intersectingTriangles.Count == 0)
                faceIntersectingTriangles = face.mirrorFace.intersectingTriangles;
          else
                faceIntersectingTriangles = face.intersectingTriangles;
            //Expect 8

          foreach (RMDeformableTriangle t in faceIntersectingTriangles)
          {
              t.FindOrGenerateVerticesFracturing();
          }
          //return;
            //If this is to be parallelized, need a mutex or something, faceGraph is modified by all threads
          foreach(RMDeformableTriangle t in faceIntersectingTriangles)
          {
              /*if (t.ownerCell != c1)
              {
                  continue;
              }*/
                 //Test the triangle against the face
                 //Do this by testing each edge of the triangle against the face, and then if you only have one intersection, testing each edge of the face against the triangle
                           
                 //Check each triangle edge in turn
                ATVector3f pt = ATVector3f.ZERO;
                int numIntersect = 0;
                List<DigraphNode> generatedNodes = new List<DigraphNode>();
                List<DigraphNode> generatedFaceEdgeNodes = new List<DigraphNode>();

                /*Debug.Log(LineSegmentTriangleIntersection(
                   new ATVector3f(.4f,.4f,-1f),
                   new ATVector3f(.4f,.4f,1f),
                   new ATVector3f(0,0,0),
                   new ATVector3f(1,0,0),
                   new ATVector3f(0,1,0),
                   ref pt));
                Debug.Log(pt);*/
     
                 //Check for face-intersection nodes first, as these count as edge nodes
                for(int faceEdge = 0; faceEdge < 4; faceEdge++)
                {
                      if(numIntersect >= 2) break;
                      //TODO: Will using shared mesh vertices make things unhappy when new vertices are added?
                      if(LineSegmentTriangleIntersection(face.corners[faceEdge].originalPosition,
                          face.corners[(faceEdge + 1) % 4].originalPosition, 
                          t.v[0].p0, t.v[1].p0, t.v[2].p0, ref pt) == 1)
                      {
                            if(numIntersect == 1 && pt == generatedNodes[0].pos)
                            {
                                   //Ignore this because we don't want to generate more than one coincident point
                            }
                            else
                            {
                                //Debug.Log("LO: " + gameObject.GetComponent<BodyShape>().getCenter());
                                //Debug.Log("FACEEDGENODE");
                                  numIntersect++;
                                  DigraphNode node = new DigraphNode();
                                  //if (t.ownerCell != c1 && t.ownerCell != c2)
                                  //{
                                  //    node.genType = DigraphNode.GenType.GENERATED_BY_OTHER_FACE_EDGE;
                                  //}
                                  //else
                                  //{
                                      node.genType = DigraphNode.GenType.GENERATED_BY_FACE_EDGE;
                                  //}
                                  node.generatingTriangle = t;
                                  node.pos = pt;
                                  //Debug.Log(pt);
                                  generatedNodes.Add(node);
                                  generatedFaceEdgeNodes.Add(node);
                                  t.mostRecentlyGeneratedNodes[numIntersect-1] = node;
                                //Debug.Log(face.corners[faceEdge].originalPosition);
                                //Debug.Log(face.corners[(faceEdge + 1) % 4].originalPosition);
                                //Debug.Log(t.v[0].p0 + " " + t.v[1].p0 + " " + t.v[2].p0);
                                //Debug.Log(pt);
                            }
                      }
                }
              //Mark: The face's bounding box already takes the vertex offset into account and lives in mesh space
                 //Then check for triangle intersections
                for(int triangleEdge = 0; triangleEdge < 3; triangleEdge++)
                {
                    if (numIntersect >= 2) break;
                      //Debug.Log("TRYING TRIEDGE " + triangleEdge + ", " + t.v[triangleEdge].p0 + " " + t.v[(triangleEdge + 1) % 3].p0);
                      if(LineSegmentFaceTest(t.v[triangleEdge].p0,//+vertexOffset,
                          t.v[(triangleEdge + 1) % 3].p0,//+vertexOffset, 
                          face.boundingBox, offset, ref pt))
                      {

                          //pt = pt - vertexOffset;
                          //Debug.Log("INTERSECTION");
                            if(numIntersect == 1 && pt == generatedNodes[0].pos)
                            {
                                   //Ignore this because we don't want to generate more than one coincident point
                            }
                            else
                            {
                                //Debug.Log(face.boundingBox.Center);
                                  numIntersect++;
                                  DigraphNode node = new DigraphNode();
                                  node.genType = DigraphNode.GenType.GENERATED_BY_TRIANGLE_EDGE;
                                  node.generatingTriangle = t;
                                  node.v1 = t.vI[triangleEdge];
                                  node.v2 = t.vI[(triangleEdge + 1) % 3];
                                  node.pos = pt;
                                  //Debug.Log(pt);
                                  generatedNodes.Add(node);
                                  t.mostRecentlyGeneratedNodes[numIntersect-1] = node;
                            }
                      }
                }
     
                 //If we intersect, generate the edge between the two nodes, and add ourself to the list of intersecting triangles.
                if(numIntersect != 0)
                {
                      if(numIntersect != 2)
                      {
                          //Should only hit this case for degenerate triangles
                          //We'll just skip over those, neighboring triangle intersections will merge
                          /*for (int triangleEdge = 0; triangleEdge < 3; triangleEdge++)
                          { Debug.Log(t.v[triangleEdge].p0); }
                          Debug.Log(numIntersect);
                          //ATVector3f a = t.v[1].p0;
                          //ATVector3f b = t.v[2].p0;
                          //Debug.Log(a);
                          //Debug.Log(b);
                          Debug.Log(offset);
                          Debug.Log("Triangle edges");
                          for (int triangleEdge = 0; triangleEdge < 3; triangleEdge++) {
                              Debug.Log(LineSegmentFaceTestVerbose(t.v[triangleEdge].p0,//+vertexOffset,
                              t.v[(triangleEdge+1)%3].p0,//+vertexOffset, 
                              face.boundingBox, offset, ref pt));
                          }
                          Debug.Log("XP" + (t.v[0].p0-t.v[1].p0).CrossProduct(t.v[0].p0-t.v[2].p0));
                          Debug.Log("Face edge");
                          for(int faceEdge = 0; faceEdge < 4; faceEdge++)
                          {
                              Debug.Log(face.corners[faceEdge].originalPosition);
                              //Debug.Log(LineSegmentTriangleIntersection(face.corners[faceEdge].originalPosition,
                              //    face.corners[(faceEdge + 1) % 4].originalPosition,
                              //    t.v[0].p0, t.v[1].p0, t.v[2].p0, ref pt) == 1);
                          }
                          Debug.Log("BBox");
                          Debug.Log(face.boundingBox.min);
                          Debug.Log(face.boundingBox.max);
                          Debug.Log(c1.boundingBox.min);
                          Debug.Log(c1.boundingBox.max);
                          Debug.Log(c2.boundingBox.min);
                          Debug.Log(c2.boundingBox.max);*/
                          Debug.Log("ERROR: Triangles should never have just one intersection.");
                          ;// float f = 1;
                            //assert(0);                  // Error.
                      }
                      else
                      {
                            //faceGraph.nodes.
                            faceGraph.nodes.InsertRange(0, generatedNodes);
                            faceEdgeNodes.InsertRange(0, generatedFaceEdgeNodes);
                           
                            intersectingTriangles.Add(t);
     
                             //1c. Generate a directed edge between n1 and n2, according to the rule n1->n2 iff [nt x (n2 - n1)].nf > 0; nf is normal of face with respect to c1. (Eqn. 1 in Mueller paper)
                            DigraphEdge edge = new DigraphEdge();
                            float directionDecider = t.n.CrossProduct(t.mostRecentlyGeneratedNodes[1].pos - t.mostRecentlyGeneratedNodes[0].pos).Dot(face.normal);
                            //Debug.Log(directionDecider);
                            if(directionDecider < 0) {
                                //std::swap(t.mostRecentlyGeneratedNodes[0], t.mostRecentlyGeneratedNodes[1]);
                                DigraphNode temp = t.mostRecentlyGeneratedNodes[0];
                                t.mostRecentlyGeneratedNodes[0] = t.mostRecentlyGeneratedNodes[1];
                                t.mostRecentlyGeneratedNodes[1] = temp;
                            }
                            edge.a = t.mostRecentlyGeneratedNodes[0];
                            edge.b = t.mostRecentlyGeneratedNodes[1];
                            edge.a.outgoing = edge;     // Must update nodes too
                            edge.b.incoming = edge;
                            faceGraph.edges.Add(edge);
                      }
                }
          }

          //Debug.Log(faceGraph.nodes.Count);
          //Debug.Log(faceGraph.edges.Count);
        //MARK
        //So, at this point we have determined where the triangles intersect the faces.  The intervals of intersection are stored in the faceGraph.edges,
        //and the new nodes (vertices) are stored in faceGraph.nodes.  Not sure exactly what faceEdgeNodes are yet.
     
           //Opportunity to break out early: if there are no intersecting triangles, and the corners are not in the mesh, then break out now
           //to avoid generating a capping face that is actually outside the model.
          if(intersectingTriangles.Count == 0 && face.corners[0].inMesh == false && face.corners[1].inMesh == false && face.corners[2].inMesh == false && face.corners[3].inMesh == false)
          {
                return;
          }
          //Debug.Log("NOT BREAK OUT");
          //Debug.Log(faceGraph.nodes.Count);
          //Debug.Log(faceGraph.edges.Count);
     
           //1d. Connect coincident nodes from the node without an outgoing edge to the one with one.
            //MARK: If we've generated nodes at the same place, they're really the same node, so link them by sharing input and output edges
          
        //foreach (DigraphNode node in faceGraph.nodes) {
        //    Debug.Log(node.pos);
        //}
        //foreach(DigraphNode node in faceGraph.nodes)
         // {
           //     foreach(DigraphNode check in faceGraph.nodes)
             //   {
        for (int i = 0; i < faceGraph.nodes.Count; i++) {
            DigraphNode node = faceGraph.nodes[i];
            for (int j = i+1; j < faceGraph.nodes.Count; j++) {
                DigraphNode check = faceGraph.nodes[j];
                      if((check.pos-node.pos).Length()<.0000001f)//(check.pos == node.pos)//
                      {
                            DigraphNode a = null, b = null;
                            if(node.incoming != null && node.outgoing == null && check.incoming == null)
                            {
                                  a = node;
                                  b = check;
                            }
                            else if(check.incoming != null && check.outgoing == null && node.incoming == null)
                            {
                                  a = check;
                                  b = node;
                            }
                           
                            if(a != null && b != null)
                            {
                                  DigraphEdge edge = new DigraphEdge();
                                  edge.a = a;
                                  edge.b = b;
                                  edge.a.outgoing = edge;     // Must update nodes too
                                  edge.b.incoming = edge;
                                  faceGraph.edges.Add(edge);
                            }
                      }
                }
            /*if (node.incoming == null || node.outgoing == null)
            {
                Debug.Log(node.pos);
            }*/
          }
        //Debug.Log("---");
        /*faceGraph.nodes.Sort(
            delegate(DigraphNode firstNode,
            DigraphNode nextNode)
                        {
                            if (firstNode.pos.x < nextNode.pos.x) { return -1; }
                            else if (firstNode.pos.x > nextNode.pos.x) { return 1; }
                            else if (firstNode.pos.y < nextNode.pos.y) { return -1; }
                            else if (firstNode.pos.y > nextNode.pos.y) { return 1; }
                            else { return 0; }
                        }
            );*/
        /*List<DigraphNode> stragglers = new List<DigraphNode>();
        foreach (DigraphNode node in faceGraph.nodes)
        {
            if (node.incoming == null || node.outgoing == null)
            {
                stragglers.Add(node);
                Debug.Log(node.pos);
            }
        }*/






        /*for (int i = 0; i < stragglers.Count; i++)
        {
            DigraphNode node = stragglers[i];
            for (int j = i + 1; j < stragglers.Count; j++)
            {
                DigraphNode check = stragglers[j];
                if ((check.pos-node.pos).Length()<.2f)//(check.pos == node.pos)//
                {
                    DigraphNode a = null, b = null;
                    if (node.incoming != null && node.outgoing == null && check.incoming == null)
                    {
                        a = node;
                        b = check;
                    }
                    else if (check.incoming != null && check.outgoing == null && node.incoming == null)
                    {
                        a = check;
                        b = node;
                    }

                    if (a != null && b != null)
                    {
                        DigraphEdge edge = new DigraphEdge();
                        edge.a = a;
                        edge.b = b;
                        edge.a.outgoing = edge;     // Must update nodes too
                        edge.b.incoming = edge;
                        faceGraph.edges.Add(edge);
                    }
                }
            }*/
            /*if (node.incoming == null || node.outgoing == null)
            {
                Debug.Log(node.pos);
            }*/
        //}



          for(int i = 0; i < 4; i++)
          {
              if (!face.corners[i].inMesh)
              {
                  continue;
              }
              //Debug.Log(face.corners[i].initialized);
              face.corners[i].CornerFracture(c1, c2);
          }

          //Debug.Log(faceGraph.edges.Count);
     
           //1e. Generate four nodes at the corners of f.
          for(int i = 0; i < 4; i++)
          {
              if (!face.corners[i].inMesh)
              {
                  continue;
              }
                 //Make sure a node with this point doesn't already exist, which can happen in pathological cases
                bool alreadyExists = false;
                foreach(DigraphNode node in faceGraph.nodes)
                {
                      if(node.pos == face.corners[i].originalPosition)
                      {
                            alreadyExists = true;
                            break;
                      }
                }
                if(!alreadyExists)
                {
                      DigraphNode node = new DigraphNode();
                      node.genType = DigraphNode.GenType.CORNER;
                      node.generatingTriangle = null;
                      node.generatingCorner = face.corners[i];
                      node.pos = face.corners[i].shiftedPosition;// originalPosition;
                      faceGraph.nodes.Add(node);
                      faceEdgeNodes.Add(node);
                }
          }
     
           //1f. Walk along the edges of f in the positive direction, connecting sequential nodes iff the first node has no outgoing edge yet
           //We do this by sorting all nodes on any edge of the face by their angle relative to the center of the face
          Dictionary<float, DigraphNode> nodesAndAngles = new Dictionary<float,DigraphNode>();
          ATVector3f faceCenter = ATVector3f.ZERO;
          for(int i = 0; i < 4; i++) faceCenter += face.corners[i].originalPosition;
          faceCenter /= 4.0f;
          //Debug.Log(faceEdgeNodes.Count);
          foreach(DigraphNode node in faceEdgeNodes)
          {
                float angle;
                ATVector3f v = node.pos - faceCenter;
     
                float x, y;
                x = 0;
                y = 0;
                if(offset.x == 1)
                {
                      x = -v.z;
                      y = v.y;
                }
                else if(offset.x == -1)
                {
                      x = v.z;
                      y = v.y;
                }
                else if(offset.y == 1)
                {
                      x = v.x;
                      y = -v.z;
                }
                else if(offset.y == -1)
                {
                      x = v.x;
                      y = v.z;
                }
                else if(offset.z == 1)
                {
                      x = v.x;
                      y = v.y;
                }
                else if(offset.z == -1)
                {
                      x = -v.x;
                      y = v.y;
                }
     
                angle = Mathf.Atan2(y, x);
     
                nodesAndAngles[angle] = node;
          }
          //Debug.Log(nodesAndAngles.Keys.Count);
     
           //OK, now we can do the walk.
           //...connecting sequential nodes iff the first node has no outgoing edge yet

        //MARK TODO: Make sure the nodes are properly sorted before connecting the face nodes??
        //Sorted for test case, don't believe in general

        /*
          DigraphNode[] values = new DigraphNode[nodesAndAngles.Count];
          nodesAndAngles.Values.CopyTo(values, 0);
          int jkl = 0;
          float tempf = 0;
          foreach (float key in nodesAndAngles.Keys)
          {
              if (jkl == 0)
              {
                  tempf = key;
              }
              //Debug.Log(key);
              else if (tempf > key)
              {
                  DigraphNode tempdn = values[0];
                  values[0] = values[1];
                  values[1] = tempdn;
              }
              jkl++;
              //Debug.Log(nodesAndAngles[key]);
          }*/
        
        /*
          for (int asdf = 0; asdf < 4; asdf++)
            Debug.Log(values[asdf].pos);
         */
        List<KeyValuePair<float, DigraphNode>> myList = new List<KeyValuePair<float, DigraphNode>>(nodesAndAngles);
        myList.Sort(
            delegate(KeyValuePair<float,DigraphNode> firstPair,
            KeyValuePair<float, DigraphNode> nextPair)
            {
                return firstPair.Key.CompareTo(nextPair.Key);
            }
        );
        DigraphNode[] values = new DigraphNode[nodesAndAngles.Count];
        //Debug.Log("START");
        for (int i = 0; i < nodesAndAngles.Count; i++)
        {
            //Debug.Log(myList[i].Key);
            values[i] = myList[i].Value;
        }
        //Debug.Log("END");

          for(int iter = 0; iter<values.Length; )
          {
                DigraphNode a, b;
               
                // Get the two nodes -- this one, and the next one
                a = values[iter];
                iter++;
                if(iter != values.Length)
                      b = values[iter];
                else
                      b = values[0];
     
                // Now add an edge if appropriate
                if(a.outgoing == null)
                {
                      DigraphEdge edge = new DigraphEdge();
                      edge.a = a;
                      edge.b = b;
                      edge.a.outgoing = edge;     // Must update nodes too
                      //if (b.incoming == null)
                      //      edge.b.incoming = edge;
                      faceGraph.edges.Add(edge);
                }
          }
          //Debug.Log(faceGraph.nodes.Count);
          //Debug.Log(faceGraph.edges.Count);
           //1g. Detect cycles, pruning branch edges and unused points
          int touch = 1;
          bool foundUnclassifiedNode = true;
          while(foundUnclassifiedNode)
          {
                touch++;
                DigraphNode current = null;
                foundUnclassifiedNode = false;
                foreach(DigraphNode node in faceGraph.nodes)
                {
                      if(node.touch < 0)
                      {
                            foundUnclassifiedNode = true;
                            current = node;
                            break;
                      }
                }
                if(!foundUnclassifiedNode) break;
     
                // First follow this guy to his limit, touching the nodes
                while(current != null && current.touch < 0)
                {
                      current.touch = touch;
                      if(current.outgoing != null)
                            current = current.outgoing.b;
                      else
                            current = null;
                }
     
                 //Now, if we've come back on our own touch, hooray! We've got a cycle
                if(current != null && current.touch == touch)
                {
                      // Pull out the cycle
                      SubDigraph cycle = new SubDigraph();
                      touch++;
                      while(current.touch == touch - 1)
                      {
                            current.touch = touch;
                            cycle.nodes.Add(current);
                            cycle.edges.Add(current.outgoing);
                            current = current.outgoing.b;
                      }
     
                       //OK, we've got the cycle.
                      cycles.Add(cycle);
                }
          }
          //Debug.Log(cycles.Count);
          //Debug.Log(cycles[0].nodes.Count);
        //We should have two cycles here.  One of the outermost face edges, and one of the inner loop of the mesh's intersection
         
           //2. Extend the nodes outward to connect to the surface mesh. (That is, close the mesh.)
           //2a. Assign to each node a vertex of the surface mesh.
          foreach(DigraphNode node in faceGraph.nodes)
          {
                // See Sec. 3.2 in Mueller paper for rules on vertex assignment.
                // If n is generated by a triangle edge e = (v1, v2)...
                if(node.genType == DigraphNode.GenType.GENERATED_BY_TRIANGLE_EDGE)
                {
                      // Find out which vertex of the edge that generated this node lies on the c1 side and which lies on the c2 side of the face.
                      ATVector3f diff = node.v2.p - node.v1.p;
                      //Debug.Log(diff);
                      RMDeformableVertexInfo c1SideVertex, c2SideVertex;
                      //Debug.Log(diff.Dot(face.normal));
                      if(diff.Dot(face.normal) > 0)
                      {
                            c1SideVertex = node.v1;
                            c2SideVertex = node.v2;
                            //Debug.Log(c1SideVertex.p.x);
                      }
                      else
                      {
                            c1SideVertex = node.v2;
                            c2SideVertex = node.v1;
                            //Debug.Log(c1SideVertex.p.x);
                      }
                      //Debug.Log("c1: " + c1SideVertex.p);
                      //Debug.Log("c2: " + c2SideVertex.p);
     
                       //... and the corresponding triangle t is assigned to c1, then the vertex of e which does not lie on the c1 side of f is selected.
                      if(node.generatingTriangle.ownerCell == c1)
                      {
                            node.assignedVertex = c2SideVertex;
                      }
                       //If triangle t is not assigned to cube c1 then the vertex of e which lies on the c1 side of f is selected.
                      else
                      {
                          node.assignedVertex = c1SideVertex;
                      }
#if PROJECTIVE_RENDER
                      node.assignedVertex = new RMDeformableVertexInfo();
                      node.assignedVertex.p = node.pos;
#endif
                }
                else if(node.genType == DigraphNode.GenType.GENERATED_BY_FACE_EDGE)
                {
                      RMDeformableTriangle t = node.generatingTriangle;
                      // Choose the vertex depending on the second node n' generated by t.
                      DigraphNode n2;
                      if(t.mostRecentlyGeneratedNodes[0] == node)
                            n2 = t.mostRecentlyGeneratedNodes[1];
                      else
                            n2 = t.mostRecentlyGeneratedNodes[0];
     
                       //If n' is generated by the intersection of an edge (v1, v2) of t, we select the third vertex v3 of t.
                       //TODO: Is this really right? Do we really want to attach ourselves to the far end of a triangle that isn't even in c1 or c2 ???
                      if(n2.genType == DigraphNode.GenType.GENERATED_BY_TRIANGLE_EDGE)
                      {
                          //Debug.Log("TRIEDGE");
                            for(int i = 0; i < 3; i++)
                            {
                                  if(t.vI[i] != n2.v1 && t.vI[i] != n2.v2)
                                  {
                                      //Debug.Log(i);
                                        node.assignedVertex = t.vI[i];
#if PROJECTIVE_RENDER
                                        node.assignedVertex = new RMDeformableVertexInfo();
                                        node.assignedVertex.p = node.pos;
#endif
                                        break;
                                  }
                            }
                          //Mark edit
                            //if (t.ownerCell != c1 && t.ownerCell != c2)
                            //    node.assignedVertex = node.incoming.a.assignedVertex;
                          //      node.assignedVertex = n2.assignedVertex;
                          /*
                            if (t.ownerCell != c1 && t.ownerCell != c2)
                            {
                                ATVector3f diff = n2.v2.p - n2.v1.p;
                                //Debug.Log(diff);
                                RMDeformableVertexInfo c1SideVertex, c2SideVertex;
                                //Debug.Log(diff.Dot(face.normal));
                                if (diff.Dot(face.normal) > 0)
                                {
                                    c1SideVertex = n2.v1;
                                    c2SideVertex = n2.v2;
                                    //Debug.Log(c1SideVertex.p.x);
                                }
                                else
                                {
                                    c1SideVertex = n2.v2;
                                    c2SideVertex = n2.v1;
                                    //Debug.Log(c1SideVertex.p.x);
                                }
                                //Debug.Log("c1: " + c1SideVertex.p);
                                //Debug.Log("c2: " + c2SideVertex.p);

                                //... and the corresponding triangle t is assigned to c1, then the vertex of e which does not lie on the c1 side of f is selected.
                                if (n2.generatingTriangle.ownerCell == c1)
                                {
                                    node.assignedVertex = c2SideVertex;
                                }
                                //If triangle t is not assigned to cube c1 then the vertex of e which lies on the c1 side of f is selected.
                                else
                                {
                                    node.assignedVertex = c1SideVertex;
                                }
                            }*/




                            //node.assignedVertex = n2.assignedVertex;
                            //Debug.Log(node.assignedVertex.p);
                            //assert(node->assignedVertex != NULL);
                      }
                       //Otherwise, if n' is also generated by the intersection of an edge of f with t, we chose the vertex v of t closest to n.
                      else if((t.ownerCell != c1 && t.ownerCell != c2) || n2.genType == DigraphNode.GenType.GENERATED_BY_FACE_EDGE)
                      {
                          //Debug.Log("FACEEDGE");
                            float closestDistance = float.MaxValue;
                            RMDeformableVertexInfo closest = new RMDeformableVertexInfo();
                            ;// bool foundClosest = false;
                            for(int i = 0; i < 3; i++)
                            {
                                  float dist = (t.vI[i].p - node.pos).Length();
                                  if(dist < closestDistance)
                                  {
                                        closestDistance = dist;
                                        closest = t.vI[i];
                                        //foundClosest = true;
                                  }
                            }
                            //assert(foundClosest);
                            node.assignedVertex = closest;
#if PROJECTIVE_RENDER
                            node.assignedVertex = new RMDeformableVertexInfo();
                            node.assignedVertex.p = node.pos;
#endif
                      }
                      else
                      {
                            //assert(0);        // Error.
                      }
                }
                else if(node.genType == DigraphNode.GenType.CORNER)
                {
                    //if (node.generatingCorner.inMesh)
                    //{
                    //    Debug.Log("HALP");
                    //}
                       //If we're looking at a corner vertex, generate something appropriate (may or may not result in creating a vertex)a
                      RMDeformableVertexInfo v;
                      v.p = node.generatingCorner.shiftedPosition;
                      v.n = face.normal;
                      //TODO: REAL INDEX
                      v.unityIndex = 0;
                      v.isCorner = true;
                      v.corner = node.generatingCorner;
                      node.assignedVertex = v;
#if PROJECTIVE_RENDER
                      node.assignedVertex = new RMDeformableVertexInfo();
                      node.assignedVertex.p = node.generatingCorner.originalPosition;
#endif
                }
          }
     
              //2b. Move each pair of nodes n1, n2 generated by triangle t towards each other by one third of the segment length (to make sure the triangulation generates everything).
          foreach(RMDeformableTriangle t in intersectingTriangles)
          {
                ATVector3f segment = t.mostRecentlyGeneratedNodes[1].pos - t.mostRecentlyGeneratedNodes[0].pos;
                //Debug.Log(segment);
                //Here, we expect them all to have a magnitude of .5
                t.mostRecentlyGeneratedNodes[0].pos += segment / 3.0f;
                t.mostRecentlyGeneratedNodes[1].pos -= segment / 3.0f;
          }
     
              //2c. Delete all consecutive nodes on the formed cycles that refer to the same surface vertex.
          //std::vector<SubDigraph> tightenedCycles;
          List<SubDigraph> tightenedCycles = new List<SubDigraph>();
          foreach(SubDigraph cycle in cycles)
          {
                 //First make sure we're not generating an unnecessary face of all corner nodes
                bool allCornerNodes = true;
                bool allExternalCorners = false;
                foreach(DigraphNode node in cycle.nodes)
                {
                      if(node.genType != DigraphNode.GenType.CORNER)
                      {
                            allCornerNodes = false;
                            break;
                      }
                }
                if(allCornerNodes)
                {
                      allExternalCorners = true;
                      for(int i = 0; i < 4; i++)
                      {
                            if(face.corners[i].inMesh == true)
                            {
                                  allExternalCorners = false;
                                  break;
                            }
                      }
                }
                if(!allExternalCorners)
                {
                      touch++;
     
                      SubDigraph tightenedCycle = new SubDigraph();
                      DigraphNode current = cycle.nodes[0];
                      while(current.touch != touch)
                      {
                            DigraphNode next = current.outgoing.b;

                            /*if (current.genType == DigraphNode.GenType.GENERATED_BY_FACE_EDGE
                                && current.generatingTriangle.ownerCell != c1 && current.generatingTriangle.ownerCell != c2)
                            {
                                current.incoming.b = next;
                                next.incoming = current.incoming;
                            } else*/
                            if(next.assignedVertex != current.assignedVertex || next.genType == DigraphNode.GenType.CORNER)
                            {
                                  tightenedCycle.nodes.Add(next);
                            }
                            current.touch = touch;
                            current = next;
                      }
                      tightenedCycles.Add(tightenedCycle);
                }
          }
          //Debug.Log(tightenedCycles.Count);
            //Count should be 1, discard the outer cycle since all are external
          //Debug.Log(tightenedCycles[0].nodes.Count);
        //And that one inner cycle should have 8 vertices
     
          List<DigraphNode> finalNodes = new List<DigraphNode>();
     
              //2d. Use the original locations of the nodes on f to compute a 2D trianglulation using ear-cutting.
           //Do ear cutting
          //List<ATVector2f> surface;
          List<RMDeformableTriangle> newSurfaceTriangles = new List<RMDeformableTriangle>();
          foreach(SubDigraph cycle in tightenedCycles)
          {
                List<ATVector2f> projectedContour = new List<ATVector2f>();
                List<int> surfaceTriangles = new List<int>();
     
                int zxcv = 0;
                foreach(DigraphNode node in cycle.nodes)
                {
                    //Debug.Log(node.assignedVertex.unityIndex);
                      ATVector2f pos2d = ATVector2f.ZERO;
                      if(offset.x != 0)
                            pos2d = new ATVector2f(node.pos.y, node.pos.z);
                      else if(offset.y != 0)
                            pos2d = new ATVector2f(node.pos.z, node.pos.x);
                      else if(offset.z != 0)
                            pos2d = new ATVector2f(node.pos.x, node.pos.y);
     
                      projectedContour.Add(pos2d);
                      //Debug.Log(zxcv + ": " + pos2d);
                      zxcv++;
                }
                //projectedContour.Reverse();
     
                 //Do the ear cutting on the 2d face f
                EarCutting.Process(projectedContour, ref surfaceTriangles);
                /*for (int uiop = 0; uiop < surfaceTriangles.Count; uiop+=3)
                {
                    Debug.Log(surfaceTriangles[uiop] + " " + surfaceTriangles[uiop + 1] + " " + surfaceTriangles[uiop + 2]);
                }*/
                //Debug.Log(surfaceTriangles.Count/3);
                    //2e. Span the generated triangles across the assigned surface vertices.
                 //Add the generated triangles, spanned across their assigned vertices, to the exposed surface submesh.
                double minCubeSide = Math.Min(spacing.x, Math.Min(spacing.y, spacing.z));
                for(int i = 0; i < surfaceTriangles.Count; i += 3)
                {
                    //Mark: I changed the order of the nodes here.  Unity is stupid and uses a left handed coordinate system
                    //and thus wants clockwise triangles and not CCW like the rest of the world >_<
                      DigraphNode[] n = new DigraphNode[3];
                      n[0] = cycle.nodes[surfaceTriangles[i]];
                      n[1] = cycle.nodes[surfaceTriangles[i+1]];
                      n[2] = cycle.nodes[surfaceTriangles[i+2]];
     
                      finalNodes.Add(n[0]);
                      finalNodes.Add(n[1]);
                      finalNodes.Add(n[2]);
     
                      RMDeformableTriangle t = new RMDeformableTriangle();
                      t.mesh = this;
                      t.vI[0] = n[0].assignedVertex;
                      t.vI[1] = n[1].assignedVertex;
                      t.vI[2] = n[2].assignedVertex;

                      ATVector3f triNormal = (t.vI[1].p-t.vI[0].p).CrossProduct(t.vI[2].p-t.vI[0].p);

                      t.vI[0].n = triNormal;// new ATVector3f(offset[0], offset[1], offset[2]);
                      t.vI[1].n = triNormal;// new ATVector3f(offset[0], offset[1], offset[2]);
                      t.vI[2].n = triNormal;// new ATVector3f(offset[0], offset[1], offset[2]);
     
                      // Get the fracture capping texture coordinates by projecting down an endless loop of the texture where it tiles every minCubeSide
                      for(int j = 0; j < 3; j++)
                      {
                            RMDeformableVertexInfo vI = t.vI[j];
                            ATVector2f relevantCoords = ATVector2f.ZERO;
                            if(offset.x != 0)
                                  relevantCoords = new ATVector2f(vI.p.y, vI.p.z);
                            else if(offset.y != 0)
                                  relevantCoords = new ATVector2f(vI.p.z, vI.p.x);
                            else if(offset.z != 0)
                                  relevantCoords = new ATVector2f(vI.p.x, vI.p.y);

                            t.textureCoordinates[j] = (relevantCoords / (float)minCubeSide) / 10;
                            t.textureCoordinates2[j] = (relevantCoords / (float)minCubeSide) / 10;
                      }
     
                      t.ownerCell = c1;
                      t.FindOrGenerateVerticesFracturing();
                      t.submesh = exposedSurface;
                      t.submesh.triangles.Add(t);
                      t.submesh.recentlyCreatedTriangles.Add(t);
                      newSurfaceTriangles.Add(t);
                }
          }

          //finalNodes.Sort();
        //Got the same result from sort by index, check next item
        //nlogn vs n^2, can we get away with it justifiably?
          //Debug.Log(finalNodes.Count);
          //List<DigraphNode> tempNodes = new List<DigraphNode>();
        //Unity doesn't support HashSet =(
          Dictionary<DigraphNode,bool> tempNodes = new Dictionary<DigraphNode,bool>();
          //Keep only unique elements from a sorted list.
          /*for (int i = 0; i < finalNodes.Count; ) {
              DigraphNode first = finalNodes[i];
              i++;
              while(i < finalNodes.Count) {
                  DigraphNode second = finalNodes[i];
                  if (first == second) {
                      i++;
                  } else {
                      break;
                  }
              }
              tempNodes.Add(first);
          }*/

          //int unityIndex = mesh.vertices.Length;
          //Debug.Log(mesh.vertexCount);
          //Debug.Log(mesh.vertices.Length);
          /*for (int i = 0; i < finalNodes.Count; i++)
          {
              DigraphNode first = finalNodes[i];
              tempNodes2[first] = true;
              bool failed = false;
              for (int j = i+1; j < finalNodes.Count; j++)
              {
                  DigraphNode second = finalNodes[j];
                  if (first == second)
                  {
                      failed = true;
                      //first.assignedVertex.unityIndex = unityIndex;
                      break;
                  }
              }
              if (!failed)
              {
                  //first.assignedVertex.unityIndex = unityIndex;
                  //unityIndex++;
                  tempNodes.Add(first);
              }
          }*/
          //Debug.Log(tempNodes.Count + " " + tempNodes2.Count + " " + finalNodes.Count);


          finalNodes = new List<DigraphNode>(tempNodes.Keys);
          //tempNodes.Keys.CopyTo(finalNodes,0);
          //Debug.Log(finalNodes.Count);

          //float f2 = 1;
     
          ////if(newSurfaceTriangles.Count == 0)
          ////if((c1 == lattice[0][1][1] && c2 == lattice[1][1][1]) || (c1 == lattice[1][1][1] && c2 == lattice[0][1][1]))
          //if(false)
          //{
          //      static bool rendered = false;
          //      if(rendered == false)
          //      {
          //            rendered = true;
          //            faceGraphToRender = faceGraph;
          //      }
          //      int num = tightenedCycles.size();
          //      for each(SubDigraph d in tightenedCycles)
          //      {
          //            float f = 1;
          //      }
          //}
     
              //2f. For each generated triangle t, create a duplicate triangle t' with reversed orientation. Assign t to c1 and t' to c2.
          foreach(RMDeformableTriangle t in newSurfaceTriangles)
          {
              //Debug.Log("NEWTRI");
                RMDeformableTriangle t2 = new RMDeformableTriangle();
                t2.mesh = this;
                for(int i = 0; i < 3; i++)
                {
                      t2.vI[i] = t.vI[2-i];
                      t2.vI[i].n = -t2.vI[i].n;
                      t2.textureCoordinates[i] = t.textureCoordinates[2 - i];
                      t2.textureCoordinates2[i] = t.textureCoordinates2[2 - i];
                }
                t2.ownerCell = c2;
                t2.FindOrGenerateVerticesFracturing();
                t2.submesh = exposedSurface;
                t2.submesh.triangles.Add(t2);
                t2.submesh.recentlyCreatedTriangles.Add(t2);
          }

        //Debug.Log(mesh.vertexCount);
       // Debug.Log(mesh.vertices.Length);

        //Okay, so we've got our final nodes and interior triangles.
        //First, how do we draw them?
        //Second, wtf about cutting the triangles that are already there?
     
           //Redo position arbiters for dirty vertices
           //HACK: Should do this only for dirty vertices
           //TODO: Make this only for dirty vertices
          ////foreach(Vertex *v in vertices)
          ////{
          ////    v.DeterminePositionArbiter();
          ////}
           //Now, what vertices could possibly be affected by this?
           //Check all cells adjacent to this face.
    }
}