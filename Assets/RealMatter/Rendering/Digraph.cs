using System.Collections.Generic;
using System;

public class DigraphNode {//: IComparable<DigraphNode>{
    public DigraphEdge incoming = null;
    public DigraphEdge outgoing = null;
    public RMDeformableTriangle generatingTriangle;
    public RMDeformableCellCorner generatingCorner;
    public enum GenType { GENERATED_BY_TRIANGLE_EDGE, GENERATED_BY_FACE_EDGE, CORNER };
    public GenType genType;
    public RMDeformableVertexInfo v1, v2;
    public ATVector3f pos;
    public RMDeformableVertexInfo assignedVertex;
    public int touch = -1;

/*    public int CompareTo(DigraphNode obj)
    {
        if (pos.x < obj.pos.x)
        {
            return -1;
        }
        else if (pos.x > obj.pos.x)
        {
            return 1;
        }
        else
        {
            if (pos.y < obj.pos.y)
            {
                return -1;
            }
            else if (pos.y > obj.pos.y)
            {
                return 1;
            }
            else
            {
                if (pos.z < obj.pos.z)
                {
                    return -1;
                }
                else if (pos.z > obj.pos.z)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }*/
}

public class DigraphEdge
{
    public DigraphNode a, b;
}

public class Digraph {
    public List<DigraphNode> nodes = new List<DigraphNode>();
    public List<DigraphEdge> edges = new List<DigraphEdge>();
}

public class SubDigraph
{
    public List<DigraphNode> nodes = new List<DigraphNode>();
    public List<DigraphEdge> edges = new List<DigraphEdge>();
}