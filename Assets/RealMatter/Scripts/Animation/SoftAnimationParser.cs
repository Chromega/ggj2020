using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

/**
 * This class is responsible for taking the text representation of an animation
 * and producing SoftAnimationClips
 */
public class SoftAnimationParser {

    private const string PROCEDURAL = "procedural";
    private const string PARTICLE = "particle";
    private const char COMMENT = '#';
    private const int EOF = -1;
    private const char TUPLE_DELIMITER = ' ';
    private const char TUPLE_COMPONENT_DELIMITER = ',';
    private const string FPS = "fps";
    private const string INTERPOLATE = "interpolate";
    private const string DECLARATION = "declaration";
    private const string FRAMES = "frames";

    /**
     * Takes a TextAsset specifying an animation, and returns the SoftAnimationClip it describes
     * Returns null if there was a problem, along with printing a problem description to console
     */
    public static SoftAnimationClip Parse(TextAsset ta)
    {
        int currentLine = -1;
        string[] lines = ta.text.Split('\n');
        currentLine = NextValidLine(lines, currentLine);
        if (currentLine == EOF)
        {
            Debug.Log("Empty soft animation: " + ta.name);
            return null;
        }

        string definitionType = lines[currentLine].Trim();
        if (definitionType == PROCEDURAL)
        {
            return ParseProcedural(lines, currentLine);
        }
        else if (definitionType == PARTICLE)
        {
            return ParseParticle(lines, currentLine);
        }
        else
        {
            Debug.Log("Unknown Animation Type: " + lines[0]);
            return null;
        }
    }

    /**
     * If we have a procedural animation, we're given its class name
     */
    private static SoftAnimationClip ParseProcedural(string[] lines, int currentLine)
    {
        //Expected file format is:
        //procedural
        //ProcedureName
        currentLine = NextValidLine(lines, currentLine);
        if (currentLine == EOF)
        {
            Debug.Log("A procedure name is required for a procedural animation");
            return null;
        }
        string procedureName = lines[currentLine].Trim();
        //Create the clip from the string specified in the file
        SoftAnimationClip clip = (SoftAnimationClip)Assembly.GetExecutingAssembly().CreateInstance(procedureName);
        if (clip == null) {
            Debug.Log("Unknown procedural animation name: " + procedureName);
        }
        return clip;
    }

    /**
     * Parse a particle animation
     */
    private static SoftAnimationClip ParseParticle(string[] lines, int currentLine)
    {
        ParticleSoftAnimationClip clip = new ParticleSoftAnimationClip();

        //FPS
        currentLine = NextValidLine(lines, currentLine);
        if (currentLine == EOF)
        {
            Debug.Log("A Particle animation requires an FPS.");
            return null;
        }
        string fpsLine = lines[currentLine].Trim();
        if (fpsLine.StartsWith(FPS))
        {
            fpsLine = fpsLine.Replace(FPS, "").Trim();
            try
            {
                float fps = (float)System.Convert.ToDouble(fpsLine);
                clip.fps = fps;
            }
            catch
            {
                Debug.Log("FPS not in valid format in particle animation");
                return null;
            }
        }
        else
        {
            Debug.Log("A Particle animation requires an FPS.");
            return null;
        }

        //Interpolate frames?
        currentLine = NextValidLine(lines, currentLine);
        if (currentLine == EOF)
        {
            Debug.Log("A Particle animation requires a frame interpolation boolean.");
            return null;
        }
        string lerpLine = lines[currentLine].Trim();
        if (lerpLine.StartsWith(INTERPOLATE))
        {
            lerpLine = lerpLine.Replace(INTERPOLATE, "").Trim();
            try
            {
                bool lerp = (System.Convert.ToInt32(lerpLine) != 0 ? true : false);
                clip.interpolateFrames = lerp;
            }
            catch
            {
                Debug.Log("Interpolation not in valid format in particle animation");
                return null;
            }
        }
        else
        {
            Debug.Log("A Particle animation requires an FPS.");
            return null;
        }

        //Particle lattice declaration
        currentLine = NextValidLine(lines, currentLine);
        string declaration = lines[currentLine].Trim();
        if (declaration != DECLARATION)
        {
            Debug.Log("A particle lattice declaration is required");
            return null;
        }
        currentLine = NextValidLine(lines, currentLine);
        string affectedParticles = lines[currentLine].Trim();

        string[] particleTuples = affectedParticles.Split(TUPLE_DELIMITER);
        foreach (string tuple in particleTuples)
        {
            string[] tupleComponents = tuple.Split(TUPLE_COMPONENT_DELIMITER);
            if (tupleComponents.Length != 3)
            {
                Debug.Log("Every tuple declared in a particle animation requires 3 components.");
                return null;
            }
            ATPoint3 latticePoint = new ATPoint3();
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    int index = System.Convert.ToInt32(tupleComponents[i]);
                    latticePoint[i] = index;
                }
                catch
                {
                    Debug.Log("Particle lattice declaration in invalid format in particle animation");
                    return null;
                }
            }
            clip.particleIndices.Add(latticePoint);
        }

        //Frame declarations
        currentLine = NextValidLine(lines, currentLine);
        string frames = lines[currentLine].Trim();
        if (frames != FRAMES)
        {
            Debug.Log("A specification of frames is required in particle animation");
            return null;
        }
        currentLine = NextValidLine(lines, currentLine);
        int frameNum = 0;
        while (currentLine != EOF)
        {
            List<ATVector3f> frame = new List<ATVector3f>();
            string particlePositions = lines[currentLine].Trim();

            string[] positionTuples = particlePositions.Split(TUPLE_DELIMITER);

            bool frameNumbersSpecified = true;

            if (positionTuples.Length == clip.particleIndices.Count)
            {
                //No frame numbers specified
                frameNumbersSpecified = false;
                clip.keyframeNumbers.Add(frameNum);
            }
            else if (positionTuples.Length != clip.particleIndices.Count+1)
            {
                Debug.Log(positionTuples.Length + " positions specified on frame " + frameNum + " of soft animation, expected " + clip.particleIndices.Count);
                return null;
            }
            int index = 0;
            foreach (string tuple in positionTuples)
            {
                if (index == 0 && frameNumbersSpecified)
                {
                    index++;
                    try
                    {
                        int keyframeNum = System.Convert.ToInt32(tuple);
                        clip.keyframeNumbers.Add(keyframeNum);
                        continue;
                    }
                    catch
                    {
                        Debug.Log("Frame " + frameNum + " in invalid format in particle animation, keyframe id incorrect");
                        return null;
                    }
                }
                index++;
                string[] tupleComponents = tuple.Split(TUPLE_COMPONENT_DELIMITER);
                if (tupleComponents.Length != 3)
                {
                    Debug.Log("Every position tuple in a particle animation requires 3 components.");
                    return null;
                }
                ATVector3f position = new ATVector3f();
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        float x = (float)System.Convert.ToDouble(tupleComponents[i]);
                        position[i] = x;
                    }
                    catch
                    {
                        Debug.Log("Frame " + frameNum + " in invalid format in particle animation");
                        return null;
                    }
                }
                frame.Add(position);
            }
            clip.keyframes.Add(frame);
            currentLine = NextValidLine(lines, currentLine);
            frameNum++;
        }
        return clip;

    }

    /**
     * Get the next valid line, skipping whitespace and comments
     */
    private static int NextValidLine(string[] lines, int fromIndex)
    {
        int currentIndex = fromIndex + 1;
        while (currentIndex < lines.Length)
        {
            foreach (char c in lines[currentIndex].Trim())
            {
                if (c == COMMENT) //First substantial character is comment, no good
                {
                    break;
                }
                else
                {
                    return currentIndex;
                }
            }
            currentIndex++;
        }
        return EOF;
    }
}
