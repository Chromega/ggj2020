using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(BodyScript))]

public class BodyEditor : Editor
{
    private bool fracturingFoldout = false;
    private bool spacingFoldout = false;
    private bool cellFoldout = false;
    private bool particleFoldout = false;
    private bool sleepFoldout = false;
    private bool collisionFoldout = false;
    private int DEFAULT_INDENT = 16;

    public override void OnInspectorGUI()
    {
        BodyScript selected = (BodyScript)target;
        if (selected == null) return;

        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("Rebuild Body"))
            UpdateBody();

        //General lattice properties
        GUIBeginIndent(DEFAULT_INDENT);
            //selected.shape = (BodyShape)EditorGUILayout.ObjectField("Shape Script", selected.shape, typeof(BodyShape));
            selected.w = EditorGUILayout.IntField("w: Object Rigidity", selected.w);
            EnsureNonnegative(ref selected.w);
            selected.alpha = EditorGUILayout.FloatField("alpha: Object Restitution", selected.alpha);
            EnsureNonnegative(ref selected.alpha);
        GUIEndIndent();

        //Spacing Foldout
        GUIBeginIndent(spacingFoldout ? 11 : 14);
            spacingFoldout = EditorGUILayout.Foldout(spacingFoldout, "Spacing: " + selected.spacing);
            if (spacingFoldout)
            {
                GUIBeginIndent(DEFAULT_INDENT);
                    selected.spacing.x = EditorGUILayout.FloatField("X", selected.spacing.x);
                    EnsureNonnegative(ref selected.spacing.x);
                    selected.spacing.y = EditorGUILayout.FloatField("Y", selected.spacing.y);
                    EnsureNonnegative(ref selected.spacing.y);
                    selected.spacing.z = EditorGUILayout.FloatField("Z", selected.spacing.z);
                    EnsureNonnegative(ref selected.spacing.z);
                GUIEndIndent();
            }
        GUIEndIndent();

        //Damping
        GUIBeginIndent(DEFAULT_INDENT);
            selected.regionDamping = EditorGUILayout.FloatField("Region Damping", selected.regionDamping);
            EnsureNonnegative(ref selected.regionDamping);
            selected.chunkDamping = EditorGUILayout.FloatField("Chunk Damping", selected.chunkDamping);
            EnsureNonnegative(ref selected.chunkDamping);
        GUIEndIndent();

        //Fracturing foldout
        GUIBeginIndent(fracturingFoldout ? 11 : 14);
            fracturingFoldout = EditorGUILayout.Foldout(fracturingFoldout, selected.fracturing?"Fracturing (enabled)":"Fracturing (disabled)");
            if (fracturingFoldout)
            {
                GUIBeginIndent(DEFAULT_INDENT);
                    selected.fracturing = EditorGUILayout.BeginToggleGroup("Enable Fracturing", selected.fracturing);
                    selected.fractureDistanceTolerance = EditorGUILayout.FloatField("Fracture Distance Tolerance", selected.fractureDistanceTolerance);
                    EnsureNonnegative(ref selected.fractureDistanceTolerance);
                    selected.fractureRotationTolerance = EditorGUILayout.FloatField("Fracture Rotation Tolerance", selected.fractureRotationTolerance);
                    EnsureNonnegative(ref selected.fractureRotationTolerance);
                    selected.fractureMaterial = (Material)EditorGUILayout.ObjectField("Fracture Material", selected.fractureMaterial, typeof(Material));
                    selected.fractureScript = (FractureScript)EditorGUILayout.ObjectField("Fracture Script", selected.fractureScript, typeof(FractureScript));
                    EditorGUILayout.EndToggleGroup();
                GUIEndIndent();
            }
        GUIEndIndent();

        //Sleep foldout
        GUIBeginIndent(sleepFoldout ? 11 : 14);
            sleepFoldout = EditorGUILayout.Foldout(sleepFoldout, selected.useSleep ? "Sleeping (enabled)" : "Sleeping (disabled)");
            if (sleepFoldout)
            {
                GUIBeginIndent(DEFAULT_INDENT);
                selected.useSleep = EditorGUILayout.BeginToggleGroup("Enable Sleeping", selected.useSleep);
                selected.sleepRadius = EditorGUILayout.FloatField("Distance which triggers sleep", selected.sleepRadius);
                EnsureNonnegative(ref selected.sleepRadius);
                selected.wakeRadius = EditorGUILayout.FloatField("Distance which triggers wake", selected.wakeRadius);
                EnsureNonnegative(ref selected.wakeRadius);
                EditorGUILayout.EndToggleGroup();
                GUIEndIndent();
            }
            GUIEndIndent();

        //Particle foldout
        GUIBeginIndent(particleFoldout ? 11 : 14);
            particleFoldout = EditorGUILayout.Foldout(particleFoldout, "Particle Properties");
            if (particleFoldout)
            {
                GUIBeginIndent(DEFAULT_INDENT);
                selected.particlePrefab = (GameObject)EditorGUILayout.ObjectField("Particle Prefab", selected.particlePrefab, typeof(GameObject));
                float newScale = EditorGUILayout.FloatField("Particle Scale", selected.extraParticleScale);
                EnsureNonnegative(ref newScale);
                if (newScale != selected.extraParticleScale)
                {
                    selected.extraParticleScale = newScale;
                    RescaleCollisionGeometry();
                }
                selected.particleMassScale = EditorGUILayout.FloatField("Mass Scale", selected.particleMassScale);
                EnsureNonnegative(ref selected.particleMassScale);
                selected.particleMesh = (Mesh)EditorGUILayout.ObjectField("Particle Mesh", selected.particleMesh, typeof(Mesh));

                EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Make Particles Visible"))
                    {
                        if (selected.particleMesh != null)
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
                    if (GUILayout.Button("Make Particles Invisible"))
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
                EditorGUILayout.EndHorizontal();
                GUIEndIndent();
            }
        GUIEndIndent();
        
        //Cell foldout
        GUIBeginIndent(cellFoldout ? 11 : 14);
            cellFoldout = EditorGUILayout.Foldout(cellFoldout, "Cell Properties");
            if (cellFoldout)
            {
                GUIBeginIndent(DEFAULT_INDENT);
                selected.cellPrefab = (GameObject)EditorGUILayout.ObjectField("Cell Prefab", selected.cellPrefab, typeof(GameObject));
                GUIEndIndent();
            }
        GUIEndIndent();

        //Collision foldout
        GUIBeginIndent(collisionFoldout ? 11 : 14);
            collisionFoldout = EditorGUILayout.Foldout(collisionFoldout, "Collision Properties");
            if (collisionFoldout)
            {
                GUIBeginIndent(DEFAULT_INDENT);
                if (GUILayout.Button("Trim Collision Boundary"))
                    TrimCollisionBoundary();
                if (GUILayout.Button("Rebuild Collision Boundary"))
                    RebuildCollisionBoundary();
                if (GUILayout.Button("Magic!"))
                    Magic();


                int newNumColliders = EditorGUILayout.IntField("Ignored Collider Count", selected.collidersToIgnore.Count);
                EnsureNonnegative(ref newNumColliders);
                if (newNumColliders > selected.collidersToIgnore.Count)
                {
                    List<GameObject> newColliders = new List<GameObject>();
                    newColliders.InsertRange(0, selected.collidersToIgnore);
                    while (newColliders.Count < newNumColliders)
                    {
                        newColliders.Add(null);
                    }
                    selected.collidersToIgnore = newColliders;
                }
                else if (newNumColliders < selected.collidersToIgnore.Count)
                {
                    List<GameObject> newColliders = new List<GameObject>();
                    for (int i = 0; i < newNumColliders; i++)
                    {
                        newColliders.Add(selected.collidersToIgnore[i]);
                    }
                    selected.collidersToIgnore = newColliders;
                }

                for (int i = 0; i < selected.collidersToIgnore.Count; i++)
                {
                    GameObject ignoredCollider = selected.collidersToIgnore[i];
                    GameObject newIgnoredCollider;
                    newIgnoredCollider = (GameObject)EditorGUILayout.ObjectField("Ignored Collider " + i, ignoredCollider, typeof(GameObject));
                    if (newIgnoredCollider != ignoredCollider)
                    {
                        selected.collidersToIgnore[i] = newIgnoredCollider;
                    }
                }


                GUIEndIndent();
            }
        
            EditorGUILayout.LabelField("Particle Count", ""+selected.particleScripts.Count);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Export Body Data"))
                ExportBodyData();

            selected.exportName = EditorGUILayout.TextField(selected.exportName);
            
            EditorGUILayout.EndHorizontal();
        GUIEndIndent();


        EditorGUILayout.EndVertical();

        addBodyShape();

        removeColliders();
    }

    void ExportBodyData()
    {
        BodyScript selected = (BodyScript)target;
        string text = "";
        Vector3 center = selected.GetComponent<BodyShape>().getCenter();
        text += center[0] + "," + center[1] + "," + center[2] + "\n";
        text += selected.spacing[0] + "," + selected.spacing[1] + "," + selected.spacing[2] + "\n";
        text += selected.transform.localScale[0] + "," + selected.transform.localScale[1] + "," + selected.transform.localScale[2] + "\n";
        foreach (ParticleScript p in selected.particleScripts)
        {
            ATPoint3 index = p.index;
            text += index[0] + "," + index[1] + "," + index[2] + "\n";
        }
        TextWriter tw = new StreamWriter("Assets\\Lattices\\" + selected.exportName + ".rmb");
        tw.Write(text);
        tw.Close();
    }

    void addBodyShape()
    {
        BodyScript selected = (BodyScript)target;
        GameObject body = selected.gameObject;
        if (body.GetComponent<BodyShape>() == null)
        {
            //BodyShape shape;
            if (body.GetComponent<MeshFilter>() == null)
                body.AddComponent<BoxShape>();
            else
                body.AddComponent<MeshShape>();
            //selected.shape = shape;
        }
    }

    void removeColliders()
    {
        BodyScript selected = (BodyScript)target;
        GameObject body = selected.gameObject;
        if (!Application.isPlaying)
        {
            if (body.GetComponent<Collider>() != null)
            {
                DestroyImmediate(body.GetComponent<Collider>());
            }
            if (body.GetComponent<Rigidbody>() != null)
            {
                DestroyImmediate(body.GetComponent<Rigidbody>());
            }
        }
    }

    void TrimCollisionBoundary()
    {
        BodyScript selected = (BodyScript)target;
        foreach (ParticleScript p in selected.particleScripts) {
            if (p.numConnectedCells != 8) {
                DestroyImmediate(p.gameObject.GetComponent<Collider>());
            }
        }
    }

    void RebuildCollisionBoundary()
    {
        BodyScript selected = (BodyScript)target;
        Collider prefabCollider = selected.particlePrefab.GetComponent<Collider>();
        //I'd really like to believe there's a nicer way to do this.
        if (prefabCollider is SphereCollider)
        {
            SphereCollider prefabColliderSphere = prefabCollider as SphereCollider;
            foreach (ParticleScript p in selected.particleScripts)
            {
                SphereCollider collider;
                if (p.gameObject.GetComponent<Collider>() == null)
                {
                    collider = p.gameObject.AddComponent<SphereCollider>();
                } else {
                    collider = p.gameObject.GetComponent<Collider>() as SphereCollider;
                }
                collider.transform.localScale = selected.extraParticleScale*Vector3.Scale(prefabColliderSphere.transform.localScale, selected.spacing);
                collider.GetComponent<Rigidbody>().mass = prefabColliderSphere.GetComponent<Rigidbody>().mass;
                collider.material = prefabColliderSphere.material;
                collider.radius = prefabColliderSphere.radius;
                collider.center = prefabColliderSphere.center;
            }
        }
        else if (prefabCollider is BoxCollider)
        {
            BoxCollider prefabColliderBox = prefabCollider as BoxCollider;
            BoxCollider collider;
            foreach (ParticleScript p in selected.particleScripts)
            {
                if (p.gameObject.GetComponent<Collider>() == null)
                {
                    collider = p.gameObject.AddComponent<BoxCollider>();
                }
                else
                {
                    collider = p.gameObject.GetComponent<Collider>() as BoxCollider;
                }
                collider.transform.localScale = selected.extraParticleScale * Vector3.Scale(prefabColliderBox.transform.localScale, selected.spacing);
                collider.GetComponent<Rigidbody>().mass = prefabColliderBox.GetComponent<Rigidbody>().mass;
                collider.material = prefabColliderBox.material;
                collider.size = prefabColliderBox.size;
                collider.center = prefabColliderBox.center;
            }
        }
        else {
            Debug.Log("Non-sphere or box particle regeneration isn't implemented yet.  Tell Mark to do it.");
        }
    }

    /**
     * Since I don't yet have a completely general purpose way to do manipulations on the body, do special case stuff here
     */
    void Magic()
    {
        BodyScript selected = (BodyScript)target;
        foreach (ParticleScript p in selected.particleScripts)
        {
            if (p.gameObject.transform.localPosition.y < -2)
            {
                DestroyImmediate(p.gameObject.GetComponent<Collider>());
            }
        }
    }

    void RescaleCollisionGeometry() {
        BodyScript selected = (BodyScript)target;
        Collider prefabCollider = selected.particlePrefab.GetComponent<Collider>();
        foreach (ParticleScript p in selected.particleScripts)
        {
            if (p.gameObject.GetComponent<Collider>() != null)
            {
                if (p.gameObject.GetComponent<Collider>() is SphereCollider)
                {
                    (p.gameObject.GetComponent<Collider>() as SphereCollider).radius = selected.extraParticleScale * (prefabCollider as SphereCollider).radius;
                }
                //p.gameObject.collider.transform.localScale = selected.extraParticleScale * Vector3.Scale(prefabCollider.transform.localScale, selected.spacing);
                //p.gameObject.transform.localScale = selected.extraParticleScale * Vector3.Scale(prefabCollider.transform.localScale, selected.spacing);
                if (p.gameObject.GetComponent<Collider>() is BoxCollider)
                {
                    (p.gameObject.GetComponent<Collider>() as BoxCollider).size = selected.extraParticleScale * (prefabCollider as BoxCollider).size;
                }
            }
        }
    }

    void UpdatePositions()
    {
        BodyScript selected = (BodyScript)target;
        //CellScript[] cellScripts = selected.GetComponentsInChildren<CellScript>();
        //foreach (CellScript j in cellScripts)
        //{
        //    ;// j.FixedUpdate();
        //}
        ParticleScript[] particleScripts = selected.GetComponentsInChildren<ParticleScript>();
        foreach (ParticleScript i in particleScripts)
        {
            i.BeforeBodySim();
            //i.RefreshStartPosition();
        }
    }

    void UpdateBody()
    {
        BodyScript selected = (BodyScript)target;
        selected.RebuildBody();
        UpdatePositions();
    }

    void GUIBeginIndent(int indent)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(indent);
        GUILayout.BeginVertical();
    }

    void GUIEndIndent()
    {
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    void EnsureNonnegative(ref int x)
    {
        if (x < 0)
        {
            x = 0;
        }
    }

    void EnsureNonnegative(ref float x)
    {
        if (x < 0.0f)
        {
            x = 0.0f;
        }
    }
}
