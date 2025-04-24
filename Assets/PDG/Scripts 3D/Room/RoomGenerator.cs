using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
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


            int maxFloorIndex = roomList.Max((room) => room.FloorIndex).Yield().First();

            // create floors
            foreach (var room in roomList)
            {

                RoomStyle roomStyle = dungeonDecorator.roomStyles[Random.Range(0, dungeonDecorator.roomStyles.Count)];
                GameObject roomObj = new GameObject(roomStyle.name+room.SplitPosition);

                roomObj.transform.SetParent(dungeonGenerator.transform.Find(room.RoomType.ToString()), false);


                if (room.RoomType != RoomType.Corridor)
                {
                   

                    BoxCollider roomEntryCollider = roomObj.AddComponent<BoxCollider>();

                    // for testing check what current room is in
                    roomEntryCollider.size = new Vector3(room.Bounds.size.x, 1f, room.Bounds.size.z);
                    roomEntryCollider.center = room.Bounds.center + new Vector3(1, 0, 1) - (room.Bounds.size.y / 2) * Vector3.up;
                    roomEntryCollider.isTrigger = true;

                    RoomVisitChecker rc = roomObj.AddComponent<RoomVisitChecker>();
                    rc.RoomID = room.NodeID;
                }

                if (!room.CorridorType.Equals(CorridorType.Perpendicular) && !(room.RoomType == RoomType.Corridor))
                {
                    //DrawCeiling(room, roomStyle, roomObj);
                    DrawFloor(room, roomStyle, roomObj);
                }

                if (room.RoomType.Equals(RoomType.Corridor))
                {
               

                    foreach (BoundsInt bound in room.CorridorBoundsList)
                    {

                        Debug.Log($"THE NEW CORRIDOR FLOOR SIZE {bound.size}");

                        GameObject floor = MeshHelper.CreatePlane(bound.size, 1, false);
                        floor.transform.tag = "Floor";
                        floor.transform.SetParent(roomObj.transform, false);

                        // FIX ME: CHECK THIS TRANSFORMATION
                        floor.transform.localPosition = (bound.center - new Vector3(0, bound.size.y / 2f, 0)) + new Vector3(1, 0, 1) * wallThickness + Vector3.up * 0.001f; ; // CHECK ME: May be wrong
                        floor.GetComponent<MeshRenderer>().material = room.RoomType.Equals(RoomType.Corridor) ? floorMaterial : roomStyle.roomMaterials.floor;




                    }

                }

                if (room.RoomType != RoomType.Corridor)
                {
                    DrawWalls(room, roomStyle, roomObj);
                } else
                {
                    if (room.CorridorType == CorridorType.Perpendicular)
                    {
                        DrawWalls(room, roomStyle, roomObj);
                    }
                    else
                    {


                        GameObject wallHolder = new GameObject("wallHolder");
                        wallHolder.transform.SetParent(roomObj.transform, false);

                        foreach (BoundsInt wallBound in room.CorridorWallBoundsList)
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
                    }
                }

                //room.HolePlacements.Where(holePlacement => holePlacement.PositionType == SplitPosition.Up).Count() == 0

                //    && room.HolePlacements.Where(holePlacement => holePlacement.PositionType == SplitPosition.Down).Count() > 0 ||
                //    room.FloorIndex == maxFloorIndex ||


                //roomList.All(otherRoom => Mathf.Abs(Vector3.Dot(Vector3.Normalize(otherRoom.Bounds.position-room.Bounds.position), Vector3.up)) < 0.7f))

                //if (


                //     room.RoomType != RoomType.Corridor &&
                //     room.FloorIndex == maxFloorIndex

                //    // exepensive check


                //    )

                //{
                //    if (room.Parent.SplitPosition != SplitPosition.Down
                //        && room.HolePlacements.Where(holePlacement => holePlacement.PositionType == SplitPosition.Up).Count() == 0

                //    )
                //    {
                //        roomObj.name = roomObj.name + "willHaveRoof";

                //        GameObject roofObj = GameObject.Instantiate(dungeonDecorator.roofObject, roomObj.transform, false);
                //        roofObj.transform.localPosition = room.Bounds.center + room.Bounds.size.y * Vector3.up * 0.5f + new Vector3(1, 0, 1);



                //        var roofHeight = (Random.value + 1) * room.Bounds.size.y;

                //        roofObj.transform.localScale = new Vector3(room.Bounds.size.x, roofHeight, room.Bounds.size.z) * 0.5f;
                //    }
                //}

                // add corner blocks 
                if (!room.RoomType.Equals(RoomType.Corridor))
                {

                    GameObject cornerObj = GameObject.Instantiate(dungeonDecorator.cornerObject, roomObj.transform, false);
                    //cornerObj.AddComponent<MeshRenderer>();
                    cornerObj.GetComponent<MeshRenderer>().material = roomStyle.roomMaterials.wall;
                    cornerObj.transform.localScale = new Vector3(
                                cornerObj.transform.localScale.x,
                                cornerObj.transform.localScale.y,
                                cornerObj.transform.localScale.z * room.Bounds.size.y

                        );

                    // topLeftCorner
                    GameObject topLeftCorner = GameObject.Instantiate(cornerObj, roomObj.transform, false);
                    topLeftCorner.transform.localEulerAngles = new Vector3(90, 180, 0);
                    topLeftCorner.transform.localPosition = room.Bounds.size.z * Vector3.forward + room.Bounds.min + room.Bounds.size.y * Vector3.up + new Vector3(1, 0, 1);


                    // topRightCorner
                    GameObject topRightCorner = GameObject.Instantiate(cornerObj, roomObj.transform, false);
                    topRightCorner.transform.localEulerAngles = new Vector3(90, -90, 0);
                    topRightCorner.transform.localPosition = room.Bounds.size.z * Vector3.forward + room.Bounds.size.x * Vector3.right + room.Bounds.min + room.Bounds.size.y * Vector3.up + new Vector3(1, 0, 1);


                    // bottomLeftCorner
                    GameObject bottomLeftCornerObj = GameObject.Instantiate(cornerObj, roomObj.transform, false);
                    bottomLeftCornerObj.transform.localEulerAngles = new Vector3(90, 90, 0);
                    bottomLeftCornerObj.transform.localPosition = room.Bounds.min + room.Bounds.size.y * Vector3.up + new Vector3(1, 0, 1);

                    // bottomRightCorner
                    GameObject bottomRightCorner = GameObject.Instantiate(cornerObj, roomObj.transform, false);
                    bottomRightCorner.transform.localEulerAngles = new Vector3(90, 0, 0);
                    bottomRightCorner.transform.localPosition = room.Bounds.size.x * Vector3.right + room.Bounds.min + room.Bounds.size.y * Vector3.up + new Vector3(1, 0, 1);

                    GameObject.DestroyImmediate(cornerObj);
                } // remove this bracket


                    //    // generate top wall 


                    //    // construct bounds for top wall 

                    //    BoundsInt topWallBounds = new BoundsInt(
                    //        room.Bounds.position + room.Bounds.size.y * Vector3Int.up - new Vector3Int(1, 0, 1),
                    //        new Vector3Int(room.Bounds.size.x + 4, 0, room.Bounds.size.z + 4)
                    //    );

                    //    //GameObject topWallPlane = MeshHelper.CreatePlane(topWallBounds.size, 1, false);

                    //    // handle top part

                    //    if(topWallBounds.size.x % 2 == 0)
                    //    {
                    //        // if is even 
                    //        var nSegments = topWallBounds.size.x;

                    //        for (int i = 1; i < (nSegments-1); i++)
                    //        {

                    //            if (i % 2 == 0)
                    //            {
                    //                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //                cube.transform.localPosition = topWallBounds.position + i * Vector3Int.right + Vector3Int.forward; // CHECK ME: May be wrong
                    //                cube.GetComponent<MeshRenderer>().material = room.RoomType.Equals(RoomType.Corridor) ? floorMaterial : roomStyle.roomMaterials.floor;
                    //                cube.transform.SetParent(roomObj.transform, false);

                    //                GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //                cube2.transform.localPosition = topWallBounds.size.z * Vector3Int.forward + topWallBounds.position + i * Vector3Int.right - Vector3Int.forward; // CHECK ME: May be wrong
                    //                cube2.GetComponent<MeshRenderer>().material = room.RoomType.Equals(RoomType.Corridor) ? floorMaterial : roomStyle.roomMaterials.floor;
                    //                cube2.transform.SetParent(roomObj.transform, false);
                    //            }
                    //        }
                    //        // add corner segments



                    //    } else
                    //    {
                    //        // if is odd
                    //        // if is even 
                    //        var nSegments = topWallBounds.size.x;

                    //        for (int i = 1; i < (nSegments - 1); i++)
                    //        {

                    //            if (i % 2 == 0)
                    //            {
                    //                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //                cube.transform.localPosition = topWallBounds.position + i * Vector3Int.right + Vector3Int.forward; // CHECK ME: May be wrong
                    //                //cube.GetComponent<MeshRenderer>().material = roomStyle.roomMaterials.ceiling;
                    //                cube.transform.SetParent(roomObj.transform, false);

                    //                GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //                cube2.transform.localPosition = topWallBounds.size.z * Vector3Int.forward + topWallBounds.position + i * Vector3Int.right - Vector3Int.forward; // CHECK ME: May be wrong
                    //                //cube2.GetComponent<MeshRenderer>().material = roomStyle.roomMaterials.ceiling;
                    //                cube2.transform.SetParent(roomObj.transform, false);
                    //            }
                    //        }

                    //    }


                    //    // handle side part
                    //    if (topWallBounds.size.z % 2 == 0)
                    //    {
                    //        // if is even 
                    //        var nSegments = topWallBounds.size.z;

                    //        for (int i = 1; i < (nSegments - 1); i++)
                    //        {

                    //            if (i % 2 == 0)
                    //            {
                    //                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //                cube.transform.localPosition = topWallBounds.position + i * Vector3Int.forward + Vector3Int.right; // CHECK ME: May be wrong
                    //                cube.GetComponent<MeshRenderer>().material = room.RoomType.Equals(RoomType.Corridor) ? floorMaterial : roomStyle.roomMaterials.floor;
                    //                cube.transform.SetParent(roomObj.transform, false);

                    //                GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //                cube2.transform.localPosition = topWallBounds.size.x * Vector3Int.right + topWallBounds.position + i * Vector3Int.forward - Vector3Int.right; // CHECK ME: May be wrong
                    //                cube2.GetComponent<MeshRenderer>().material = room.RoomType.Equals(RoomType.Corridor) ? floorMaterial : roomStyle.roomMaterials.floor;
                    //                cube2.transform.SetParent(roomObj.transform, false);
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // if is odd
                    //        // if is even 
                    //        var nSegments = topWallBounds.size.z;

                    //        for (int i = 1; i < (nSegments - 1); i++)
                    //        {

                    //            if (i % 2 == 0)
                    //            {
                    //                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //                cube.transform.localPosition = topWallBounds.position + (i+0.5f) * Vector3.forward + Vector3.right; // CHECK ME: May be wrong
                    //                //cube.GetComponent<MeshRenderer>().material = roomStyle.roomMaterials.ceiling;
                    //                cube.transform.SetParent(roomObj.transform, false);

                    //                GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //                cube2.transform.localPosition = topWallBounds.size.x * Vector3.right + topWallBounds.position + (i+0.5f) * Vector3.forward - Vector3.right; // CHECK ME: May be wrong
                    //                //cube2.GetComponent<MeshRenderer>().material = roomStyle.roomMaterials.ceiling;
                    //                cube2.transform.SetParent(roomObj.transform, false);
                    //            }
                    //        }
                    //    }




                    #region Debugging TopWall Placement
                    //topWallPlane.transform.localPosition = topWallBounds.center; // CHECK ME: May be wrong
                    //topWallPlane.GetComponent<MeshRenderer>().material = room.RoomType.Equals(RoomType.Corridor) ? floorMaterial : roomStyle.roomMaterials.floor;
                    //topWallPlane.transform.SetParent(roomObj.transform, false);
                    #endregion



                    //}









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
            room.StairWaypoints.ForEach(v => v = v + dungeonGenerator.transform.position);
            StairsGenerator.GenerateStairs(roomObj.transform, room.StairPosition + dungeonGenerator.transform.position, floorMaterial, room.StairSize, room.StairWaypoints);

            #region ladder generation

            // generate a ladder
            //BoundsInt ladderBounds = wallBounds.getWalls()[Random.Range(0, wallBounds.getWalls().Count)];
            //wallBounds.removeWall(ladderBounds);


            //ladderBounds = new BoundsInt(
            //        new Vector3Int(ladderBounds.x, connectedRoomsOrderedByY[0].Bounds.y, ladderBounds.z), // Bottom Room min Bounds y
            //        new Vector3Int(ladderBounds.size.x, connectedRoomsOrderedByY[0].Bounds.size.y + ladderBounds.size.y, ladderBounds.size.z)
            //);

            //GameObject ladder = MeshHelper.CreateCuboid(ladderBounds.size, 1);
            //ladder.name = "ladder";
            //BoxCollider boxCollider = ladder.AddComponent<BoxCollider>();
            //boxCollider.isTrigger = true;
            //boxCollider.size = boxCollider.size + new Vector3(0.1f, 0, 0.1f);

            //ladder.AddComponent<Ladder>();
            //ladder.transform.SetParent(roomObj.transform, false);
            //ladder.transform.localPosition = ladderBounds.center + new Vector3(1, 0, 1) * wallThickness; // CHECK ME: May be wrong
            //ladder.GetComponent<MeshRenderer>().material = roomStyle.roomMaterials.ladder;

            //ladder.transform.transform.position = ladder.transform.transform.position - new Vector3(0, 0.001f, 0); // prevent z fighting
            #endregion
        }

        public void DrawWalls(Node room, RoomStyle roomStyle, GameObject roomObj)
        {

            WallCalculator wallCalculator = new WallCalculator(dungeonGenerator, dungeonDecorator);
            WallBounds wallBounds = wallCalculator.CalculateWalls(room, wallThickness);
            GameObject wallHolder = new GameObject("wallHolder");
            wallHolder.transform.SetParent(roomObj.transform, false);

            Debug.Log($"The room type is {room.CorridorType}");

            if (room.CorridorType.Equals(CorridorType.Perpendicular))
            {
                
                HandlePerpendicularWalls(wallBounds, room, roomStyle, roomObj);
                // if else removed if ladders generated... 
            }
            else
            {

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
            floor.transform.localPosition = (room.Bounds.center - new Vector3(0,room.Bounds.size.y / 2f,0)) + new Vector3(1, 0, 1) * wallThickness + Vector3.up * 0.001f; ; // CHECK ME: May be wrong
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

            ceiling.transform.localPosition = room.Bounds.center + Vector3.up * room.Bounds.size.y/2 + new Vector3(1, 0, 1) * wallThickness - Vector3.up*0.01f; // added Z offset

            ceiling.GetComponent<MeshRenderer>().material = ceilingMaterial;

     
        }
    }

}
