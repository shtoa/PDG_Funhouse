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

    [CustomEditor(typeof(DungeonGenerator))]
    public class DungeonEditor : Editor
    {
        DungeonGenerator dungeonGenerator;
        BoxBoundsHandle h;
       



        private void OnEnable()
        {

            if (this.h == null)
            {
                this.h = new BoxBoundsHandle();
            }

     

            // FIXME: Add only if dungoen generator Exists
            dungeonGenerator = target.GetComponent<DungeonGenerator>();
            
            var dungeonBounds = dungeonGenerator.dungeonBounds;

            this.h.size = dungeonBounds.size;
            this.h.center = target.GameObject().transform.position + new Vector3(dungeonBounds.size.x/2f, dungeonBounds.size.y / 2f, dungeonBounds.size.z /2f); // FIX ME: Encode into Single Value
        
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        private void OnSceneGUI()
        {

            this.h.size = Vector3Int.FloorToInt(this.h.size);

            var dungeonBounds = dungeonGenerator.dungeonBounds;
            dungeonGenerator.dungeonBounds.size = Vector3Int.FloorToInt(this.h.size);
            this.h.center = target.GameObject().transform.position + new Vector3(dungeonBounds.size.x / 2f, dungeonBounds.size.y / 2f, dungeonBounds.size.z / 2f);

            Handles.DrawWireCube(this.h.center, this.h.size);


            EditorGUI.BeginChangeCheck();
            Vector3 scale = Handles.ScaleHandle(this.h.size, this.h.center, ((DungeonGenerator)target).transform.rotation);
            Vector3 clampedScale = new Vector3(Mathf.Max(scale.x, 1), Mathf.Max(scale.y, 1), Mathf.Max(scale.z, 1));

            if (EditorGUI.EndChangeCheck())
            {
                this.h.size = Vector3Int.FloorToInt(clampedScale);
                          
            }


        }


    }
}
