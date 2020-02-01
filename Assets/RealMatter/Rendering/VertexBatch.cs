using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RMVertexBatch
{
    RMDeformableSubmesh submesh;
    List<RMParticle> particles;
    List<RMDeformableVertex> vertices;
    List<ATVector2f> textureCoordinates;
    List<int> vertexParticleIndices;
}