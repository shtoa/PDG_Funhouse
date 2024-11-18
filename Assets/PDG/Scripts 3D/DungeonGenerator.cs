using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
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
        public float splitCenterDeviationWidthPercent;
        [Range(0f, 1)]
        public float splitCenterDeviationLengthPercent;
        private Vector2 splitCenterDeviation;


        [Header("Room Properties")]
        public int roomWidthMin;
        public int roomLengthMin;
        private Vector2 roomSizeMin;


        [Range(0f, 1)] 
        public float roomOffsetWidthPercent;
        [Range(0f, 1)]
        public float roomOffsetLengthPercent;
        private Vector2 roomOffset;

        [Range(0f, 1)]
        public int roomRandomnessWidth;
        [Range(0f, 1)]
        public int roomRandomnessLength;
        private Vector2 roomRandomness;

        [Header("Corridor Properties")]
        [Range(1, 10)]
        public int corridorWidth;

        [Header("Wall Properties")]
        [Range(1,3)] // change to being a percent
        public int wallThickness;
        [Range(1, 10)]
        public int wallHeight;


        private List<BoundsInt> wallBounds = new List<BoundsInt>();
        private List<BoundsInt> doorBounds = new List<BoundsInt>();

        private void Awake()
        {
                splitCenterDeviation = new Vector2(splitCenterDeviationWidthPercent, splitCenterDeviationLengthPercent);
                roomSizeMin = new Vector2(roomWidthMin, roomLengthMin);
                roomOffset = new Vector2(roomOffsetWidthPercent, roomOffsetLengthPercent);
                roomRandomness = new Vector2(roomRandomnessWidth, roomRandomnessLength);

        }

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
            var roomList = calculator.CalculateDungeon(maxIterations, roomWidthMin, roomLengthMin, splitCenterDeviation, corridorWidth);

                
            DrawRooms(roomList);
        }

        private void DrawRooms(List<Node> roomList)
        {

            CalculateWalls(roomList, wallThickness);

            // create floors
            foreach (var room in roomList)
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.SetParent(transform, false);

                floor.transform.localPosition = room.Bounds.center;
                floor.transform.localScale = room.Bounds.size;
            }

            foreach (var wallBound in wallBounds)
            {

                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.transform.SetParent(transform, false);

                wall.transform.localPosition = wallBound.center;
                wall.transform.localScale = wallBound.size;
            }

        

        }

        private void DrawWalls(Node room, int wallThickness)
        {


            BoundsInt bounds = room.Bounds;

            Vector3 horizontalWallSize = new Vector3(
               bounds.size.x - this.wallThickness,
               this.wallHeight,
               this.wallThickness
           );

            Vector3 verticalWallSize = new Vector3(
               this.wallThickness,
               this.wallHeight,
               bounds.size.z - this.wallThickness
            );

            GameObject topWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topWall.transform.SetParent(transform, false);
            topWall.transform.localScale = horizontalWallSize;
            topWall.transform.localPosition = new Vector3(bounds.center.x, this.wallHeight/2f, bounds.max.z);
       

            GameObject bottomWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bottomWall.transform.SetParent(transform, false);
            bottomWall.transform.localScale = horizontalWallSize;
            bottomWall.transform.localPosition = new Vector3(bounds.center.x, this.wallHeight / 2f, bounds.min.z);


            GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.transform.SetParent(transform, false);
            rightWall.transform.localScale = verticalWallSize;
            rightWall.transform.localPosition = new Vector3(bounds.max.x, this.wallHeight / 2f, bounds.center.z);
            
            GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);   
            leftWall.transform.SetParent(transform, false);
            leftWall.transform.localScale = verticalWallSize;
            leftWall.transform.localPosition = new Vector3(bounds.min.x, this.wallHeight / 2f, bounds.center.z);
        
        }
        private void CalculateWalls(List<Node> rooms, int wallThicknes)
        {
            // create floors

            foreach (var room in rooms) {


               addWall(wallThickness, room);

            }
        }

        private void addWall(int wallThickness, Node room)
        {
            List<BoundsInt> curRoomWallBounds = CalculateWallBounds(wallThickness, room);
            
            // optimize as you know how many rooms there are 
            foreach(var roomWall in curRoomWallBounds)
            {
                for(int i = this.wallBounds.Count-1; i >= 0; i--)
                {
                    BoundsInt wall = this.wallBounds[i];

                    if (wall.Contains(Vector3Int.CeilToInt(roomWall.position))){                       
                        this.wallBounds.Remove(wall);
                        List<BoundsInt> splitWalls = splitWall(roomWall, wall); // change to array

                        this.wallBounds.AddRange(splitWalls);
                        this.doorBounds.Add(roomWall);
                    }
                }

                this.wallBounds.Add(roomWall);
            }

            
        }

        private List<BoundsInt> splitWall(BoundsInt door, BoundsInt wall)
        {
            List<BoundsInt> splitWalls = new List<BoundsInt>();

            // horizontal case
            if (door.min.z == wall.min.z)
            {
                splitWalls.Add(new BoundsInt(
                    wall.position,
                    new Vector3Int(
                        door.min.x-wall.min.x,
                        this.wallHeight,
                        this.wallThickness
                        
                    )

                ));

                splitWalls.Add(new BoundsInt(
                    new Vector3Int(
                        door.max.x,
                        wall.min.y,
                        wall.min.z
                    ),
                    new Vector3Int(
                        wall.max.x - door.max.x,
                        this.wallHeight,
                        this.wallThickness
                    )
                )) ;
            }

            // vertical case
            else if(door.min.x == wall.min.x)
            {
                splitWalls.Add(new BoundsInt(
                    wall.position,
                    new Vector3Int(
                        this.wallThickness,
                        this.wallHeight,
                        door.min.z - wall.min.z

                    )
                    ));

                splitWalls.Add(new BoundsInt(
                   new Vector3Int(
                        wall.min.x,
                        wall.min.y,
                        door.max.z
                    ),
                   new Vector3Int(
                       this.wallThickness,
                       this.wallHeight,
                       wall.max.z - door.max.z

                   )
                   ));

            }


            return splitWalls;
        }

        private List<BoundsInt> CalculateWallBounds(int wallThickness, Node room)
        {

            List<BoundsInt> thisRoomsWallBounds = new List<BoundsInt>();

            BoundsInt bounds = room.Bounds;

            Vector3Int horizontalWallSize = new Vector3Int(
               bounds.size.x - this.wallThickness,
               this.wallHeight,
               this.wallThickness
           );

            Vector3Int verticalWallSize = new Vector3Int(
               this.wallThickness,
               this.wallHeight,
               bounds.size.z - this.wallThickness
            );

            // top and bottom walls

            thisRoomsWallBounds.Add(
                new BoundsInt(
                    new Vector3Int((int)bounds.min.x + this.wallThickness, 0, (int)bounds.max.z),
                    horizontalWallSize)
                );

            thisRoomsWallBounds.Add(
                new BoundsInt(
                    new Vector3Int((int)bounds.min.x + this.wallThickness, 0, (int)bounds.min.z),
                    horizontalWallSize)
                );

            // left right wall bounds

            thisRoomsWallBounds.Add(
               new BoundsInt(
                   new Vector3Int((int)bounds.max.x, 0, (int)bounds.min.z + this.wallThickness),
                   verticalWallSize)
               );

            thisRoomsWallBounds.Add(
                new BoundsInt(
                    new Vector3Int((int)bounds.min.x, 0, (int)bounds.min.z + this.wallThickness),
                    verticalWallSize)
                );


            return thisRoomsWallBounds;

        }
    }
}