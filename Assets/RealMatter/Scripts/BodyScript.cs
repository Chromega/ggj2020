//#define PARALLEL_SIMULATION

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[AddComponentMenu("Soft Body Physics/Soft Body")]
[ExecuteInEditMode]
public class BodyScript : MonoBehaviour
{

    public RMBody body;
    public List<ParticleScript> particleScripts = new List<ParticleScript>();
    //note excess below?
    private List<RMParticle> rmParticles = new List<RMParticle>();
    private List<GameObject> particles = new List<GameObject>();
    private GameObject physicsNode = null;

    //Public vars exposed to Unity editor
    //Body Shape
    private BodyShape shape;

    public FractureScript fractureScript;
    //Velocity
    public Vector3 velocity = new Vector3(0, 0, 0);
    //Which object will be used as particles?
    public GameObject particlePrefab;
    //What do we use when we want to see the particles for debugging?
    public Mesh particleMesh;
    //Which object will be used as cells?
    public GameObject cellPrefab;
    //What is the spacing between particles
    public Vector3 spacing = new Vector3(1, 1, 1);
    //How rigid is the object?
    public int w = 1;
    //Can the object fracture?
    public bool fracturing = false;
    //Fracture tolerance
    public float fractureDistanceTolerance = 999.0f;
    //Fracture rotation tolerance
    public float fractureRotationTolerance = 0.6f;
    //Damping
    public float regionDamping = .02f;
    //Damping
    public float chunkDamping = 0.0f;
    //Restoring
    public float alpha = .75f;
    //Mass
    public float particleMassScale = 1.0f;

    public bool useSleep = false;

    public float sleepRadius = 50;

    public float wakeRadius = 40;

    public bool isAsleep = false;

    private Vector3 scale; //Remember our scale, because the transform will drop it
    //Is this an in-game construction, or one triggered by the editor?
    private bool editorReconstruction = false;

    public float extraParticleScale = 1.0f;

    private Mesh mesh;

    public bool hasFractured = false;

    public Material fractureMaterial;

    //It'd be nice to support more than one, but not sure how to get a list to work in Unity editor
    public List<GameObject> collidersToIgnore = new List<GameObject>();

    /**
     * The name of the output file when using Maya animation tools
     */
    [HideInInspector]
    public string exportName = "untitled";

    public void RebuildBody()
    {
        editorReconstruction = true;
        body = null;
        particleScripts.Clear();
        particles.Clear();
        rmParticles.Clear();

        //This dance here destroys the child cells and particles
        //Latches onto the objects, unparents, then destroys
        List<GameObject> children = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i).gameObject);
        }
        transform.DetachChildren();
        foreach (GameObject child in children)
        {
            DestroyImmediate(child, true);
        }
        physicsNode = null;
        Start();
        editorReconstruction = false;
    }

    void Start()
    {
        //If we're in the game, build away!
        if (this != null && this.body == null)
        {
            CreateBody();
        }
    }

    public void CreateBody()
    {
        shape = GetComponent<BodyShape>();

        particleScripts = new List<ParticleScript>();
        rmParticles = new List<RMParticle>();
        particles = new List<GameObject>();

        //Initialize body variables
        body = new RMBody(spacing);
        body.w = w;
        body.alpha = alpha;
        body.fracturing = fracturing;
        body.fractureDistanceTolerance = fractureDistanceTolerance;
        body.fractureRotationTolerance = fractureRotationTolerance;
        body.kRegionDamping = regionDamping;
        body.kChunkDamping = chunkDamping;

        //All cells and particles have a parent, Physics Node
        //This node basically undoes the scale in the editor, so that mesh scale doesn't affect particle scale
        //In game it does nothing, since mesh scale becomes unit.
        Transform physicsNodeT = transform.Find("Physics Node");
        if (physicsNodeT == null)
        {
            physicsNode = new GameObject("Physics Node");
            physicsNode.transform.parent = transform;
            physicsNode.transform.localPosition = Vector3.zero;
            physicsNode.transform.localRotation = Quaternion.identity;
        }
        else
        {
            physicsNode = physicsNodeT.gameObject;
        }

        //Create cell lattice
        if (shape != null)
        {
            foreach (Vector3 point in shape.getLatticePoints())
            {
                RMCell rmc = new RMCell();
                rmc = body.AddCell(new ATPoint3((int)point[0], (int)point.y, (int)point.z));
                
                //find Unity Cell if it already exists, otherwise create one
                Transform cellTransform = this.physicsNode.transform.Find(rmc.index.ToString());
                if (cellTransform == null)
                {
                    GameObject c = Instantiate(cellPrefab) as GameObject;
                    c.name = rmc.index.ToString();
                    c.transform.parent = this.physicsNode.transform;
                    cellTransform = c.transform;
                    cellTransform.localScale = spacing; //We're dropping the scale off of body, so make sure children know
                }
                CellScript s = cellTransform.GetComponent(typeof(CellScript)) as CellScript;
                s.Initialize(rmc);
            }
        }

        //Body initialization complete
        body.BodyFinalize();
        if (Application.isPlaying && shape != null)
        {
            shape.finishInitialization(body);
            if (shape is MeshShape)
            {
                mesh = (shape as MeshShape).mesh;
            }
            else
            {
                mesh = null;
            }
        }

        //Set up each particle with a Unity representation
        //Scale of the body goes back to zero.  Don't worry, we already used it to update the voxelization,
        //this just makes the math easier.
        scale = transform.localScale;
        transform.localScale = new Vector3(1, 1, 1);
        physicsNode.transform.localScale = new Vector3(1, 1, 1);
        //set up counter for labeling particles
        for (int i = 0; i < body.particles.Count; i++)
        {
            RMParticle rmp = body.particles[i];
            rmp.v = velocity;
            //find Unity Particle if it already exists, otherwise create one
            Transform particleTransform = this.physicsNode.transform.Find(i.ToString());
            if (particleTransform == null)
            {
                addNewParticle(i);
            }
            else
            {
                //Instantiate and cache particle script
                ParticleScript s = particleTransform.GetComponent(typeof(ParticleScript)) as ParticleScript;
                particleScripts.Add(s);
                //Latch onto start position, unless we triggered it in editor, where we need to assign it
                if (!editorReconstruction)
                    s.RefreshStartPosition();
                s.Initialize(rmp);
                particles.Add(particleTransform.gameObject);
            }
        }


        //Ignore self collisions
        for (int i = 0; i < particles.Count; i++)
        {
            GameObject particle1 = particles[i];
            if (particle1.GetComponent<Collider>() != null)
            {
                for (int j = i+1; j < particles.Count; j++)
                {
                    GameObject particle2 = particles[j];
                    if (particle2.GetComponent<Collider>() != null)
                    {
                        Physics.IgnoreCollision(particle1.GetComponent<Collider>(), particle2.GetComponent<Collider>());
                    }
                }

                //Ignore collisions with specified gameObject as well
                foreach (GameObject colliderToIgnore in collidersToIgnore) {
                    if (colliderToIgnore != null && colliderToIgnore.GetComponent<Collider>() != null) {
                        Physics.IgnoreCollision(particle1.GetComponent<Collider>(), colliderToIgnore.GetComponent<Collider>());
                    }
                }
            }
        }

        //Call FixedUpdate in BodyUpdate to give the cell its location
        isAsleep = false;
        FixedUpdate();
        //CellScript[] cellScripts = this.GetComponentsInChildren<CellScript>();
        /*foreach (CellScript j in cellScripts)
        {
            ;// j.FixedUpdate();
        }*/

        //For trimming collision boundary
        foreach (ParticleScript p in particleScripts)
        {
            p.numConnectedCells = p.particle.parents.Count;
            //Mass scaling, could improve in future so not at runtime
            if (Application.isPlaying)
                p.GetComponent<Rigidbody>().mass *= particleMassScale;
        }

        //If we're in the editor, we need to go back to our old scales.
        //The computations took place with unit scale so I would stop messing up the math
        if (!Application.isPlaying)
        {
            transform.localScale = scale;
            physicsNode.transform.localScale = new Vector3(1.0f / transform.localScale[0], 1.0f / transform.localScale[1], 1.0f / transform.localScale[2]);
        }
    }

    public void addNewParticle(int particleNumber)
    {
        RMParticle rmp = body.particles[particleNumber];
        //Create particle prefab
        GameObject p = Instantiate(particlePrefab) as GameObject;
        p.name = particleNumber.ToString();
        Vector3 pScale = p.transform.localScale;
        p.transform.parent = physicsNode.transform;
        Transform particleTransform = p.transform;
        p.transform.localScale = extraParticleScale*Vector3.Scale(pScale, spacing);
        //Give each particle starting position and velocity
        rmp.x = transform.TransformPoint(rmp.x - Vector3.Scale(shape.getCenter(), spacing));
        Vector3 startPosition;
        startPosition = rmp.x;

        ParticleScript s = particleTransform.GetComponent(typeof(ParticleScript)) as ParticleScript;
        particleScripts.Add(s);
        s.startPosition = startPosition;
        //Latch onto start position, unless we triggered it in editor, where we need to assign it
        if (!editorReconstruction)
            s.RefreshStartPosition();
        s.Initialize(rmp);

        particles.Add(p);
    }

    public void FixedUpdate()
    {
        //Perform simulation
        if (!isAsleep)
        {
            //Profiler.BeginSample("BeforeSim");
            //Debug.Log(particleScripts.Count);
#if PARALLEL_SIMULATION
            RMParallel.ForEach<ParticleScript>(particleScripts, delegate(ParticleScript s)
#else
            foreach (ParticleScript s in particleScripts)
#endif
            {
                s.BeforeBodySim();
            }
#if PARALLEL_SIMULATION
            );
#endif
            //Profiler.EndSample();

            //Profiler.BeginSample("RMSim");
            float h = Time.deltaTime;
            body.ShapeMatch();
            body.CalculateParticleVelocities(h);
            body.ApplyChunkDamping(h);
            body.ApplyImpulsesAndRegionDamping(h);
            body.ApplyParticleVelocities(h);
            //store particle count
            int originalParticleCount = body.particles.Count;
            body.DoFracturing();
            //Profiler.EndSample();

            //Profiler.BeginSample("AfterSim");
            //NO PARALLEL: Rigidbody.setLinearVelocity no good
            foreach (ParticleScript s in particleScripts)
            {
                s.AfterBodySim();
            }
            //Profiler.EndSample();

            //Profiler.BeginSample("Mesh");
            if (shape != null && Application.isPlaying)
            {
                shape.updateMesh();
            }
            //Profiler.EndSample();

            //Create new particles if there was a fracture
            if (body.recentFractures.Count > 0)
            {
                hasFractured = true;

                //get new particle count
                int particlesLeftToGenerate = body.particles.Count - originalParticleCount;
                while (particlesLeftToGenerate > 0)
                {
                    int newParticleIndex = body.particles.Count - particlesLeftToGenerate;
                    addNewFractureParticle(newParticleIndex);
                    particlesLeftToGenerate -= 1;
                }
                //Debug.Log("NULL");
                if (fractureScript != null)
                {
                    //Debug.Log("NOTNULL");
                    foreach (RMBody.FractureInfo info in body.recentFractures)
                    {
                        fractureScript.OnFracture(info);
                    }
                }

                body.recentFractures.Clear();
            }
        }

        if (useSleep)
        {
            ////Should we sleep?
            Vector3 center;
            if (mesh != null)
            {
                center = transform.rotation * mesh.bounds.center + transform.position;
            }
            else
            {
                center = transform.position;
            }

            if (Application.isPlaying && !isAsleep && (center - Camera.main.transform.position).sqrMagnitude > sleepRadius * sleepRadius)
            {
                SleepBody();
            }
            //Should we wake up?
            else if (isAsleep && (center - Camera.main.transform.position).sqrMagnitude < wakeRadius * wakeRadius)
            {
                WakeBody();
            }
        }
    }

    void addNewFractureParticle(int particleIndex)
    {
        RMParticle rmp = body.particles[particleIndex];
        //Keep track of the particle
        //Create particle prefab
        GameObject p = Instantiate(particlePrefab) as GameObject;
        p.name = particleIndex.ToString();
        p.transform.parent = physicsNode.transform;
        p.transform.localScale = extraParticleScale*Vector3.Scale(p.transform.localScale, spacing);
        //Instantiate and cache particle script
        ParticleScript s = p.GetComponent(typeof(ParticleScript)) as ParticleScript;
        particleScripts.Add(s);
        s.startPosition = rmp.x;
        s.Initialize(rmp);
        //Ensure that this particle doesn't collide with other particles in the body.
        foreach (GameObject particle in particles)
        {
            if (particle.GetComponent<Collider>() != null)
                Physics.IgnoreCollision(particle.GetComponent<Collider>(), p.GetComponent<Collider>());
        }
        particles.Add(p);

    }

    public void SleepBody()
    {
        StartCoroutine(SleepBodyWait());
        isAsleep = true;
    }

    public void WakeBody()
    {
        foreach (ParticleScript s in particleScripts)
        {
            s.GetComponent<Rigidbody>().isKinematic = false;
            s.GetComponent<Rigidbody>().WakeUp();
        }
        isAsleep = false;
    }

    IEnumerator SleepBodyWait()
    {
        //Need to wait a frame before sleeping, probably only the first time so that the object
        //is initialized to a nonzero position, probably a better way to go about this
        //Set transform positions directly at start for each particle?
        yield return 0;
        foreach (ParticleScript s in particleScripts)
        {
            if (!s.GetComponent<Rigidbody>()) continue;
            s.GetComponent<Rigidbody>().isKinematic = true;
            s.GetComponent<Rigidbody>().Sleep();
        }
    }
}
