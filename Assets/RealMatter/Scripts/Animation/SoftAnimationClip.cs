using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Inspired by Unity's AnimationClip.
 * Contains data about an animation, and is responsible for updating an object's pose when requested.
 * A 'pose' is a mapping from particle indices to vector3 positions.
 */
public abstract class SoftAnimationClip {
    /**
     * Called once per frame, gives the animation a chance to update the pose.
     * @param indices A list of elements of the particle lattice
     * @param pose The destination where the output pose should be written (particle index->position)
     * @return Whether or not the animation updated the pose this frame
     */
    public abstract bool UpdatePose(List<ATPoint3> indices, Dictionary<ATPoint3,ATVector3f> pose);

    /**
     * Called when the animation should reset (i.e. return to frame 0)
     */
    public abstract void Reset();
}
