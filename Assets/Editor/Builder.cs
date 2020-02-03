using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Builder
{

   [MenuItem("GGJ/Build Client")]
   public static void BuildClient()
   {
      // Get filename.
      string path = "Builds-Public/phone";
      string[] levels = new string[] { "Assets/Scenes/LobbyClient.unity", "Assets/Scenes/PhoneScene.unity" };
      Build(path, levels, "PROJECT:Responsive", 375, 550);

   }


   [MenuItem("GGJ/Build Client (Test)")]
   public static void BuildTestClient()
   {
      // Get filename.
      string path = "Builds-Public/phone-test";
      string[] levels = new string[] { "Assets/Scenes/LobbyClient.unity", "Assets/Scenes/PhoneScene.unity" };
      Build(path, levels, "PROJECT:Responsive", 375, 550);

   }


   [MenuItem("GGJ/Build Host")]
   public static void BuildHost()
   {
      string path = "Builds-Public/host";
      string[] levels = new string[] { "Assets/Scenes/LobbyHost.unity", "Assets/Scenes/MainScene.unity" };
      Build(path, levels, "APPLICATION:Default", 1024, 768);
   }

   static void Build(string path, string[] levels, string template, int width, int height)
   {
      string oldTemplate = PlayerSettings.WebGL.template;
      int oldWidth = PlayerSettings.defaultWebScreenWidth;
      int oldHeight = PlayerSettings.defaultWebScreenHeight;
      
      PlayerSettings.WebGL.template = template;
      PlayerSettings.defaultWebScreenWidth = width;
      PlayerSettings.defaultWebScreenHeight = height;

      // Build player.
      BuildPipeline.BuildPlayer(levels, path, BuildTarget.WebGL, BuildOptions.None);

      PlayerSettings.WebGL.template = oldTemplate;
      PlayerSettings.defaultWebScreenWidth = oldWidth;
      PlayerSettings.defaultWebScreenHeight = oldHeight;
   }
}
