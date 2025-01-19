using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;

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


        public WallBounds CalculateWalls(Node room, int wallThickness)
        {

            WallBounds curRoomWallBounds = CalculateWallBounds(wallThickness, room);
            bool isIntersected = false;


            foreach (var placement in room.DoorPlacements)
            {

                //Debug.Log(placement.PositionType);

                List<BoundsInt> splitWalls = new List<BoundsInt>();

                switch (placement.PositionType)
                {
                    case SplitPosition.Left:

                        if (room.RoomType != RoomType.Corridor)
                        {
                            splitWalls = splitWall(placement.DoorBounds, curRoomWallBounds.left[0], SplitPosition.Left);
                        }
                        curRoomWallBounds.left = splitWalls;

                        break;

                    case SplitPosition.Right:

                        if (room.RoomType != RoomType.Corridor)
                        {
                            splitWalls = splitWall(placement.DoorBounds, curRoomWallBounds.right[0], SplitPosition.Right);
                        }

                        curRoomWallBounds.right = splitWalls;
                        break;

                    case SplitPosition.Top:

                        if (room.RoomType != RoomType.Corridor)
                        {
                            splitWalls = splitWall(placement.DoorBounds, curRoomWallBounds.top[0], SplitPosition.Top);
                        }
                        curRoomWallBounds.top = splitWalls;
                        break;

                    case SplitPosition.Bottom:

                        if (room.RoomType != RoomType.Corridor)
                        {
                            splitWalls = splitWall(placement.DoorBounds, curRoomWallBounds.bottom[0], SplitPosition.Bottom);
                        }
                        curRoomWallBounds.bottom = splitWalls;
                        break;


                }
            }


            return curRoomWallBounds;

            //getWalls(wallThickness, room);

        }

        public void addWall(int wallThickness, Node room)
        {
            


            // optimize as you know how many rooms there are 
            //foreach (var roomWall in curRoomWallBounds)
            //{

            //    isIntersected = false;

            //    for (int i = wallBounds.Count - 1; i >= 0; i--)
            //    {
            //        BoundsInt wall = wallBounds[i];


            //        if (wall.Contains(Vector3Int.CeilToInt(roomWall.position)))
            //        {
            //            wallBounds.Remove(wall);
            //            List<BoundsInt> splitWalls = splitWall(roomWall, wall); // change to array

            //            wallBounds.AddRange(splitWalls);
            //            doorBounds.Add(roomWall);

            //            // add break or continue
            //            isIntersected = true;
            //        }

            //    }

            //    if (!isIntersected)
            //    {
            //        wallBounds.Add(roomWall);

            //    }

            //}

        }

        public List<BoundsInt> splitWall(BoundsInt door, BoundsInt wall, SplitPosition splitDir)
        {
            List<BoundsInt> splitWalls = new List<BoundsInt>();

            // horizontal case
            if (splitDir == SplitPosition.Top || splitDir == SplitPosition.Bottom)
            {

                splitWalls.Add(new BoundsInt(
                    wall.position,
                    new Vector3Int(
                        door.min.x - wall.min.x,
                        wall.size.y,
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
                        wall.size.y,
                        dungeonGenerator.wallThickness
                    )
                ));
            }

            // vertical case
            else if (splitDir >= SplitPosition.Left || splitDir >= SplitPosition.Right)
            {

                splitWalls.Add(new BoundsInt(
                    wall.position,
                    new Vector3Int(
                        dungeonGenerator.wallThickness,
                        wall.size.y,
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
                       wall.size.y,
                       wall.max.z - door.max.z

                   )
                   ));

            }


            return splitWalls;
        }

        public WallBounds CalculateWallBounds(int wallThickness, Node room)
        {

            WallBounds wallBounds = new WallBounds();
            BoundsInt bounds;

            // for wall intersection checking
            if (room.RoomType == RoomType.Corridor)
            {
                bounds = room.Bounds;

                if (room.CorridorType.Equals(CorridorType.Horizontal))
                {

                    bounds = new BoundsInt(
                        new Vector3Int(bounds.min.x + wallThickness,
                        bounds.min.y,
                        bounds.min.z + wallThickness)
                        , new Vector3Int(bounds.size.x - 2*wallThickness,
                        bounds.size.y,
                        bounds.size.z - 2 * wallThickness)
                    );
                } else if (room.CorridorType.Equals (CorridorType.Vertical))
                {

                    bounds = new BoundsInt(
                        new Vector3Int(bounds.min.x + wallThickness,
                        bounds.min.y,
                        bounds.min.z + wallThickness)
                        , new Vector3Int(bounds.size.x - 2 * wallThickness,
                        bounds.size.y,
                        bounds.size.z - 2 * wallThickness)
                    );

                }
            }
            else
            {
                bounds = room.Bounds;
            }

           

            Vector3Int horizontalWallSize = new Vector3Int(
               bounds.size.x, //- dungeonGenerator.wallThickness
               room.Bounds.size.y,
               dungeonGenerator.wallThickness
           );

            Vector3Int verticalWallSize = new Vector3Int(
               dungeonGenerator.wallThickness,
               room.Bounds.size.y,
               bounds.size.z //  - dungeonGenerator.wallThickness
            );

            // top and bottom walls

            wallBounds.top.Add(
                new BoundsInt(
                    new Vector3Int((int)bounds.min.x, (int)bounds.min.y, (int)bounds.max.z),
                    horizontalWallSize)
                );


            // for bottom wall + check if start r
            if (room.RoomType == RoomType.Start)
            {

                //BoundsInt wall1 = new BoundsInt(
                //       new Vector3Int((int)bounds.min.x, 0, (int)bounds.min.z - dungeonGenerator.wallThickness),
                //       horizontalWallSize
                //    );

                //BoundsInt door1 = new BoundsInt(
                //       new Vector3Int((int)((bounds.min.x + bounds.max.x) / 2f - dungeonGenerator.corridorWidth / 2f), 0, (int)((bounds.min.z + bounds.min.z) / 2f) - dungeonGenerator.wallThickness),
                //       new Vector3Int(dungeonGenerator.corridorWidth, dungeonGenerator.dungeonBounds.size.y, dungeonGenerator.wallThickness) //  - dungeonGenerator.wallThickness*2
                //    );

                //var splitWalls = splitWall(door1, wall1);
                //doorBounds.Add(door1);
                //wallBounds.AddRange(splitWalls);

                wallBounds.bottom.Add(
                    new BoundsInt(
                        new Vector3Int((int)bounds.min.x, (int)bounds.min.y, (int)bounds.min.z - dungeonGenerator.wallThickness),
                        horizontalWallSize)
                    );


            }
            else
            {
                wallBounds.bottom.Add(
                    new BoundsInt(
                        new Vector3Int((int)bounds.min.x, (int)bounds.min.y, (int)bounds.min.z - dungeonGenerator.wallThickness),
                        horizontalWallSize)
                    );
                    
            }

            // left right wall bounds

            // right
            wallBounds.right.Add(
               new BoundsInt(
                   new Vector3Int((int)bounds.max.x, (int)bounds.min.y, (int)bounds.min.z),
                   verticalWallSize)
               );

            // left
            wallBounds.left.Add(
                new BoundsInt(
                    new Vector3Int((int)bounds.min.x - dungeonGenerator.wallThickness, (int)bounds.min.y, (int)bounds.min.z),
                    verticalWallSize)
                );
                


            return wallBounds;

        }
    }

}