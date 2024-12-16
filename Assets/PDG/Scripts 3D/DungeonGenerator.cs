using System;
using System.Collections;
using System.Collections.Generic;
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
        public Vector2 splitCenterDeviation;


        [Header("Room Properties")]
        public BoundsInt roomBoundsMin;
        public Vector2Int roomOffset;

        public Vector2Int roomPlacementRandomness;

        private int corridorWidthAndWall;
        [Header("Corridor Properties")]
        [Range(1, 10)]
        public int corridorWidth;

        [Header("Wall Properties")]
        [Range(1, 3)] // change to being a percent
        public int wallThickness;
  


        [Header("Door")]
        [Range(0, 1)]
        public float doorThickness;
 

        private List<BoundsInt> wallBounds = new List<BoundsInt>();
        private List<BoundsInt> doorBounds = new List<BoundsInt>();


        public List<Node> roomList { get; private set; }
        public GameObject startRoom { get; private set; }


        private void Awake()
        {
            CorridorNode.wallThickness = wallThickness;
        }

        void Start()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }

            corridorWidthAndWall = corridorWidth + 2 * wallThickness;
            GenerateDungeon();
      
        }

        private void GenerateDungeon()
        {
            DungeonCalculator calculator = new DungeonCalculator(dungeonBounds);
            // TODO: Make objects for Room Properties, Wall Properties, Corridor Properties to pass down
            roomList = calculator.CalculateDungeon(maxIterations, roomBoundsMin, splitCenterDeviation, corridorWidthAndWall, wallThickness, roomOffset);


            InitializeStartAndEnd(calculator.roomSpaces);

            RoomGenerator roomGenerator = new RoomGenerator(roomList, this.gameObject);
            roomGenerator.GenerateRooms(roomList);
        }


        private void InitializeStartAndEnd(List<Node> roomSpaces)
        {
            // 5. Pick starting and ending rooms
            var startAndEnd = GraphHelper.ChooseStartAndEnd(roomSpaces); // change the data structure here


            // get edge rooms  
            // MOVE THIS INTO SEPARATE FUNCTION
            var deadEnds = GraphHelper.GetLeaves(startAndEnd[0], false);
            foreach (var deadEnd in deadEnds)
            {
                if (deadEnd != startAndEnd[1])
                {
                    deadEnd.RoomType = RoomType.DeadEnd;
                }
            }

            GameManager.Instance.total[CollectableType.cylinder] = 1;
            GameManager.Instance.total[CollectableType.sphere] = deadEnds.Count - 1;
            GameManager.Instance.total[CollectableType.cube] = roomSpaces.Count - 2 - GameManager.Instance.total[CollectableType.sphere];
        }


        /*
         
        
            Add checks for UI sliders to be in range
         
          
         */

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
                    Mathf.Max(dungeonBounds.size.x, 1+2*wallThickness),
                    Mathf.Max(dungeonBounds.size.y, 1),
                    Mathf.Max(dungeonBounds.size.z,1+2*wallThickness)
                )
            );

            // Split Center Deviation
            // --- Clamp between 0-1
            splitCenterDeviation.x = Mathf.Clamp01(splitCenterDeviation.x);
            splitCenterDeviation.y = Mathf.Clamp01(splitCenterDeviation.y);


            // Room Bounds 
            // --- Dont Allow Rooms larger than the dungeon
            // TODO: Find Method to do it in one call
            roomBoundsMin = new BoundsInt(dungeonBounds.position,
                new Vector3Int(
                    Mathf.Clamp(roomBoundsMin.size.x, 1, dungeonBounds.size.x),
                    Mathf.Clamp(roomBoundsMin.size.y, 1, dungeonBounds.size.y),
                    Mathf.Clamp(roomBoundsMin.size.z, 1, dungeonBounds.size.z)
                )
            );

            // Corridor Width
            // --- Dont Allow Corridors larger than the minimum size dimension (width / length)

            var minDim = Mathf.Min(roomBoundsMin.size.x, roomBoundsMin.size.z);


            corridorWidth = Mathf.Clamp(corridorWidth,1, minDim);


            

        }




    }


}