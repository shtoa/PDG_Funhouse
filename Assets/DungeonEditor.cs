using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using tutorialGenerator;
using Unity.VisualScripting;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;
using System.Drawing;


namespace dungeonGenerator
{
    /*
     * <summary>
     *  Class for Visualising and Manipulating the Dungeon Bounds within the Editor 
     * </summary>
     */

    [CustomEditor(typeof(DungeonGenerator))]
    public class DungeonEditor : Editor
    {
        // ref to DungeonGenerator
        DungeonGenerator dungeonGenerator;
        private void OnEnable()
        {
            // Get Instance of DungeonGenerator Script
            dungeonGenerator = target.GetComponent<DungeonGenerator>();
        }

        public override void OnInspectorGUI()
        {
            DungeonGenerator myTarget = (DungeonGenerator)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Regenerate Dungeon"))
            {
                myTarget.RegenerateDungeon();
            }

            if (GUILayout.Button("Delete Dungeon"))
            {
                myTarget.DeleteDungeon();
            }
        }

        private void OnSceneGUI()
        {

            // run only in edit mode
            if (Application.isPlaying == false)
            {
                var dungeonBounds = dungeonGenerator.dungeonBounds; // get the bounds of the dungeon
                dungeonBounds.position = Vector3Int.CeilToInt(((DungeonGenerator)target).transform.position); // set the dungeonBounds position to the transform of the gameObject

                Handles.DrawWireCube(dungeonBounds.center, dungeonBounds.size); // draw an indication of the dungeonBounds in the editor

                EditorGUI.BeginChangeCheck();

                Vector3 scale = Handles.ScaleHandle(dungeonBounds.size, dungeonBounds.position + dungeonBounds.size, ((DungeonGenerator)target).transform.rotation); // get the scale from the scale handler


                Vector3 clampedScale = new Vector3(
                    Mathf.Max(scale.x, 2 * dungeonGenerator.wallThickness + dungeonGenerator.roomOffsetMin.x + dungeonGenerator.roomBoundsMin.size.x), 
                    Mathf.Max(scale.y, 1), 
                    Mathf.Max(scale.z, 2 * dungeonGenerator.wallThickness + dungeonGenerator.roomOffsetMin.y + +dungeonGenerator.roomBoundsMin.size.z)
                ); // clamp the scale to be at least one

               
                
                Undo.RecordObject(target, "scaleDungeon"); // add ability to undo dungeon scaling
                dungeonGenerator.dungeonBounds.size = Vector3Int.FloorToInt(clampedScale); // set the new size to the Dungeon Bounds
                
            }

        }
    }
}
