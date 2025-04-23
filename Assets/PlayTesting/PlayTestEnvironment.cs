using dungeonGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using System.IO;

public class PlayTestEnvironment : MonoBehaviour
{
    public DungeonGenerator dungeonGenerator;
    public List<PlayTestScriptableObject> PlayTests;
    public bool recordPlayTestVideo;

    public string playerName = string.Empty;


    [NonSerialized]
    public PlayTestScriptableObject currentPlayTest;

    [NonSerialized]
    public int currentPlayTestIndex = 0;

    [NonSerialized]
    public int currentConfigIndex = 0;

    private string curDir = string.Empty;

    [ExecuteInEditMode]


    void Start()
    {
        if (Application.isPlaying)
        {
            curDir = AssetDatabase.GenerateUniqueAssetPath(Application.dataPath + "/PlayTesting/PlayTests/" + playerName + "/" + PlayTests[currentPlayTestIndex].name + "/" + PlayTests[currentPlayTestIndex].dungeonConfigs[currentConfigIndex].name) + "/";
            if(recordPlayTestVideo) PlayTestRecorder.StartRecording(curDir, "testRecording");
        }
    }
    public void StartPlaytest()
    {

        InitializePlayerDir();
        InitializeConfig();
        InitializeTestDir();

    }

    public void InitializePlayerDir()
    {
        string dir = Application.dataPath + "/PlayTesting/PlayTests/" + playerName;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }


    public void InitializeTestDir()
    {
        string dir = Application.dataPath + "/PlayTesting/PlayTests/" + playerName + "/" + PlayTests[currentPlayTestIndex].name;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    public void InitializeConfigDir()
    {
        string dir = AssetDatabase.GenerateUniqueAssetPath(Application.dataPath + "/PlayTesting/PlayTests/" + playerName + "/" + PlayTests[currentPlayTestIndex].name + "/" + PlayTests[currentPlayTestIndex].dungeonConfigs[currentConfigIndex].name);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    public void InitializeConfig()
    {

        // reset player rotation and position

        GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<CharacterController>().enabled = false;
        GameObject.FindGameObjectsWithTag("Player")[0].transform.position = new Vector3(0, 0.46f, 0);
        GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<CharacterController>().enabled = true;

        dungeonGenerator.dungeonConfig = PlayTests[currentPlayTestIndex].dungeonConfigs[currentConfigIndex];
        dungeonGenerator.LoadDungeon();
        dungeonGenerator.RegenerateDungeon();



    }

    public bool Next()
    {
        DungeonStatTrack.saveDungeonTestData(curDir, PlayTests[currentPlayTestIndex].dungeonConfigs[currentConfigIndex].name);

        if (recordPlayTestVideo)  PlayTestRecorder.StopRecording();




        if (currentConfigIndex < PlayTests[currentPlayTestIndex].dungeonConfigs.Count-1)
        {

            currentConfigIndex++;
            //InitializeConfigDir();

            Debug.Log("Testing new Config");
            InitializeConfig();
            curDir = AssetDatabase.GenerateUniqueAssetPath(Application.dataPath + "/PlayTesting/PlayTests/" + playerName + "/" + PlayTests[currentPlayTestIndex].name + "/" + PlayTests[currentPlayTestIndex].dungeonConfigs[currentConfigIndex].name) + "/";
            if (recordPlayTestVideo) PlayTestRecorder.StartRecording(curDir, "testRecording");

            return true;
        }

        else if (currentPlayTestIndex < PlayTests.Count-1)
        {
            currentPlayTestIndex++;
            currentConfigIndex = 0;
            InitializeTestDir();
            //InitializeConfigDir();

            Debug.Log("Testing new Config");

            InitializeConfig();
            curDir = AssetDatabase.GenerateUniqueAssetPath(Application.dataPath + "/PlayTesting/PlayTests/" + playerName + "/" + PlayTests[currentPlayTestIndex].name + "/" + PlayTests[currentPlayTestIndex].dungeonConfigs[currentConfigIndex].name) + "/";
            if (recordPlayTestVideo) PlayTestRecorder.StartRecording(curDir, "testRecording");

            return true;
        }
        else
        {
            Debug.Log("Finished Testing Config");
            return false;
        }
    }

    public void NextTest()
    {

    }

    public void NextConfig()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //void OnValidate()
    //{
    //    if (Directory.Exists())
    //    {

    //    }
    //}
}
