using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static dungeonGenerator.RoomGenerator;
using static UnityEngine.UI.CanvasScaler;
using Random = UnityEngine.Random;

namespace dungeonGenerator
{

    // FIX ME: Move into separate class
    public class WallBounds
    {
        public List<BoundsInt> left;
        public List<BoundsInt> right;
        public List<BoundsInt> top;
        public List<BoundsInt> bottom;

        public WallBounds()
        {
            left = new List<BoundsInt>();
            right = new List<BoundsInt>();
            top = new List<BoundsInt>();
            bottom = new List<BoundsInt>();
        }

        public List<BoundsInt> getWalls()
        {
            List<BoundsInt> walls = new List<BoundsInt>();
            walls.AddRange(left);
            walls.AddRange(right);
            walls.AddRange(top);
            walls.AddRange(bottom);

            return walls;
        }
        // FIX ME: Make method clearer
        public void removeWall(BoundsInt wall)
        {

            if (left.Contains(wall))
            {
                left.Remove(wall);
            }


            if (right.Contains(wall))
            {
                right.Remove(wall);
            }


            if (top.Contains(wall))
            {
                top.Remove(wall);
            }


            if (bottom.Contains(wall))
            {
                bottom.Remove(wall);
            }

        }

    }
    public class RoomGenerator
    {
        private int wallThickness;
        private List<BoundsInt> wallBounds = new List<BoundsInt>();
        private List<BoundsInt> doorBounds = new List<BoundsInt>();
       
        private Material wallMaterial;
        private Material floorMaterial;
        private int wallHeight;
        private Material ceilingMaterial;
        private int corridorWidth;

        private DungeonGenerator dungeonGenerator;
        private DungeonDecorator dungeonDecorator;

        public Material DoorMat { get; private set; }
        public Material StartRoomMat { get; private set; }
        public Material EndRoomMat { get; private set; }

        List<WallBounds> allWallBounds = new List<WallBounds>();

        public RoomGenerator(List<Node> RoomSpaces, GameObject dungeonObject)
        {

            DungeonDecorator dungeonDecorator = dungeonObject.GetComponent<DungeonDecorator>();
            this.dungeonDecorator = dungeonDecorator;
            this.wallMaterial = dungeonDecorator.wallMaterial;
            this.StartRoomMat = dungeonDecorator.StartRoomMat;
            this.EndRoomMat = dungeonDecorator.EndRoomMat;
            this.floorMaterial = dungeonDecorator.floorMaterial;
            this.ceilingMaterial = dungeonDecorator.ceilingMaterial;
            this.DoorMat = dungeonDecorator.DoorMat;

            DungeonGenerator dungeonGenerator = dungeonObject.GetComponent<DungeonGenerator>();

            this.dungeonGenerator = dungeonGenerator;

            // set the dungeon properties
            this.wallThickness = dungeonGenerator.wallThickness;
            this.wallHeight = dungeonGenerator.dungeonBounds.y; // FIX ME: Possibly change to a wallHeight variable
            this.corridorWidth = dungeonGenerator.corridorWidth;
    
        }


        public void GenerateRooms(List<Node> roomList)
        {

            // Room roomStyle = dungeonDecorator.rooms[Random.Range(0, dungeonDecorator.rooms.Count)];

            // create gameObjects for organization
            foreach (RoomType roomType in Enum.GetValues(typeof(RoomType)))
            {
                if (roomType != RoomType.None)
                {
                    var roomHolder = new GameObject(roomType.ToString());
                    roomHolder.transform.SetParent(dungeonGenerator.transform);
                    roomHolder.transform.position = dungeonGenerator.transform.position;
                }
            }

            // create floors
            foreach (var room in roomList)
            {

                RoomStyle roomStyle = dungeonDecorator.roomStyles[Random.Range(0, dungeonDecorator.roomStyles.Count)];
                GameObject roomObj = new GameObject(roomStyle.name);

                
               
                roomObj.transform.SetParent(dungeonGenerator.transform.Find(room.RoomType.ToString()), false);

           

                if (!room.CorridorType.Equals(CorridorType.Perpendicular))
                {
                    DrawCeiling(room, roomStyle, roomObj);
                    DrawFloor(room, roomStyle, roomObj);
                }

                if (room.RoomType.Equals(RoomType.Corridor))
                {
                }
                    DrawWalls(room, roomStyle, roomObj);
               
                //DrawWalls(room, roomStyle, roomObj);

                //if (!room.CorridorType.Equals(CorridorType.Perpendicular))
                //{
                //}

                //DrawWallsTest(room, roomStyle);
            
            }

            //foreach (var wallBound in wallCalculator.WallBounds)
            //{
            //    //DrawWalls(wallBound, roomStyle);
            //}

            //foreach (var doorBound in wallCalculator.DoorBounds)
            //{
            //    //DrawDoors(doorBound, roomStyle);
            //}


        }

        public void DrawDoors(BoundsInt doorBound, RoomStyle roomStyle, GameObject roomObj)
        {
            GameObject door = MeshHelper.CreateCuboid(doorBound.size, 1); // TODO: Make door thickness possible to manipulate
            door.transform.SetParent(roomObj.transform, false);

            door.transform.localPosition = doorBound.center + new Vector3(1, 0, 1) * wallThickness; // CHECK ME: May be wrong


            MaterialPropertyBlock matBlock = new MaterialPropertyBlock();
            matBlock.SetFloat(0, 0);

            door.GetComponent<MeshRenderer>().SetPropertyBlock(matBlock);
            door.GetComponent<MeshRenderer>().material = DoorMat;
            door.AddComponent<DoorInteraction>();
            //door.layer = LayerMask.NameToLayer("Dungeon");
        }

        public void HandlePerpendicularWalls(WallBounds wallBounds, Node room, RoomStyle roomStyle, GameObject roomObj)
        {
            // generate aladder
            BoundsInt ladderBounds = wallBounds.getWalls()[Random.Range(0, wallBounds.getWalls().Count)];
            wallBounds.removeWall(ladderBounds);


            var connectedRoomsOrderedByY = room.ConnectionsList.OrderBy(connectedRoom => connectedRoom.Bounds.min.y).ToArray();

            ladderBounds = new BoundsInt(
                    new Vector3Int(ladderBounds.x, connectedRoomsOrderedByY[0].Bounds.y, ladderBounds.z), // Bottom Room min Bounds y
                    new Vector3Int(ladderBounds.size.x, connectedRoomsOrderedByY[0].Bounds.size.y + ladderBounds.size.y, ladderBounds.size.z)
            );



            GameObject ladder = MeshHelper.CreateCuboid(ladderBounds.size, 1);
            ladder.name = "ladder";
            BoxCollider boxCollider = ladder.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = boxCollider.size + new Vector3(0.1f, 0, 0.1f);

            ladder.AddComponent<Ladder>();
            ladder.transform.SetParent(roomObj.transform, false);
            ladder.transform.localPosition = ladderBounds.center + new Vector3(1, 0, 1) * wallThickness; // CHECK ME: May be wrong
            ladder.GetComponent<MeshRenderer>().material = roomStyle.roomMaterials.ladder;

            ladder.transform.transform.position = ladder.transform.transform.position - new Vector3(0, 0.001f, 0); // prevent z fighting
        }

        public void DrawWalls(Node room, RoomStyle roomStyle, GameObject roomObj)
        {

            WallCalculator wallCalculator = new WallCalculator(dungeonGenerator);
            WallBounds wallBounds = wallCalculator.CalculateWalls(room, wallThickness);
            GameObject wallHolder = new GameObject("wallHolder");
            wallHolder.transform.SetParent(roomObj.transform, false);

            if (room.CorridorType.Equals(CorridorType.Perpendicular))
            {
                HandlePerpendicularWalls(wallBounds, room, roomStyle, roomObj);
            }

            foreach (var wallBound in wallBounds.getWalls())
            {

                GameObject wall = MeshHelper.CreateCuboid(wallBound.size, 1);
                wall.name = "Wall";
                wall.transform.SetParent(wallHolder.transform, false);

                if (room.CorridorType.Equals(CorridorType.Perpendicular))
                {
                    wall.transform.localScale = new Vector3(1, 0.99f, 1); // prevent z-fighting
                }



                wall.transform.localPosition = wallBound.center + new Vector3(1, 0, 1) * wallThickness; // CHECK ME: May be wrong
                wall.GetComponent<MeshRenderer>().material = roomStyle.roomMaterials.wall;


            }

            


                #region Minimapped Walls
                //wall.layer = LayerMask.NameToLayer("Dungeon");

                // draw wall minimap object
                //GameObject miniMapObject = GameObject.Instantiate(wall, wall.transform.position, wall.transform.rotation);
                //miniMapObject.layer = LayerMask.NameToLayer("MiniMap");
                //miniMapObject.transform.parent = wall.transform;
                #endregion
            }

        
        public GameObject getFloorMesh(Node room)
        {
            //return MeshHelper.CreatePlane(room.Bounds.size, 1);

            if (room.HolePlacements.Count > 0)
            {
                foreach (var hole in room.HolePlacements)
                {
                    if (hole.PositionType == SplitPosition.Down)
                    {
                        // logic for drawing room with a hole in the floor for movement between floors

                        //Debug.Log($"hole bounds: {hole.HoleBounds}");
                        //Debug.Log($"room bounds: {room.Bounds}");

                        return MeshHelper.CreatePlaneWithHole(room.Bounds, hole.HoleBounds, room.Bounds.size, false);

                    }
                }
            }
        
            int uvUnit = 1;
            return MeshHelper.CreatePlane(room.Bounds.size, uvUnit);
            

        }

        public GameObject getCeilingMesh(Node room)
        {

            if (room.HolePlacements.Count > 0)
            {
                foreach (var hole in room.HolePlacements)
                {
                    if (hole.PositionType == SplitPosition.Up)
                    {
                        // logic for drawing room with a hole in the floor for movement between floors

                        //Debug.Log($"hole bounds: {hole.HoleBounds}");
                        //Debug.Log($"room bounds: {room.Bounds}");

                        GameObject ceilingHole = MeshHelper.CreatePlaneWithHole(room.Bounds, hole.HoleBounds, room.Bounds.size, true);
                        ceilingHole.name = "ceilingHole";
                        return ceilingHole;

                    }
                }
            }

            int uvUnit = 1;
            return MeshHelper.CreatePlane(room.Bounds.size, uvUnit, true);


        }

        public void DrawFloor(Node room, RoomStyle roomStyle, GameObject roomObj)
        {

            GameObject floor = getFloorMesh(room);

            floor.transform.tag = "Floor";
            floor.transform.SetParent(roomObj.transform, false);

            // FIX ME: CHECK THIS TRANSFORMATION
            floor.transform.localPosition = (room.Bounds.center - new Vector3(0,room.Bounds.size.y / 2f,0)) + new Vector3(1, 0, 1) * wallThickness ; // CHECK ME: May be wrong
            floor.GetComponent<MeshRenderer>().material = room.RoomType.Equals(RoomType.Corridor) ? floorMaterial : roomStyle.roomMaterials.floor;

        

            GameObject collectable = null;

  

            switch (room.RoomType)
            {
                case RoomType.Start:


                    floor.GetComponent<MeshRenderer>().material = StartRoomMat;
                    floor.AddComponent<FloorTriggers>().roomType = room.RoomType;


                    // remove this
                    BoxCollider m = floor.AddComponent<BoxCollider>();
                    m.isTrigger = true;
                    m.size = new Vector3(m.size.x, 2f, m.size.z);

                    break;



                case RoomType.End:


                    floor.GetComponent<MeshRenderer>().material = EndRoomMat;
                    collectable = MeshCollectableCreator.Instance.GenerateCollectable(CollectableType.cylinder, floor.transform);
                    break;



                case RoomType.DeadEnd:


                    floor.GetComponent<MeshRenderer>().material = EndRoomMat;
                    collectable = MeshCollectableCreator.Instance.GenerateCollectable(CollectableType.sphere, floor.transform);
                    break;

                case RoomType.Room:

                    // when is just a normal room
                    collectable = MeshCollectableCreator.Instance.GenerateCollectable(CollectableType.cube, floor.transform);
                    break;


            }

            if (collectable != null)
            {
                collectable.transform.localPosition = new Vector3(0, 0.35f, 0);
                collectable.transform.localScale = Vector3.one * 0.25f;
            }
            //// draw the minimap floor
            //GameObject miniMapObject = GameObject.Instantiate(floor, floor.transform.position, floor.transform.rotation);

            //foreach (Transform child in floor.transform)
            //{
            //    GameObject.Destroy(child.gameObject);
            //}

            //miniMapObject.layer = LayerMask.NameToLayer("MiniMap");
            //miniMapObject.transform.parent = floor.transform;
        }


        public void DrawCeiling(Node room, RoomStyle roomStyle, GameObject roomObj)
        {
            GameObject ceiling = getCeilingMesh(room);

            ceiling.transform.SetParent(roomObj.transform, false);

            ceiling.transform.localPosition = room.Bounds.center + Vector3.up * room.Bounds.size.y/2 + new Vector3(1, 0, 1) * wallThickness; // should be 0.25f

            ceiling.GetComponent<MeshRenderer>().material = ceilingMaterial;

     
        }
    }

}
