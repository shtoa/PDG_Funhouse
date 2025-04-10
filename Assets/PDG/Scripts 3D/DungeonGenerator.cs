using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.AI;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;
namespace dungeonGenerator
{

    public class DungeonGenerator : MonoBehaviour
    {

        //[HideInInspector]
        public BoundsInt dungeonBounds; // TODO: Find Way to Visualize the bounds inside a mini Window

        [Header("Dungeon Properties")]

        public int maxIterations;

        [Header("Split Properties")]
        public Vector3 splitCenterDeviation;


        [Header("Room Properties")]
        public BoundsInt roomBoundsMin;
        public Vector3Int roomOffsetMin;

        public Vector3Int roomPlacementRandomness;

        private int corridorWidthAndWall;
        [Header("Corridor Properties")]
        public int corridorWidth;
        public int corridorHeight;  

        [Header("Wall Properties")]
        public int wallThickness;

        [Header("Door")]
        public float doorThickness;


        private List<BoundsInt> wallBounds = new List<BoundsInt>();
        private List<BoundsInt> doorBounds = new List<BoundsInt>();
        

        public List<Node> roomList { get; private set; }
        public GameObject startRoom { get; private set; }

        private static Dictionary<CollectableType, int> total = Enum.GetValues(typeof(CollectableType)).Cast<CollectableType>().ToDictionary(t => t, t => 0);

        public void RegenerateDungeon()
        {
            DeleteDungeon();
            corridorWidthAndWall = corridorWidth + 2 * wallThickness;
            GenerateDungeon();
        }

        public void DeleteDungeon()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        private void GenerateDungeon()
        {
            DungeonCalculator calculator = new DungeonCalculator(dungeonBounds);
            
            // TODO: Make objects for Room Properties, Wall Properties, Corridor Properties to pass down
            roomList = calculator.CalculateDungeon(maxIterations, 
                                                   roomBoundsMin, 
                                                   splitCenterDeviation, 
                                                   corridorWidthAndWall, 
                                                   wallThickness, 
                                                   roomOffsetMin, 
                                                   corridorHeight);

            if(roomList.Count == 0)
            {
                this.RegenerateDungeon();
                return;
            }


            InitializeStartAndEnd(calculator.RoomSpaces);

            RoomGenerator roomGenerator = new RoomGenerator(roomList, this.gameObject);
            roomGenerator.GenerateRooms(roomList);
        }

        private void InitializeStartAndEnd(List<Node> roomSpaces)
        {
            // Pick Start and End Rooms
            var startAndEnd = GraphHelper.ChooseStartAndEnd(roomSpaces); // change the data structure here


            // get edge rooms  
            // TODO: MOVE THIS INTO SEPARATE FUNCTION
            var deadEnds = GraphHelper.GetLeaves(startAndEnd[0], false);
            foreach (var deadEnd in deadEnds)
            {
                if (deadEnd != startAndEnd[1])
                {
                    deadEnd.RoomType = RoomType.DeadEnd;
                } 
            }

            // TODO: Refactor this to an event 
            total[CollectableType.cylinder] = 1;
            total[CollectableType.sphere] = deadEnds.Count - 1;
            total[CollectableType.cube] = roomSpaces.Count - 2 - total[CollectableType.sphere];

            saveDungeonConfig();
            GameManager.Instance.total = total;
            GameManager.Instance.numCollected = Enum.GetValues(typeof(CollectableType)).Cast<CollectableType>().ToDictionary(t => t, t => 0);
        }

        void saveDungeonConfig()
        {
            
            //Debug.Log("Save Config");
            string savePath = Application.dataPath + "/Save/DungeonConfig.txt";
            File.WriteAllText(savePath, 
                "cylinder"+","+ total[CollectableType.cylinder]+"\n"+
                "sphere" + "," + total[CollectableType.sphere] + "\n"+
                "cube" + "," + total[CollectableType.cube] + "\n"
            );
            
        }

        void loadDungeonConfig()
        {
            string savePath = Application.dataPath + "/Save/DungeonConfig.txt";
            List<string> configValues = File.ReadLines(savePath).ToList();


            foreach (var line in configValues)
            {
                //Debug.Log(line.Split(",")[1]);
                if (line.Contains("cylinder"))
                {
                    total[CollectableType.cylinder] = int.Parse(line.Split(",")[1]);
                } else if (line.Contains("sphere"))
                {
                    total[CollectableType.sphere] = int.Parse(line.Split(",")[1]);
                } else if (line.Contains("cube"))
                {
                    total[CollectableType.cube] = int.Parse(line.Split(",")[1]);
                }
            }
        }

        private void Awake()
        {
            loadDungeonConfig();
            GameManager.Instance.total = total;
        }

        /// <summary>
        /// Add Checks for Slider Ranges for Valid Generation
        /// </summary>

        [ExecuteInEditMode]
        void OnValidate()
        {

            // TODO: Make this more modular 

            // Max Iterations
            maxIterations = Mathf.Max(maxIterations,0);

            // wallThickness
            wallThickness = Mathf.Max(1, wallThickness);

            // Dungeon Bounds 
            // --- Dont Allow Dungeons of Negative sizes
            // TODO: Find Method to do it in one call
            dungeonBounds = new BoundsInt(dungeonBounds.position,
                new Vector3Int(
                    Mathf.Max(dungeonBounds.size.x, 2 * wallThickness + roomOffsetMin.x + roomBoundsMin.size.x),
                    Mathf.Max(dungeonBounds.size.y, 1),
                    Mathf.Max(dungeonBounds.size.z, 2 * wallThickness + + roomOffsetMin.y + roomBoundsMin.size.z)
                )
            );

            // Split Center Deviation
            // --- Clamp between 0-1
            splitCenterDeviation.x = Mathf.Clamp01(splitCenterDeviation.x);
            splitCenterDeviation.y = Mathf.Clamp01(splitCenterDeviation.y);
            splitCenterDeviation.z = Mathf.Clamp01(splitCenterDeviation.z);


            // Room Bounds 
            // --- Dont Allow Rooms larger than the dungeon
            // TODO: Find Method to do it in one call
            roomBoundsMin = new BoundsInt(Vector3Int.zero,
                new Vector3Int(
                    Mathf.Clamp(roomBoundsMin.size.x, 3, dungeonBounds.size.x), // minimum dungeon size is set to 3 as otherwise corridor positioning looks weird 
                    Mathf.Clamp(roomBoundsMin.size.y, 1, dungeonBounds.size.y),
                    Mathf.Clamp(roomBoundsMin.size.z, 3, dungeonBounds.size.z)
                )
            );

            // TODO: Change to allow Dungeons without Offset
            // --- Dont Allow Rooms larger than the dungeon
            roomOffsetMin = new Vector3Int(Mathf.Max(1,roomOffsetMin.x),Mathf.Max(1,roomOffsetMin.y), Mathf.Max(1, roomOffsetMin.z));

            // Corridor Width
            // --- Dont Allow Corridors larger than the minimum size dimension (width / length)

            var minDim = Mathf.Min(roomBoundsMin.size.x, roomBoundsMin.size.z);


            this.corridorWidth = Mathf.Clamp(corridorWidth,1, minDim-2);


            

        }




    }


}