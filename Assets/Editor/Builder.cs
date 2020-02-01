using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Builder
{

    [MenuItem("GGJ/Build WebGL")]
    public static void BuildWebGL()
    {

        // Get filename.
        string path = "Builds-Public/phone";////EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
        string[] levels = new string[] { "Assets/Scenes/Lobby.unity", "Assets/Scenes/PhoneScene.unity" };

        // Build player.
        BuildPipeline.BuildPlayer(levels, path, BuildTarget.WebGL, BuildOptions.None);

    }
}
