using UnityEngine;
using System.Collections;

public abstract class FractureScript : MonoBehaviour {

    public abstract void OnFracture(RMBody.FractureInfo info);
}
