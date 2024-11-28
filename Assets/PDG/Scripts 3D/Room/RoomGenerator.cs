using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using dungeonGenerator;


namespace dungeonGenerator
{
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

        public Material DoorMat { get; private set; }
        public Material StartRoomMat { get; private set; }
        public Material EndRoomMat { get; private set; }

        public RoomGenerator(List<Node> RoomSpaces, DungeonGenerator _dungeonGenerator)
        {
            wallThickness = _dungeonGenerator.wallThickness;
            wallMaterial = _dungeonGenerator.wallMaterial;
            wallHeight = _dungeonGenerator.wallHeight;
            ceilingMaterial = _dungeonGenerator.ceilingMaterial;
            corridorWidth = _dungeonGenerator.corridorWidth;
            dungeonGenerator = _dungeonGenerator;
            DoorMat = _dungeonGenerator.DoorMat;
            StartRoomMat = _dungeonGenerator.StartRoomMat;
            EndRoomMat = _dungeonGenerator.EndRoomMat;
            floorMaterial = _dungeonGenerator.floorMaterial;

        }


        public void GenerateRooms(List<Node> roomList)
        {

            WallCalculator wallCalculator = new WallCalculator(dungeonGenerator);
            wallCalculator.CalculateWalls(roomList, wallThickness);


            // create floors
            foreach (var room in roomList)
            {
                DrawFloor(room);
                //DrawCeiling(room);
            }

            foreach (var wallBound in wallCalculator.WallBounds)
            {
                DrawWalls(wallBound);
            }

            foreach (var doorBound in wallCalculator.DoorBounds)
            {
                DrawDoors(doorBound);
            }


        }

        public void DrawDoors(BoundsInt doorBound)
        {
            GameObject door = MeshHelper.CreateCuboid(doorBound.size, 1);
            door.transform.SetParent(dungeonGenerator.transform, false);

            door.transform.localPosition = doorBound.center;


            MaterialPropertyBlock matBlock = new MaterialPropertyBlock();
            matBlock.SetFloat(0, 0);

            door.GetComponent<MeshRenderer>().SetPropertyBlock(matBlock);
            door.GetComponent<MeshRenderer>().material = DoorMat;
            door.AddComponent<DoorInteraction>();
            //door.layer = LayerMask.NameToLayer("Dungeon");
        }

        public void DrawWalls(BoundsInt wallBound)
        {
            GameObject wall = MeshHelper.CreateCuboid(wallBound.size, 1);
            wall.transform.SetParent(dungeonGenerator.transform, false);

            wall.transform.localPosition = wallBound.center;
            wall.GetComponent<MeshRenderer>().material = wallMaterial;
            //wall.layer = LayerMask.NameToLayer("Dungeon");

            // draw wall minimap object
            //GameObject miniMapObject = GameObject.Instantiate(wall, wall.transform.position, wall.transform.rotation);
            //miniMapObject.layer = LayerMask.NameToLayer("MiniMap");
            //miniMapObject.transform.parent = wall.transform;
        }


        public void DrawFloor(Node room)
        {
            // maybe convert rooms to gameobjects? to allow to give them individual effect!!!

            int uvUnit = 1;
            GameObject floor = MeshHelper.CreatePlane(room.Bounds.size, uvUnit);
            floor.transform.tag = "Floor";
            //floor.layer = LayerMask.NameToLayer("Dungeon");

            floor.transform.SetParent(dungeonGenerator.transform, false);

            floor.transform.localPosition = room.Bounds.center;
            floor.GetComponent<MeshRenderer>().material = floorMaterial;

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

                case RoomType.None:

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


        public void DrawCeiling(Node room)
        {
            GameObject ceiling = MeshHelper.CreatePlane(room.Bounds.size, 1, true);
            GameObject ceiling2 = MeshHelper.CreatePlane(room.Bounds.size, 1);

            ceiling.transform.SetParent(dungeonGenerator.transform, false);
            ceiling2.transform.SetParent(ceiling.transform, false);

            ceiling.transform.localPosition = room.Bounds.center + Vector3.up * wallHeight; // should be 0.25f
            ceiling2.transform.localPosition = 0.1f * Vector3.up;
            ceiling2.transform.localScale = Vector3.one * 1.1f;

            ceiling.GetComponent<MeshRenderer>().material = ceilingMaterial;
            ceiling2.GetComponent<MeshRenderer>().material = ceilingMaterial;
        }
    }

}
