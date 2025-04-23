using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayTestEnvironment))]
public class PlayTestEnvironmentEditor : Editor
{

    // ref to PlayTestEnvironment
    PlayTestEnvironment playTestEnvironment;
    // Start is called before the first frame update
    private void OnEnable()
    {
        // Get Instance of DungeonGenerator Script

        playTestEnvironment = target.GetComponent<PlayTestEnvironment>();
        playTestEnvironment.currentPlayTest = playTestEnvironment.PlayTests[playTestEnvironment.currentPlayTestIndex];
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        PlayTestEnvironment myTarget = (PlayTestEnvironment)target;

        DrawDefaultInspector();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Test Name", myTarget.currentPlayTest.testName);
        EditorGUI.EndDisabledGroup();



        if (GUILayout.Button("Start Playtest"))
        {
            myTarget.StartPlaytest();
        }
    }
}
