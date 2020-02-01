using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/**
 * Inspired by Unity's Animation class.
 * This is responsible for keeping track of a list of animations known to the soft body.
 * This takes the form of a list of animation names and SoftAnimationClips.
 * This class should be used for playback of those animations.
 */
[AddComponentMenu("Soft Body Physics/Soft Animation")]
//[ExecuteInEditMode]
public class SoftAnimation : MonoBehaviour {

    /**
     * This is a list of TextAssets which contain the raw animation data.  Synced with animationClipNames.
     */
    [@HideInInspector]
    public List<TextAsset> animationData = new List<TextAsset>();

    /**
     * This is a list of animation names.  Synced with animationData
     */
    [@HideInInspector]
    public List<string> animationClipNames = new List<string>();

    /**
     * This is a list of the (compiled) SoftAnimationClips
     */
    private List<SoftAnimationClip> animationClips = new List<SoftAnimationClip>();

    /**
     * This is the name of the currently playing animation
     */
    public string activeAnimationName = "";

    /**
     * This is the currently playing clip/
     */
    private SoftAnimationClip activeAnimationClip = null;

    /**
     * Should the animation named activeAnimationName play upon game start?
     */
    public bool playAutomatically = false;

    /**
     * A list of indices of the soft body's particle lattice
     */
    private List<ATPoint3> particleLatticeIndices = new List<ATPoint3>();

    /**
     * The pose our object should track
     */
    private Dictionary<ATPoint3, ATVector3f> targetPose = new Dictionary<ATPoint3,ATVector3f>();

    /**
     * Has the soft body's target pose been set to the desired targetPose?
     */
    private bool targetPoseSet = true;

    /**
     * Has this class been initialized?
     */
    [HideInInspector]
    public bool initialized = false;

    /**
     * Should we interpolate to our target pose, to prevent exploding if extreme poses are set?
     */
    public bool useLerp = false;

    /**
     * How fast should we interpolate, if useLerp is set?
     */
    public float lerpSpeed = 10.0f;

    /**
     * Enumeration specifying states our animation can be in.
     */
    public enum AnimState { Playing, Paused, Stopped };

    /**
     * Our current AnimState
     */
    [HideInInspector]
    public AnimState animState = AnimState.Stopped;

    /**
     * The BodyScript of the attached object
     */
    private BodyScript bs;

    //public bool editorPreview = false;

	void Start () {
        initialized = false;
	}
	
	void Update () {
        //Can't ensure that this Start is called before BodyScript's start
        if (!initialized)
        {
            ParseAnimationData();
            targetPoseSet = true;
            if (playAutomatically)
            {
                Play(activeAnimationName);
            }
            bs = GetComponent<BodyScript>();
            //Debug.Log(bs.body);
            //Debug.Log(bs.body.particleLattice);
            foreach (ATPoint3 index in bs.body.particleLattice.Keys)
            {
                targetPose[index] = bs.body.particleLattice[index].originalX0;
                particleLatticeIndices.Add(index);
            }
            initialized = true;
        }
        /*if (!Application.isPlaying && editorPreview)
        {
            PlayEditorAnimation();
            return;
        }*/
        //If we're in a playing state, update the animation
        if (animState == AnimState.Playing)
        {
            bool poseChanged = activeAnimationClip.UpdatePose(particleLatticeIndices, targetPose);
            if (poseChanged) targetPoseSet = false;
        }
        UpdateTargetPose();
	}

    public void Play(string animName)
    {
        int index = animationClipNames.IndexOf(animName);
        if (index == -1)
        {
            Debug.Log("Invalid animation: " + animName + ", requested.");
            return;
        }
        SoftAnimationClip clip = animationClips[index];
        if (clip == null)
        {
            Debug.Log("Null animation: " + animName);
            return;
        }
        //If we're changing clips, reset the old one (but don't if it's the same clip, it might've been paused)
        if (activeAnimationClip != clip && activeAnimationClip != null) {
            activeAnimationClip.Reset();
        }
        activeAnimationClip = clip;
        activeAnimationName = animName;
        animState = AnimState.Playing;
    }

    public void Pause()
    {
        animState = AnimState.Paused;
    }

    public void Resume()
    {
        Play(activeAnimationName);
    }

    public void Stop()
    {
        activeAnimationClip.Reset();
        animState = AnimState.Stopped;
    }

    /**
     * Returns the body to its default pose.
     */
    public void Default()
    {
        Stop();
        foreach (ATPoint3 index in particleLatticeIndices)
        {
            targetPose[index] = bs.body.particleLattice[index].originalX0;
        }
        targetPoseSet = false;
    }

    /**
     * Take the raw animation TextAssets and produce usable SoftAnimationClips
     */
    private void ParseAnimationData()
    {
        for (int i = 0; i < animationData.Count; i++)
        {
            TextAsset ta = animationData[i];
            animationClips.Add(SoftAnimationParser.Parse(ta));
        }
    }

    /**
     * Set the pose the soft body tracks.  Do nothing if the target pose hasn't changed.
     */
    private void UpdateTargetPose()
    {
        if (targetPoseSet) return;
        bool done = true;
        foreach (ATPoint3 index in particleLatticeIndices)
        {
            bool particleDone = LerpParticleTarget(bs.body.particleLattice[index], targetPose[index], lerpSpeed);
            if (!particleDone) done = false;
        }
        if (done) targetPoseSet = true;
        bs.body.invariantsDirty = true;
    }

    /**
     * Interpolate the body's target pose to the pose the AnimationClip wants it to track.
     * If useLerp is off, it just snaps.
     */
    private bool LerpParticleTarget(RMParticle rmp, ATVector3f target, float lerpSpeed)
    {
        //Instantly snap if we're not lerping
        if (!useLerp)
        {
            rmp.x0 = target;
            return true;
        }

        ATVector3f totalDisplacement = target - rmp.x0;
        float totalDistance = totalDisplacement.Length();
        float possibleDistance = lerpSpeed * Time.deltaTime;
        if (totalDistance <= possibleDistance)
        {
            rmp.x0 = target;
            return true;
        }
        else
        {
            rmp.x0 += totalDisplacement * possibleDistance / totalDistance;
            return false;
        }
    }

    /*
    private void PlayEditorAnimation()
    {
        activeAnimationClip.UpdatePose(particleLatticeIndices, targetPose);

        foreach (ParticleScript ps in bs.particleScripts)
        {
            //Instantly snap if we're not lerping
            if (!useLerp)
            {
                ps.rigidbody.transform.localPosition = targetPose[ps.particle.index];
                ps.transform.localPosition = ps.rigidbody.transform.localPosition;
                return;
            }

            ATVector3f totalDisplacement = targetPose[ps.particle.index] - ps.rigidbody.transform.localPosition;
            float totalDistance = totalDisplacement.Length();
            float possibleDistance = lerpSpeed * Time.deltaTime;
            if (totalDistance <= possibleDistance)
            {
                ps.rigidbody.transform.localPosition = targetPose[ps.particle.index];
            }
            else
            {
                ps.rigidbody.transform.localPosition += (Vector3)(totalDisplacement * possibleDistance / totalDistance);
            }
            ps.transform.localPosition = ps.rigidbody.transform.localPosition;
        }
    }*/
}
