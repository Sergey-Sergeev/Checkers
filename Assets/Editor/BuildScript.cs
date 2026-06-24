using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildScript
{
    private const string MENU_SCENE = "Assets/Scenes/MenuScene.unity";
    private const string GAME_SCENE = "Assets/Scenes/GameScene.unity";
    public static void Build()
    {
        string buildPath = "./Build/WebGL";
        if (!Directory.Exists(buildPath))
            Directory.CreateDirectory(buildPath);

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = new string[]
        {
            MENU_SCENE,
            GAME_SCENE
        };
        options.locationPathName = Path.Combine(buildPath, "index.html");
        options.target = BuildTarget.WebGL;
        options.options = BuildOptions.CompressWithLz4;

        BuildPipeline.BuildPlayer(options);
        Debug.Log("Build completed!");
    }
}