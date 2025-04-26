using dungeonGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PerformanceTester : MonoBehaviour
{


    DungeonConfig baseConfig = new DungeonConfig(); // create dungeon config to iterate 
    public DungeonGenerator dungeonGenerator;
    public TextAsset baseConfigTxt;

    public Vector2Int maxIterationRange = new Vector2Int();
    public int maxIterationGap = 1;
    public int currentMaxIteration = 1;
    private static bool hasGottenData = false;
    private static bool waitingForDungeon = false;

    public void StartTest()
    {
        currentMaxIteration = 0;

        if (currentMaxIteration < maxIterationRange.x) currentMaxIteration = maxIterationRange.x;
        // hasGottenData = false;

        //EditorApplication.playModeStateChanged += OnPlayModeChanged;
        DungeonStatTrack.trackPerformanceStats(); // track performance metrics
        EditorApplication.isPlaying = true;
        Debug.unityLogger.logEnabled = false;
        waitingForDungeon = true;
        PerformTest();
    }


    //public static void OnPlayModeChanged(PlayModeStateChange state)
    //{
    //    if(state == PlayModeStateChange.EnteredPlayMode && !hasGottenData)
    //    {
    //        GameObject.FindObjectOfType<PerformanceTester>().PerformTest();
    //        hasGottenData = true;

    //        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    //    }
    //}


    public void PerformTest()
    {

        baseConfig = SaveManager.Load(baseConfigTxt);
        baseConfig.maxIterations = currentMaxIteration;
        baseConfig.seededGenerationEnabled = true;
        Debug.Log($"CurrentMaxIteration: {currentMaxIteration}");

        if (currentMaxIteration < maxIterationRange.y)
        {
            waitingForDungeon = true;
            TestDungeon(baseConfig);

            
        } else
        {
            Debug.Log($"false, {currentMaxIteration}");
            EditorApplication.isPlaying = false;
        }

    }


    private void Update()
    {
        //while (DungeonStatTrack.getTrackPerformanceStats().fps.Count < 1)
        //{
        //    Debug.Log("Hello");
        //}
        //EditorApplication.isPlaying = false;
        if(hasGottenData)
        {
            hasGottenData = false;
            PerformTest();
        }

        if (!waitingForDungeon)
        {
            if (DungeonStatTrack.getTrackPerformanceStats().fps.Count > 5)
            {
                DungeonStatTrack.saveDungeonTestData(Application.dataPath + "/PerformanceTesting/PerformanceTestResults/", $"Iteration_PC_{currentMaxIteration}");
                //DungeonStatTrack.disposeTrackersPerformanceStats();
                DungeonStatTrack.trackedPerformanceStats = new DungeonStatTrack.TrackedPerformanceStats();
                //DungeonStatTrack.trackPerformanceStats();



                increaseIterations();
                hasGottenData = true;

            }

            if (currentMaxIteration > maxIterationRange.y) EditorApplication.isPlaying = false;
        }

    }

    public void increaseIterations()
    {
        currentMaxIteration += maxIterationGap;
    }


    public void TestDungeon(DungeonConfig config)
    {
        GameObject.Find("DungeonGen").GetComponent<DungeonGenerator>().setConfigFromTo(config, GameObject.Find("DungeonGen").GetComponent<DungeonGenerator>());
        GameObject.Find("DungeonGen").GetComponent<DungeonGenerator>().RegenerateDungeon();

        waitingForDungeon = false;




    }

    [ExecuteInEditMode]
    void OnValidate()
    {
        if(maxIterationRange.x > maxIterationRange.y) maxIterationRange.x = maxIterationRange.y;
        if (maxIterationRange.x < 0) maxIterationRange.x = 0;
        if (maxIterationRange.y < 0) maxIterationRange.y = 0;
    }
}