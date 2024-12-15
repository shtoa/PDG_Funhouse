using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.AI;
using UnityEditorInternal;
using UnityEngine;
namespace dungeonGenerator
{



    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Dungeon Properties")]

        public int dungeonWidth;
        public int dungeonLength;
        public int maxIterations;

        [Header("Space Properties")]
        [Range(0f, 1)]
        public float splitCenterDeviationWidthPercent;
        [Range(0f, 1)]
        public float splitCenterDeviationLengthPercent;
        private Vector2 splitCenterDeviation;


        [Header("Room Properties")]
        public int roomWidthMin;
        public int roomLengthMin;
        private Vector2Int roomSizeMin;


        [Range(0, 10)]
        public int roomOffsetX;
        [Range(0, 10)]
        public int roomOffsetY;
        private Vector2Int roomOffset;

        [Range(0f, 1)]
        public int roomRandomnessWidth;
        [Range(0f, 1)]
        public int roomRandomnessLength;
        private Vector2 roomRandomness;
        private int corridorWidthAndWall;
        [Header("Corridor Properties")]
        [Range(1, 10)]
        public int corridorWidth;

        [Header("Wall Properties")]
        [Range(1, 3)] // change to being a percent
        public int wallThickness;
        [Range(1, 10)]
        public int wallHeight;
        public Material wallMaterial;

        [Header("Start Room")]
        public Material StartRoomMat;

        [Header("End Room")]
        public Material EndRoomMat;

        [Header("Door")]
        public Material DoorMat;
        [Range(0, 1)]
        public float doorThickness;
        [Header("Floor Properties")]
        public Material floorMaterial;

        [Header("Ceiling Properties")]
        public Material ceilingMaterial;

        [Header("Collectable Properties")] // move this to different class
        public Material collectableMaterial;
        public Material collectableOutline;
        [SerializeField] public AnimationCurve materialFadeOut;

        private List<BoundsInt> wallBounds = new List<BoundsInt>();
        private List<BoundsInt> doorBounds = new List<BoundsInt>();


        public List<Node> roomList { get; private set; }
        public GameObject startRoom { get; private set; }


        private void Awake()
        {
            splitCenterDeviation = new Vector2(splitCenterDeviationWidthPercent, splitCenterDeviationLengthPercent);
            roomSizeMin = new Vector2Int(roomWidthMin, roomLengthMin);
            roomOffset = new Vector2Int((int)(roomOffsetX), (int)(roomOffsetY));
            roomRandomness = new Vector2(roomRandomnessWidth, roomRandomnessLength);
            corridorWidthAndWall = corridorWidth + 2 * wallThickness;
            CorridorNode.wallThickness = wallThickness;

        }

        void Start()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }


            GenerateDungeon();


        }

        private void GenerateDungeon()
        {
            DungeonCalculator calculator = new DungeonCalculator(dungeonWidth, dungeonLength);
            roomList = calculator.CalculateDungeon(maxIterations, roomWidthMin, roomLengthMin, splitCenterDeviation, corridorWidthAndWall, roomSizeMin, wallThickness, roomOffset);


            InitializeStartAndEnd(calculator.roomSpaces);

            RoomGenerator rg = new RoomGenerator(roomList, this);
            rg.GenerateRooms(roomList);
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

    }
}