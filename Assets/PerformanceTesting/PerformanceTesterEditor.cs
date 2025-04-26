using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PerformanceTester))]
public class PerformanceTesterEditor : Editor
{

    // ref to PlayTestEnvironment
    PerformanceTester performanceTester;

    private void OnEnable()
    {
        // Get Instance of performanceTester script Script

        performanceTester = target.GetComponent<PerformanceTester>();
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        PerformanceTester myTarget = (PerformanceTester)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Start Performance Test"))
        {
            myTarget.StartTest();
        }
    }
}
