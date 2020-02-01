using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * A SoftAnimationClip which is defined as a sequence of frames.
 * Each frame contains the target position of all particles.
 */
public class ParticleSoftAnimationClip : SoftAnimationClip {

    /**
     * What are the particleIndices on which we're operating?
     */
    public List<ATPoint3> particleIndices = new List<ATPoint3>();

    /**
     * The list of frames.  Each frame is a list of particle positions.  The order
     * of particles is determined by particleIndices.
     */
    public List<List<ATVector3f>> keyframes = new List<List<ATVector3f>>();

    /**
     * Frame numbers
     */
    public List<int> keyframeNumbers = new List<int>();

    /**
     * How fast the animation plays in frames per second
     */
    public float fps;

    /**
     * Should the frames of the animation be interpolated between?
     */
    public bool interpolateFrames;

    /**
     * Which frame are we on?
     */
    private int keyframeIndex = -1;

    private float frameNumber = 0;

    public override bool UpdatePose(List<ATPoint3> indices, Dictionary<ATPoint3, ATVector3f> pose)
    {
        if (interpolateFrames)
        {
            //If we're interpolating, place the particles along their trajectory between
            //this frame and the next
            frameNumber += Time.deltaTime * fps;
            while (frameNumber >= keyframeNumbers[keyframeNumbers.Count - 1])
            {
                //If not looping, signal up!
                frameNumber -= keyframeNumbers[keyframeNumbers.Count - 1];
            }


            keyframeIndex = FrameNumberToKeyframeID(frameNumber);

            List<ATVector3f> frame = keyframes[keyframeIndex];
            List<ATVector3f> nextFrame = keyframes[keyframeIndex+1];
            float lerpCoefficient = (frameNumber - keyframeNumbers[keyframeIndex]) / (keyframeNumbers[keyframeIndex+1] - keyframeNumbers[keyframeIndex]);
            for (int i = 0; i < particleIndices.Count; i++)
            {
                ATPoint3 index = particleIndices[i];
                pose[index] = (1-lerpCoefficient)*frame[i] + lerpCoefficient*nextFrame[i];
            }
            return true;
        }
        else
        {
            //Without interpolating, change the frame if enough time has passed
            frameNumber += Time.deltaTime * fps;
            while (frameNumber >= keyframeNumbers[keyframeNumbers.Count - 1])
            {
                //If not looping, signal up!
                frameNumber -= keyframeNumbers[keyframeNumbers.Count - 1];
            }

            int newKeyframeIndex = FrameNumberToKeyframeID(frameNumber);

            if (newKeyframeIndex != keyframeIndex)
            {
                keyframeIndex = newKeyframeIndex;
                List<ATVector3f> frame = keyframes[keyframeIndex];
                for (int i = 0; i < particleIndices.Count; i++)
                {
                    ATPoint3 index = particleIndices[i];
                    pose[index] = frame[i];
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public override void Reset()
    {
        keyframeIndex = -1;
        frameNumber = 0;
    }

    public int FrameNumberToKeyframeID(float frameNumber)
    {
        //This shouldn't happen, but in case the frame number gets too high
        if (frameNumber >= keyframeNumbers[keyframeNumbers.Count-1]) {
            return keyframeNumbers.Count-1;
        }
        if (keyframeIndex < 0)
        {
            keyframeIndex = 0;
        }
        //We looped around
        if (frameNumber < keyframeNumbers[keyframeIndex])
        {
            int newKeyframeIndex = 0;
            while (keyframeNumbers[newKeyframeIndex] < frameNumber)
            {
                newKeyframeIndex++;
            }
            return newKeyframeIndex-1;
        }
        //The animation has gone past this keyframe pair
        if (frameNumber > keyframeNumbers[keyframeIndex + 1])
        {
            int newKeyframeIndex = keyframeIndex+1;
            while (keyframeNumbers[newKeyframeIndex] < frameNumber)
            {
                newKeyframeIndex++;
            }
            return newKeyframeIndex-1;
        }
        return keyframeIndex;
    }
}
