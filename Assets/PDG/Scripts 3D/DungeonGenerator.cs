using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



namespace dungeonGenerator {



    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Dungeon Properties")]

        public int dungeonWidth;
        public int dungeonLength;
        public int maxIterations;

        [Header("Space Properties")]
        [Range(0f, 1)]
        public float splitCenterDeviationPercent;


        [Header("Room Properties")]
        public int roomWidthMin;
        public int roomLengthMin;


        [Range(0f, 1)] 
        public float roomOffsetWidthPercent;
        [Range(0f, 1)]
        public float roomOffsetLengthPercent;

        [Range(0f, 1)]
        public int roomRandomnessWidth;
        [Range(0f, 1)]
        public int roomRandomnessLength;


        void Start()
        {
            for(int i = transform.childCount-1; i >= 0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }


            GenerateDungeon();
 
        }

        private void GenerateDungeon()
        {
            DungeonCalculator calculator = new DungeonCalculator(dungeonWidth, dungeonLength);
            var roomList = calculator.CalculateDungeon(maxIterations, roomWidthMin, roomLengthMin, splitCenterDeviationPercent);


            DrawRooms(roomList);
        }

        private void DrawRooms(List<Node> roomList)
        {


            foreach(var room in roomList)
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.SetParent(transform, false);

                floor.transform.localPosition = room.Bounds.center;
                floor.transform.localScale = room.Bounds.size;


            }
        }
    }
}