using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.AI;
using UnityEditorInternal;
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
        private int corridorWidthAndWall;
        [Header("Corridor Properties")]
        [Range(1, 10)]
        public int corridorWidth;

        [Header("Wall Properties")]
        [Range(1,3)] // change to being a percent
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


        private List<BoundsInt> wallBounds = new List<BoundsInt>();
        private List<BoundsInt> doorBounds = new List<BoundsInt>();




        private void Awake()
        {
                splitCenterDeviation = new Vector2(splitCenterDeviationWidthPercent, splitCenterDeviationLengthPercent);
                roomSizeMin = new Vector2Int(roomWidthMin, roomLengthMin);
                roomOffset = new Vector2(roomOffsetWidthPercent, roomOffsetLengthPercent);
                roomRandomness = new Vector2(roomRandomnessWidth, roomRandomnessLength);
                corridorWidthAndWall = wallThickness + corridorWidth;

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
            var roomList = calculator.CalculateDungeon(maxIterations, roomWidthMin, roomLengthMin, splitCenterDeviation, corridorWidthAndWall, roomSizeMin, wallThickness);

                
            DrawRooms(roomList);
        }

        private void DrawRooms(List<Node> roomList)
        {

            CalculateWalls(roomList, wallThickness);

            // create floors
            foreach (var room in roomList)
            {
                DrawFloor(room);
                DrawCeiling(room);
            }

            foreach (var wallBound in wallBounds)
            {
                DrawWalls(wallBound);
            }

            foreach (var doorBound in doorBounds)
            {
                DrawDoors(doorBound);
            }


        }

        private void DrawDoors(BoundsInt doorBound)
        {
            GameObject door = MeshHelper.CreateCuboid(doorBound.size, 1);
            door.transform.SetParent(transform, false);

            door.transform.localPosition = doorBound.center;
   
            
            MaterialPropertyBlock matBlock = new MaterialPropertyBlock();
            matBlock.SetFloat(0, 0);

            door.GetComponent<MeshRenderer>().SetPropertyBlock(matBlock);
            door.GetComponent<MeshRenderer>().material = DoorMat;
            door.AddComponent<DoorInteraction>();
        }

        private void DrawWalls(BoundsInt wallBound)
        {
            GameObject wall = MeshHelper.CreateCuboid(wallBound.size, 1);
            wall.transform.SetParent(transform, false);

            wall.transform.localPosition = wallBound.center;
            wall.GetComponent<MeshRenderer>().material = wallMaterial;
        }

        private void DrawFloor(Node room)
        {
            
            int uvUnit = 1;
            GameObject floor = MeshHelper.CreatePlane(room.Bounds.size, uvUnit);

            floor.transform.SetParent(transform, false);

            floor.transform.localPosition = room.Bounds.center;
            //floor.transform.localScale = room.Bounds.size;
            floor.GetComponent<MeshRenderer>().material = floorMaterial;

            if (room.RoomType == RoomType.Start)
            {

                floor.GetComponent<MeshRenderer>().material = StartRoomMat;

            }

            if (room.RoomType == RoomType.End)
            {

                floor.GetComponent<MeshRenderer>().material = EndRoomMat;
            }
        }

        private void DrawCeiling(Node room)
        {
            GameObject ceiling = MeshHelper.CreatePlane(room.Bounds.size, 1, true);
            GameObject ceiling2 = MeshHelper.CreatePlane(room.Bounds.size, 1);

            ceiling.transform.SetParent(transform, false);
            ceiling2.transform.SetParent(ceiling.transform, false);

            ceiling.transform.localPosition = room.Bounds.center + Vector3.up * wallHeight; // should be 0.25f
            ceiling2.transform.localPosition = 0.1f * Vector3.up;
            ceiling2.transform.localScale = Vector3.one * 1.1f;

            ceiling.GetComponent<MeshRenderer>().material = ceilingMaterial;
            ceiling2.GetComponent<MeshRenderer>().material = ceilingMaterial;
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
                        this.doorBounds.Add(roomWall);

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
            BoundsInt bounds;

            // for wall intersection checking
            if (room.RoomType == RoomType.Corridor)
            {
                bounds = room.Bounds;
                bounds = new BoundsInt(
                    new Vector3Int(bounds.min.x + wallThickness,
                    bounds.min.y,
                    bounds.min.z + wallThickness)
                    , new Vector3Int(bounds.size.x - wallThickness*2,
                    bounds.size.y,
                    bounds.size.z - wallThickness*2)
                );
            } else
            {
                bounds = room.Bounds;
            }



            Vector3Int horizontalWallSize = new Vector3Int(
               bounds.size.x, //- this.wallThickness
               this.wallHeight,
               this.wallThickness
           );

            Vector3Int verticalWallSize = new Vector3Int(
               this.wallThickness,
               this.wallHeight,
               bounds.size.z //  - this.wallThickness
            );

            // top and bottom walls

            thisRoomsWallBounds.Add(
                new BoundsInt(
                    new Vector3Int((int)bounds.min.x, 0, (int)bounds.max.z),
                    horizontalWallSize)
                );


            // for bottom wall + check if start r
            if(room.RoomType == RoomType.Start)
            {

                BoundsInt wall1 = new BoundsInt(
                       new Vector3Int((int)bounds.min.x, 0, (int)bounds.min.z-wallThickness),
                       horizontalWallSize
                    );

                BoundsInt door1 = new BoundsInt(
                       new Vector3Int((int)((bounds.min.x + bounds.max.x) / 2f) - this.wallThickness, 0, (int)((bounds.min.z + bounds.min.z) / 2f) - wallThickness),
                       new Vector3Int(this.corridorWidth - this.wallThickness*2, this.wallHeight, this.wallThickness)
                    );
                
                var splitWalls = splitWall(door1, wall1);
                this.doorBounds.Add(door1);
                this.wallBounds.AddRange(splitWalls);


            } else {
                thisRoomsWallBounds.Add(
                    new BoundsInt(
                        new Vector3Int((int)bounds.min.x, 0, (int)bounds.min.z - wallThickness),
                        horizontalWallSize)
                    );
            }

            // left right wall bounds

            // right
            thisRoomsWallBounds.Add(
               new BoundsInt(
                   new Vector3Int((int)bounds.max.x, 0, (int)bounds.min.z),
                   verticalWallSize)
               );

            // left
            thisRoomsWallBounds.Add(
                new BoundsInt(
                    new Vector3Int((int)bounds.min.x-this.wallThickness, 0, (int)bounds.min.z),
                    verticalWallSize)
                );


            return thisRoomsWallBounds;

        }
    }
}