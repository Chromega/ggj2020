using UnityEngine;
using UnityEditor;

using System.Collections;

public class ParticleWindow : EditorWindow
{

    public float particleScale = 1.0f;

    [MenuItem("Window/Soft Body Particle Window")]
    [MenuItem("Component/Soft Body Physics/Soft Body Particle Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<ParticleWindow>();
    }

    public void OnGUI()
    {
        if (GUILayout.Button("Remove Colliders"))
            RemoveColliders();
        if (GUILayout.Button("Restore Colliders"))
            RestoreColliders();
        if (GUILayout.Button("Remove Fixed Joints"))
            RemoveFixedJoints();

        float newScale = EditorGUILayout.FloatField("Particle Scale", particleScale);
        EnsureNonnegative(ref newScale);
        if (newScale != particleScale)
        {
            particleScale = newScale;
            RescaleCollisionGeometry();
        }

        if (GUILayout.Button("Make Particles Visible"))
            MakeParticlesVisible();
        if (GUILayout.Button("Make Particles Invisible"))
            MakeParticlesInvisible();
    }

    void MakeParticlesVisible()
    {
        GameObject[] objects = Selection.gameObjects;
        foreach (GameObject o in objects)
        {
            BodyScript selected = o.GetComponent<BodyScript>();
            if (selected != null && selected.particleMesh != null)
            {
                foreach (ParticleScript ps in selected.particleScripts)
                {
                    MeshFilter mf = ps.GetComponent<MeshFilter>();
                    if (mf == null)
                    {
                        mf = ps.gameObject.AddComponent<MeshFilter>();
                        mf.mesh = selected.particleMesh;
                        /*Vector3[] vertices = mf.mesh.vertices;
                        Vector3[] newVertices = new Vector3[vertices.Length];
                        for (int i = 0; i < vertices.Length; i++ )
                        {
                            newVertices[i] = Vector3.Scale(vertices[i], selected.spacing);
                        }
                        mf.mesh.vertices = newVertices;*/
                    }
                    MeshRenderer mr = ps.GetComponent<MeshRenderer>();
                    if (mr == null)
                        ps.gameObject.AddComponent<MeshRenderer>();
                }
            }
        }
    }

    void MakeParticlesInvisible()
    {
        GameObject[] objects = Selection.gameObjects;
        foreach (GameObject o in objects)
        {
            BodyScript selected = o.GetComponent<BodyScript>();
            if (selected != null)
            {
                foreach (ParticleScript ps in selected.particleScripts)
                {
                    MeshFilter mf = ps.GetComponent<MeshFilter>();
                    if (mf != null)
                        DestroyImmediate(mf);
                    MeshRenderer mr = ps.GetComponent<MeshRenderer>();
                    DestroyImmediate(mr);
                }
            }
        }
    }


    void RescaleCollisionGeometry()
    {
        GameObject[] objects = Selection.gameObjects;
        foreach (GameObject o in objects)
        {
            ParticleScript p = o.GetComponent<ParticleScript>();
            if (p != null && p.gameObject.GetComponent<Collider>() != null)
            {

                BodyScript bs = p.transform.parent.parent.GetComponent<BodyScript>();
                Collider prefabCollider = bs.particlePrefab.GetComponent<Collider>();

                if (p.gameObject.GetComponent<Collider>() is SphereCollider)
                {
                    (p.gameObject.GetComponent<Collider>() as SphereCollider).radius = particleScale * (prefabCollider as SphereCollider).radius;
                }
                //p.gameObject.collider.transform.localScale = selected.extraParticleScale * Vector3.Scale(prefabCollider.transform.localScale, selected.spacing);
                //p.gameObject.transform.localScale = selected.extraParticleScale * Vector3.Scale(prefabCollider.transform.localScale, selected.spacing);
                if (p.gameObject.GetComponent<Collider>() is BoxCollider)
                {
                    (p.gameObject.GetComponent<Collider>() as BoxCollider).size = particleScale * (prefabCollider as BoxCollider).size;
                }
            }
            BodyScript selected = o.GetComponent<BodyScript>();
            if (selected != null)
            {
                Collider prefabCollider = selected.particlePrefab.GetComponent<Collider>();
                selected.extraParticleScale = particleScale;
                foreach (ParticleScript ps in selected.particleScripts)
                {
                    if (ps.gameObject.GetComponent<Collider>() != null)
                    {
                        if (ps.gameObject.GetComponent<Collider>() is SphereCollider)
                        {
                            (ps.gameObject.GetComponent<Collider>() as SphereCollider).radius = selected.extraParticleScale * (prefabCollider as SphereCollider).radius;
                        }
                        //p.gameObject.collider.transform.localScale = selected.extraParticleScale * Vector3.Scale(prefabCollider.transform.localScale, selected.spacing);
                        //p.gameObject.transform.localScale = selected.extraParticleScale * Vector3.Scale(prefabCollider.transform.localScale, selected.spacing);
                        if (ps.gameObject.GetComponent<Collider>() is BoxCollider)
                        {
                            (ps.gameObject.GetComponent<Collider>() as BoxCollider).size = selected.extraParticleScale * (prefabCollider as BoxCollider).size;
                        }
                    }
                }
            }
        }
    }

    void RemoveColliders()
    {
        GameObject[] objects = Selection.gameObjects;
        foreach (GameObject o in objects)
        {
            ParticleScript p = o.GetComponent<ParticleScript>();
            if (p != null && p.numConnectedCells != 8)
            {
                DestroyImmediate(p.gameObject.GetComponent<Collider>());
            }
        }
    }
    void RestoreColliders()
    {

        GameObject[] objects = Selection.gameObjects;
        foreach (GameObject o in objects)
        {
            ParticleScript p = o.GetComponent<ParticleScript>();
            if (p == null) continue;
            BodyScript selected = p.transform.parent.parent.gameObject.GetComponent<BodyScript>();
            Collider prefabCollider = selected.particlePrefab.GetComponent<Collider>();
            //I'd really like to believe there's a nicer way to do this.
            if (prefabCollider is SphereCollider)
            {
                SphereCollider prefabColliderSphere = prefabCollider as SphereCollider;
                SphereCollider collider;
                if (p.gameObject.GetComponent<Collider>() == null)
                {
                    collider = p.gameObject.AddComponent<SphereCollider>();
                }
                else
                {
                    collider = p.gameObject.GetComponent<Collider>() as SphereCollider;
                }
                //collider.transform.localScale = selected.extraParticleScale * Vector3.Scale(prefabColliderSphere.transform.localScale, selected.spacing);
                collider.GetComponent<Rigidbody>().mass = prefabColliderSphere.GetComponent<Rigidbody>().mass;
                collider.material = prefabColliderSphere.material;
                collider.radius = prefabColliderSphere.radius;
                collider.center = prefabColliderSphere.center;
            }
            else if (prefabCollider is BoxCollider)
            {
                BoxCollider prefabColliderBox = prefabCollider as BoxCollider;
                BoxCollider collider;
                if (p.gameObject.GetComponent<Collider>() == null)
                {
                    collider = p.gameObject.AddComponent<BoxCollider>();
                }
                else
                {
                    collider = p.gameObject.GetComponent<Collider>() as BoxCollider;
                }
                //collider.transform.localScale = selected.extraParticleScale * Vector3.Scale(prefabColliderBox.transform.localScale, selected.spacing);
                collider.GetComponent<Rigidbody>().mass = prefabColliderBox.GetComponent<Rigidbody>().mass;
                collider.material = prefabColliderBox.material;
                collider.size = prefabColliderBox.size;
                collider.center = prefabColliderBox.center;
            }
            else
            {
                Debug.Log("Non-sphere or box particle regeneration isn't implemented yet.  Tell Mark to do it.");
            }
        }
    }
    void RemoveFixedJoints()
    {
        GameObject[] objects = Selection.gameObjects;
        foreach (GameObject o in objects)
        {
            ParticleScript p = o.GetComponent<ParticleScript>();
            if (p)
            {
                FixedJoint j = o.GetComponent<FixedJoint>();
                if (j)
                    DestroyImmediate(j);
            }
        }
    }

    void EnsureNonnegative(ref float x)
    {
        if (x < 0.0f)
        {
            x = 0.0f;
        }
    }

    void EnsureNonnegative(ref int x)
    {
        if (x < 0)
        {
            x = 0;
        }
    }
}