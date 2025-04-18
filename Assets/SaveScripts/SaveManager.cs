using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditorInternal;
using UnityEditor;
using static dungeonGenerator.DungeonStatTrack;
using System;
using static dungeonGenerator.DungeonGenerator;
using dungeonGenerator;
using System.Text;
using UnityEditor.ShaderGraph.Serialization;

public static class SaveManager
{
    public static string directory = "/Save/GenerationConfigs/";
    public static string filename = "dungeonConfig";
    public static void Save(DungeonConfig dungeonConfigObject, string filename)
    {
        string dir = Application.dataPath + directory;

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string dungeonConfigJSON = JsonUtility.ToJson(dungeonConfigObject);
        File.WriteAllText(AssetDatabase.GenerateUniqueAssetPath(dir + filename + ".txt"), dungeonConfigJSON);

        Debug.Log($"file written to: {dir + filename + ".txt"}");

    }

    public static void Save(DungeonTestData dungeonTestData, string filename)
    {
        string dir = Application.dataPath + "/Save/TestData/";

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string dungeonTestDataJSON = JsonUtility.ToJson(dungeonTestData);
        File.WriteAllText(AssetDatabase.GenerateUniqueAssetPath(dir + filename + ".json"), dungeonTestDataJSON);

        Debug.Log($"file written to: {dir + filename + ".json"}");

    }

    public static DungeonConfig Load(TextAsset saveFile)
    {
        DungeonConfig dungeonConfig = JsonUtility.FromJson<DungeonConfig>(saveFile.text);

        Debug.Log($"Loaded dungeonConfig {saveFile.text}");
        Debug.Log($"Loaded dungeonConfig {dungeonConfig.roomBoundsMin.size}");

        return dungeonConfig;
    }

    public static DungeonConfig Load(string pathToSave)
    {
        DungeonConfig dungeonConfig = new DungeonConfig();

        if (File.Exists(pathToSave))
        {
            string dungeonConfigJSON = File.ReadAllText(pathToSave);
            dungeonConfig = JsonUtility.FromJson<DungeonConfig>(dungeonConfigJSON);

        }
        else
        {
            throw new System.Exception($"Config: {Path.GetFileName(pathToSave)}, Does Not Exists at: {pathToSave}");
        }

        return dungeonConfig;
    }

    internal static void Save(TestDungeonGenerationWrapper dungeonTestGeneration, string filename)
    {
        string dir = Application.dataPath + "/Save/MultiGenerationTest/";

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

      
        string dungeonConfigJSON = JsonUtility.ToJson(dungeonTestGeneration);
           
        
  
        File.WriteAllText(AssetDatabase.GenerateUniqueAssetPath(dir + filename + ".json"), dungeonConfigJSON);

        Debug.Log($"file written to: {dir + filename + ".json"}");
    }
}
