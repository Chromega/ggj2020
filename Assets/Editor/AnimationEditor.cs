using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SoftAnimation))]

public class AnimationEditor : Editor {

    private bool animationDataFoldout = true;
    private int DEFAULT_INDENT = 16;

    public override void OnInspectorGUI()
    {
        this.DrawDefaultInspector();


        SoftAnimation selected = (SoftAnimation)target;
        if (selected == null) return;

        EditorGUILayout.BeginVertical();
        //GUIBeginIndent(animationDataFoldout ? 11 : 14);
            animationDataFoldout = EditorGUILayout.Foldout(animationDataFoldout, "Animation Data");
            if (animationDataFoldout)
            {
                GUIBeginIndent(DEFAULT_INDENT);

                int newNumAnims = EditorGUILayout.IntField("Animation Count", selected.animationData.Count);
                EnsureNonnegative(ref newNumAnims);
                if (newNumAnims > selected.animationData.Count)
                {
                    List<TextAsset> newData = new List<TextAsset>();
                    List<string> newNames = new List<string>();

                    newData.InsertRange(0, selected.animationData);
                    newNames.InsertRange(0, selected.animationClipNames);

                    while (newData.Count < newNumAnims)
                    {
                        newData.Add(null);
                        newNames.Add("anim" + (newData.Count - 1));
                    }

                    selected.animationData = newData;
                    selected.animationClipNames = newNames;
                }
                else if (newNumAnims < selected.animationData.Count)
                {
                    List<TextAsset> newData = new List<TextAsset>();
                    List<string> newNames = new List<string>();
                    for (int i = 0; i < newNumAnims; i++)
                    {
                        newData.Add(selected.animationData[i]);
                        newNames.Add(selected.animationClipNames[i]);
                    }
                    selected.animationData = newData;
                    selected.animationClipNames = newNames;
                }

                for (int i = 0; i < selected.animationData.Count; i++)
                {
                    TextAsset ta = selected.animationData[i];
                    string name = selected.animationClipNames[i];

                    TextAsset newTa;
                    string newName;

                    EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(DEFAULT_INDENT);
                        newName = EditorGUILayout.TextField(name);
                        if (newName != name)
                        {
                            selected.animationClipNames[i] = newName;
                        }
                        newTa = (TextAsset)EditorGUILayout.ObjectField(ta, typeof(TextAsset));
                        if (newTa != ta)
                        {
                            selected.animationData[i] = newTa;
                        }
                        if (Application.isPlaying) {
                            if (GUILayout.Button("Play"))
                                selected.Play(newName);
                        }
                              
                    EditorGUILayout.EndHorizontal();
                }




                GUIEndIndent();
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                    if (selected.animState == SoftAnimation.AnimState.Paused)
                    {
                        if (GUILayout.Button("Resume"))
                            selected.Resume();
                    }
                    else if (selected.animState == SoftAnimation.AnimState.Playing)
                    {
                        if (GUILayout.Button("Pause"))
                            selected.Pause();
                    }
                    else
                    {
                        if (GUILayout.Button("Play"))
                            selected.Play(selected.activeAnimationName);
                    }
                    if (GUILayout.Button("Stop"))
                        selected.Stop();
                EditorGUILayout.EndHorizontal();
            }
        //GUIEndIndent();
        EditorGUILayout.EndVertical();
        //EditorUtility.SetDirty(selected);
        //selected.editorPreview = true;
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
