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

[ExecuteInEditMode]
public class PlayTestEnvironment : MonoBehaviour
{
    public DungeonGenerator dungeonGenerator;
    public List<PlayTestScriptableObject> PlayTests;
    public bool recordPlayTestVideo;

    public string playerName = string.Empty;
    [SerializeField] public bool hasTestStarted = false;

    [NonSerialized]
    public PlayTestScriptableObject currentPlayTest;

    [NonSerialized]
    public int currentPlayTestIndex = 0;

    [NonSerialized]
    public int currentConfigIndex = 0;

    private string curDir = string.Empty;

    void Start()
    {
        if (Application.isPlaying && hasTestStarted)
        {
            GameManager.Instance.isPlayTestingEnabled = true;
            curDir = AssetDatabase.GenerateUniqueAssetPath(Application.dataPath + "/PlayTesting/PlayTests/" + playerName + "/" + PlayTests[currentPlayTestIndex].name + "/" + PlayTests[currentPlayTestIndex].dungeonConfigs[currentConfigIndex].name) + "/";
            if(recordPlayTestVideo) PlayTestRecorder.StartRecording(curDir, "testRecording");
        }

        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange change)
    {
        if (change.Equals(PlayModeStateChange.EnteredEditMode)) hasTestStarted = false;
    }

    public void StartPlaytest()
    {
        currentPlayTestIndex = 0;
        currentConfigIndex = 0;
        hasTestStarted = true;
        InitializePlaytest();
    }

    public void InitializePlaytest()
    {
        InitializePlayerDir();
        InitializeConfig();
        InitializeTestDir();
    }

    public void EndPlaytest()
    {
        hasTestStarted = false; 
    }

    public void InitializePlayerDir()
    {
        string dir = Application.dataPath + "/PlayTesting/PlayTests/" + playerName;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        
    }


    public void InitializeTestDir()
    {
        string dir = Application.dataPath + "/PlayTesting/PlayTests/" + playerName + "/" + PlayTests[currentPlayTestIndex].name;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        
    }

    public void InitializeConfigDir()
    {
        string dir = AssetDatabase.GenerateUniqueAssetPath(Application.dataPath + "/PlayTesting/PlayTests/" + playerName + "/" + PlayTests[currentPlayTestIndex].name + "/" + PlayTests[currentPlayTestIndex].dungeonConfigs[currentConfigIndex].name);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
       
    }

    public void InitializeConfig()
    {
        // reset player rotation and position
        GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Rigidbody>().position = new Vector3(0, 0.46f, 0);

        dungeonGenerator.dungeonConfig = PlayTests[currentPlayTestIndex].dungeonConfigs[currentConfigIndex];
        dungeonGenerator.LoadDungeon();
        dungeonGenerator.RegenerateDungeon();
    }

    public bool Next()
    {
        Debug.Log("NextCalled");
        DungeonStatTrack.saveDungeonTestData(curDir, PlayTests[currentPlayTestIndex].dungeonConfigs[currentConfigIndex].name);

        if (recordPlayTestVideo)  PlayTestRecorder.StopRecording();


        DungeonStatTrack.disposeTrackersPerformanceStats();
        DungeonStatTrack.trackPerformanceStats();


        if (currentConfigIndex < PlayTests[currentPlayTestIndex].dungeonConfigs.Count-1)
        {

            currentConfigIndex++;
            //InitializeConfigDir();

            Debug.Log($"Testing new Config {currentPlayTestIndex}");
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

            Debug.Log($"Testing new Config {currentPlayTestIndex}");

            InitializeConfig();
            curDir = AssetDatabase.GenerateUniqueAssetPath(Application.dataPath + "/PlayTesting/PlayTests/" + playerName + "/" + PlayTests[currentPlayTestIndex].name + "/" + PlayTests[currentPlayTestIndex].dungeonConfigs[currentConfigIndex].name) + "/";
            if (recordPlayTestVideo) PlayTestRecorder.StartRecording(curDir, "testRecording");

            return true;
        }
        else
        {
            Debug.Log($"Finished Testing Configs");
            EndPlaytest();
            hasTestStarted = false;
            return false;
        }
    }

}
