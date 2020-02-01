using UnityEngine;
using System.Collections;

public class FractureParticles : FractureScript {

    public GameObject particleEffect;

    public override void OnFracture(RMBody.FractureInfo info)
    {
        //Vector3 pos = transform.TransformPoint((info.a.x - info.b.x)/2.0 - Vector3.Scale(GetComponent<BodyScript>().shape.getCenter(), GetComponent<BodyScript>().spacing));
        GameObject o = (GameObject)Instantiate(particleEffect);
        o.transform.position = (info.a.x + info.b.x) / 2.0;
        //Debug.Log("GOGOGO");
    }
}
