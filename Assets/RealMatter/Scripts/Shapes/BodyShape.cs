using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Soft Body Physics/Shapes/Empty")]
public abstract class BodyShape : MonoBehaviour {

    public abstract List<Vector3> getLatticePoints();
    public abstract Vector3 getCenter();
    public virtual void finishInitialization(RMBody b) {;}
    public virtual void updateMesh() {;}
}
