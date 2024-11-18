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
        private Vector2Int roomSizeMin;


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

        [Header("Start Room")]
        public Material StartRoomMat;

        [Header("End Room")]
        public Material EndRoomMat;

        private List<BoundsInt> wallBounds = new List<BoundsInt>();
        private List<BoundsInt> doorBounds = new List<BoundsInt>();


        

        private void Awake()
        {
                splitCenterDeviation = new Vector2(splitCenterDeviationWidthPercent, splitCenterDeviationLengthPercent);
                roomSizeMin = new Vector2Int(roomWidthMin, roomLengthMin);
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
            var roomList = calculator.CalculateDungeon(maxIterations, roomWidthMin, roomLengthMin, splitCenterDeviation, corridorWidth, roomSizeMin);

                
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


                if(room.RoomType == RoomType.Start)
                {

                    floor.GetComponent<MeshRenderer>().material = StartRoomMat;
                    
                }

                if (room.RoomType == RoomType.End)
                {

                    floor.GetComponent<MeshRenderer>().material = EndRoomMat;
                }
            }

            foreach (var wallBound in wallBounds)
            {

                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.transform.SetParent(transform, false);

                wall.transform.localPosition = wallBound.center;
                wall.transform.localScale = wallBound.size;
            }

        

        }
        private void CalculateWalls(List<Node> rooms, int wallThickness)
        {
            // create floors

            foreach (var room in rooms) {


               addWall(wallThickness, room);

            }
        }

        private void addWall(int wallThickness, Node room)
        {
            List<BoundsInt> curRoomWallBounds = CalculateWallBounds(wallThickness, room);
            bool isIntersected = false;
            
            // optimize as you know how many rooms there are 
            foreach(var roomWall in curRoomWallBounds)
            {

                isIntersected = false;

                for (int i = this.wallBounds.Count-1; i >= 0; i--)
                {
                    BoundsInt wall = this.wallBounds[i];
                  

                    if (wall.Contains(Vector3Int.CeilToInt(roomWall.position)))
                    {
                        this.wallBounds.Remove(wall);
                        List<BoundsInt> splitWalls = splitWall(roomWall, wall); // change to array

                        this.wallBounds.AddRange(splitWalls);
                        //this.doorBounds.Add(roomWall);
                        
                        // add break or continue
                        isIntersected = true;
                    }

                }

                if (!isIntersected)
                {
                    this.wallBounds.Add(roomWall);

                }




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



            if(room.RoomType == RoomType.Start)
            {

                BoundsInt wall1 = new BoundsInt(
                       new Vector3Int((int)bounds.min.x + this.wallThickness, 0, (int)bounds.min.z),
                       horizontalWallSize
                    );

                BoundsInt door1 = new BoundsInt(
                       new Vector3Int((int)((bounds.min.x + bounds.max.x) / 2f), 0, (int)((bounds.min.z + bounds.min.z) / 2f)),
                       new Vector3Int(this.corridorWidth - this.wallThickness, this.wallHeight, this.wallThickness)
                    );

                var splitWalls = splitWall(door1, wall1);
                this.doorBounds.Add(door1);
                this.wallBounds.AddRange(splitWalls);


            } else {
                thisRoomsWallBounds.Add(
                    new BoundsInt(
                        new Vector3Int((int)bounds.min.x + this.wallThickness, 0, (int)bounds.min.z),
                        horizontalWallSize)
                    );
            }

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