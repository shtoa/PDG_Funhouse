using System.Collections.Generic;
using UnityEngine;

namespace dungeonGenerator
{
    public class WallCalculator
    {

        private List<BoundsInt> wallBounds = new List<BoundsInt>();
        private List<BoundsInt> doorBounds = new List<BoundsInt>();

        private DungeonGenerator dungeonGenerator;

        public List<BoundsInt> WallBounds { get => wallBounds; set => wallBounds = value; }
        public List<BoundsInt> DoorBounds { get => doorBounds; set => doorBounds = value; }

        public WallCalculator(DungeonGenerator _dungeonGenerator)
        {
            dungeonGenerator = _dungeonGenerator;
        }


        public void CalculateWalls(List<Node> rooms, int wallThickness)
        {
            // create floors

            foreach (var room in rooms)
            {
                addWall(wallThickness, room);
            }
        }

        public void addWall(int wallThickness, Node room)
        {
            List<BoundsInt> curRoomWallBounds = CalculateWallBounds(dungeonGenerator.wallThickness, room);
            bool isIntersected = false;

            // optimize as you know how many rooms there are 
            foreach (var roomWall in curRoomWallBounds)
            {

                isIntersected = false;

                for (int i = wallBounds.Count - 1; i >= 0; i--)
                {
                    BoundsInt wall = wallBounds[i];


                    if (wall.Contains(Vector3Int.CeilToInt(roomWall.position)))
                    {
                        wallBounds.Remove(wall);
                        List<BoundsInt> splitWalls = splitWall(roomWall, wall); // change to array

                        wallBounds.AddRange(splitWalls);
                        doorBounds.Add(roomWall);

                        // add break or continue
                        isIntersected = true;
                    }

                }

                if (!isIntersected)
                {
                    wallBounds.Add(roomWall);

                }

            }

        }

        public List<BoundsInt> splitWall(BoundsInt door, BoundsInt wall)
        {
            List<BoundsInt> splitWalls = new List<BoundsInt>();

            // horizontal case
            if (door.min.z == wall.min.z)
            {
                splitWalls.Add(new BoundsInt(
                    wall.position,
                    new Vector3Int(
                        door.min.x - wall.min.x,
                        dungeonGenerator.dungeonBounds.size.y,
                        dungeonGenerator.wallThickness

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
                        dungeonGenerator.dungeonBounds.size.y,
                        dungeonGenerator.wallThickness
                    )
                ));
            }

            // vertical case
            else if (door.min.x == wall.min.x)
            {
                splitWalls.Add(new BoundsInt(
                    wall.position,
                    new Vector3Int(
                        dungeonGenerator.wallThickness,
                        dungeonGenerator.dungeonBounds.size.y,
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
                       dungeonGenerator.wallThickness,
                       dungeonGenerator.dungeonBounds.size.y,
                       wall.max.z - door.max.z

                   )
                   ));

            }


            return splitWalls;
        }

        public List<BoundsInt> CalculateWallBounds(int wallThickness, Node room)
        {

            List<BoundsInt> dungeonGeneratorRoomsWallBounds = new List<BoundsInt>();
            BoundsInt bounds;

            // for wall intersection checking
            if (room.RoomType == RoomType.Corridor)
            {
                bounds = room.Bounds;
                bounds = new BoundsInt(
                    new Vector3Int(bounds.min.x + dungeonGenerator.wallThickness,
                    bounds.min.y,
                    bounds.min.z + wallThickness)
                    , new Vector3Int(bounds.size.x - dungeonGenerator.wallThickness * 2,
                    bounds.size.y,
                    bounds.size.z - dungeonGenerator.wallThickness * 2)
                );
            }
            else
            {
                bounds = room.Bounds;
            }



            Vector3Int horizontalWallSize = new Vector3Int(
               bounds.size.x, //- dungeonGenerator.wallThickness
               dungeonGenerator.dungeonBounds.size.y,
               dungeonGenerator.wallThickness
           );

            Vector3Int verticalWallSize = new Vector3Int(
               dungeonGenerator.wallThickness,
               dungeonGenerator.dungeonBounds.size.y,
               bounds.size.z //  - dungeonGenerator.wallThickness
            );

            // top and bottom walls

            dungeonGeneratorRoomsWallBounds.Add(
                new BoundsInt(
                    new Vector3Int((int)bounds.min.x, 0, (int)bounds.max.z),
                    horizontalWallSize)
                );


            // for bottom wall + check if start r
            if (room.RoomType == RoomType.Start)
            {

                BoundsInt wall1 = new BoundsInt(
                       new Vector3Int((int)bounds.min.x, 0, (int)bounds.min.z - dungeonGenerator.wallThickness),
                       horizontalWallSize
                    );

                BoundsInt door1 = new BoundsInt(
                       new Vector3Int((int)((bounds.min.x + bounds.max.x) / 2f - dungeonGenerator.corridorWidth / 2f), 0, (int)((bounds.min.z + bounds.min.z) / 2f) - dungeonGenerator.wallThickness),
                       new Vector3Int(dungeonGenerator.corridorWidth, dungeonGenerator.dungeonBounds.size.y, dungeonGenerator.wallThickness) //  - dungeonGenerator.wallThickness*2
                    );

                var splitWalls = splitWall(door1, wall1);
                doorBounds.Add(door1);
                wallBounds.AddRange(splitWalls);


            }
            else
            {
                dungeonGeneratorRoomsWallBounds.Add(
                    new BoundsInt(
                        new Vector3Int((int)bounds.min.x, 0, (int)bounds.min.z - dungeonGenerator.wallThickness),
                        horizontalWallSize)
                    );
            }

            // left right wall bounds

            // right
            dungeonGeneratorRoomsWallBounds.Add(
               new BoundsInt(
                   new Vector3Int((int)bounds.max.x, 0, (int)bounds.min.z),
                   verticalWallSize)
               );

            // left
            dungeonGeneratorRoomsWallBounds.Add(
                new BoundsInt(
                    new Vector3Int((int)bounds.min.x - dungeonGenerator.wallThickness, 0, (int)bounds.min.z),
                    verticalWallSize)
                );


            return dungeonGeneratorRoomsWallBounds;

        }
    }

}