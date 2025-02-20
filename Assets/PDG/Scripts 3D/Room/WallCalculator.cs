using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Experimental.AI;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

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

        public List<BoundsInt> GetAvailablePositions(HashSet<Vector3Int> curAvailablePositions, HashSet<Vector3Int> removePositions)
        {


            return new List<BoundsInt>();
        }

        public int[,] getWallArray(int sizeX, int sizeY)
        {
            int[,] wallArray = new int[sizeX, sizeY];
            
            for (int x = 0; x < wallArray.GetLength(0); x++)
            {
                for (int y = 0; y < wallArray.GetLength(1); y++)
                { 
                    wallArray[x, y] = 1;
                }
            }

            return wallArray;
        }

    public WallBounds CalculateWalls(Node room, int wallThickness)
        {

            WallBounds curRoomWallBounds = CalculateWallBounds(wallThickness, room);
            bool isIntersected = false;

            #region

            #region Voxel Test Region
            //// voxel based approach
            List<BoundsInt> blockBounds = new List<BoundsInt>();
            //HashSet<Vector3Int> availablePositions = new HashSet<Vector3Int>();

            //HashSet<Vector3Int> doorPositions = new HashSet<Vector3Int>();

            //foreach(var doorPlacement in room.DoorPlacements){
            //    Debug.Log($"nDoor placements {room.DoorPlacements.Count()}");
            //    foreach (var pos in doorPlacement.DoorBounds.allPositionsWithin)
            //    {
            //        doorPositions.Add(pos);
            //    }
            //}
            //Debug.Log($"Door Position Length {doorPositions.Count()}");


            //HashSet<Vector3Int> availableTopPositions = new HashSet<Vector3Int>();
            //HashSet<Vector3Int> availableBottomPositions = new HashSet<Vector3Int>();
            //HashSet<Vector3Int> availableLeftPositions = new HashSet<Vector3Int>();
            //HashSet<Vector3Int> availableRightPositions = new HashSet<Vector3Int>();

            //// Top

            //foreach (var wall in curRoomWallBounds.top)
            //{
            //    foreach (var pos in wall.allPositionsWithin)
            //    {
            //        availableTopPositions.Add(pos);
            //        Debug.Log($"Avail Position {pos}");
            //    }
            //}

            //availableTopPositions.ExceptWith(doorPositions);

            //foreach (var availablePos in availableTopPositions) {
            //    blockBounds.Add(new BoundsInt(
            //        availablePos,
            //        new Vector3Int(1, 1, 1)

            //    ));
            //}

            //curRoomWallBounds.top = new List<BoundsInt>(blockBounds); // room.DoorPlacements.Select(doorPlacement=> doorPlacement.DoorBounds).ToList();
            //blockBounds.Clear();

            //// Bottom
            //foreach (var wall in curRoomWallBounds.bottom)
            //{


            //    foreach (var pos in wall.allPositionsWithin)
            //    {


            //        availableBottomPositions.Add(pos);
            //        Debug.Log($"Avail Position {pos}");

            //    }
            //}

            //availableBottomPositions.ExceptWith(doorPositions);

            //foreach (var availablePos in availableBottomPositions)
            //{
            //    if (!(availablePos.x % 2 == 0 && availablePos.y > 2 && availablePos.y < 5))
            //    {
            //        blockBounds.Add(new BoundsInt(
            //        availablePos,
            //        new Vector3Int(1, 1, 1)

            //        ));
            //    }
            //}

            //curRoomWallBounds.bottom = new List<BoundsInt>(blockBounds); // room.DoorPlacements.Select(doorPlacement=> doorPlacement.DoorBounds).ToList();
            //blockBounds.Clear();

            // Left 
            #endregion

            //int[,] leftWallBlocks = new int[curRoomWallBounds.left.ElementAt(0).size.z+1, curRoomWallBounds.left.ElementAt(0).size.y+1];

            //// intialize block
            //for (int x = 0; x < leftWallBlocks.GetLength(0); x++)
            //{
            //    for(int y = 0; y < leftWallBlocks.GetLength(1); y++)
            //    {

            //        if (room.RoomType != RoomType.Corridor)
            //        {
            //            if (!(x % 2 == 0 && (y==1 || y == (leftWallBlocks.GetLength(1)-2))))
            //            {
            //                leftWallBlocks[x, y] = 1;
            //            }
            //            else
            //            {
            //                leftWallBlocks[x, y] = 0;
            //            }
            //        } else
            //        {
            //            leftWallBlocks[x, y] = 1;
            //        }

            //    }
            //}

            // ----


            // 1. Initialize Wall Array
            int[,] leftWallBlocks = getWallArray(curRoomWallBounds.left.ElementAt(0).size.z+1, curRoomWallBounds.left.ElementAt(0).size.y+1);

            // 2. Remove Certain Blocks

            // Hole Test
            if (room.RoomType != RoomType.Corridor)
            {
                leftWallBlocks[2, 1] = 0;
                leftWallBlocks[2, 2] = 0;
                leftWallBlocks[5, 2] = 0;
                leftWallBlocks[5, 6] = 0;
            }


            Debug.Log($@"x: {leftWallBlocks.GetLength(0)}, y: {leftWallBlocks.GetLength(1)},
                        nBlocks:{leftWallBlocks.Length}
            ");

            // Left Chunks
            List<BoundsInt> chunkBounds = getChunkBounds(leftWallBlocks, curRoomWallBounds.left[0]);
            Debug.Log($"chunkList length {chunkBounds.Count}");
            curRoomWallBounds.left = chunkBounds;

            // Right Chunks
            int[,] rightWallBlocks = getWallArray(curRoomWallBounds.right.ElementAt(0).size.z + 1, curRoomWallBounds.right.ElementAt(0).size.y + 1);
            
            if (room.RoomType != RoomType.Corridor)
            {
                rightWallBlocks[2, 1] = 0;
                rightWallBlocks[2, 2] = 0;
                rightWallBlocks[5, 2] = 0;
                rightWallBlocks[5, 6] = 0;
            }

            chunkBounds = getChunkBounds(rightWallBlocks, curRoomWallBounds.right[0]);
            curRoomWallBounds.right = chunkBounds;


            // Forward Chunks

            // Back Chunks



            #region block Test
            //var nIterations2 = 0;

            //while (availableLeftPositions.Count > 0 && nIterations2 < 200)


            //{

            //    int curWidth = 0;
            //    int widthMax = int.MaxValue;
            //    int curHeight = 0;
            //    int heightMax = 0;

            //    var nIterations = 0;

            //    bool isPosFound = false;

            //    if (availableLeftPositions.ToList().Count() > 0)
            //    {

            //    //var startLinePos = availableLeftPositions.ToList()[0];

            //    int minPosSum = availableLeftPositions.ToList().Min(availablePos => (availablePos.z + availablePos.y));
            //    var startPos = availableLeftPositions.Where(availablePos => (availablePos.z + availablePos.y) == minPosSum).ElementAt(0);
            //    var curPos = availableLeftPositions.Where(availablePos => (availablePos.z + availablePos.y) == minPosSum).ElementAt(0);
            //        //Debug.Log($"nAv positions left {availableLeftPositions.ToList().Count()}");

            //        nIterations2++;

            //        while (nIterations < 100 && !isPosFound)
            //        {
            //            if (availableLeftPositions.Contains(curPos + Vector3Int.forward) && curWidth < widthMax)
            //            {
            //                curWidth++;
            //                curPos = curPos + Vector3Int.forward;

            //            }
            //            else if (availableLeftPositions.Contains(startPos + Vector3Int.up + curHeight * Vector3Int.up) && 0 <= curWidth)
            //            {
            //                curHeight++;
            //                curPos = startPos + Vector3Int.up + curHeight * Vector3Int.up;

            //                widthMax = curWidth;
            //                curWidth = 0;

            //            }
            //            else if (widthMax >= 0 && curHeight >= 0)
            //            {
            //                heightMax = curHeight;
            //                Debug.Log($"MaxPos found height{heightMax} width {widthMax}");

            //                var chunk = new BoundsInt(startPos, new Vector3Int(1, heightMax+1, widthMax+1));
            //                blockBounds.Add(chunk);

            //                foreach (var pos in chunk.allPositionsWithin)
            //                {
            //                    if (availableLeftPositions.Contains(pos))
            //                    {
            //                        availableLeftPositions.Remove(pos);
            //                    }
            //                }
            //                isPosFound = true;



            //            }

            //            nIterations++;

            //        }


            //    }
            //}
            #endregion




            //// Right 
            //foreach (var wall in curRoomWallBounds.right)
            //{
            //    foreach (var pos in wall.allPositionsWithin)
            //    {
            //        availableRightPositions.Add(pos);
            //        Debug.Log($"Avail Position {pos}");
            //    }
            //}

            //availableRightPositions.ExceptWith(doorPositions);

            //foreach (var availablePos in availableRightPositions)
            //{
            //    blockBounds.Add(new BoundsInt(
            //        availablePos,
            //        new Vector3Int(1, 1, 1)

            //    ));
            //}

            //curRoomWallBounds.right = blockBounds; // room.DoorPlacements.Select(doorPlacement=> doorPlacement.DoorBounds).ToList();

            ////curRoomWallBounds.left = room.DoorPlacements.Select(doorPlacement => doorPlacement.DoorBounds).ToList();


            #endregion


            #region Block Debugging
            //HashSet<Vector3Int> preAvailablePositions = new HashSet<Vector3Int>(availablePositions);
            //Debug.Log(@$"Are av the same: {availablePositions.Count() == preAvailablePositions.Count()}

            // ");
            //preAvailablePositions.ExceptWith(availablePositions);
            //Debug.Log(@$"PreAv Difference {preAvailablePositions.Count()}
            // ");


            //Vector3Int missedPos = new Vector3Int(0, 0, 0);


            //Debug.Log(@$"Is Poss Not Missed:
            //                {availablePositions.Contains(missedPos)}
            //                {missedPos}

            //            ");
            #endregion


            //foreach (var placement in room.DoorPlacements)
            //{

            //    //Debug.Log(placement.PositionType);

            //    List<BoundsInt> splitWalls = new List<BoundsInt>();

            //    switch (placement.PositionType)
            //    {
            //        case SplitPosition.Left:

            //            if (room.RoomType != RoomType.Corridor)
            //            {
            //                splitWalls = splitWall(placement.DoorBounds, curRoomWallBounds.left[0], SplitPosition.Left);
            //            }
            //            curRoomWallBounds.left = splitWalls;

            //            break;

            //        case SplitPosition.Right:

            //            if (room.RoomType != RoomType.Corridor)
            //            {
            //                splitWalls = splitWall(placement.DoorBounds, curRoomWallBounds.right[0], SplitPosition.Right);
            //            }

            //            curRoomWallBounds.right = splitWalls;
            //            break;

            //        case SplitPosition.Top:

            //            if (room.RoomType != RoomType.Corridor)
            //            {
            //                splitWalls = splitWall(placement.DoorBounds, curRoomWallBounds.top[0], SplitPosition.Top);
            //            }
            //            curRoomWallBounds.top = splitWalls;
            //            break;

            //        case SplitPosition.Bottom:

            //            if (room.RoomType != RoomType.Corridor)
            //            {
            //                splitWalls = splitWall(placement.DoorBounds, curRoomWallBounds.bottom[0], SplitPosition.Bottom);
            //            }
            //            curRoomWallBounds.bottom = splitWalls;
            //            break;


            //    }
            //}


            //// testing window positioning

            //if (room.RoomType != RoomType.Corridor)
            //{

            //    var wallLength = curRoomWallBounds.left[0].size.z;



            //    var wallsLeft = splitWall(new BoundsInt(Vector3Int.FloorToInt(curRoomWallBounds.left[0].center) - Vector3Int.one, Vector3Int.one * 2), curRoomWallBounds.left[0], SplitPosition.Left);
            //    curRoomWallBounds.left.RemoveAt(0);
            //    curRoomWallBounds.left.AddRange(wallsLeft);

            //    var wallRight = splitWall(new BoundsInt(Vector3Int.FloorToInt(curRoomWallBounds.right[0].center) - Vector3Int.one, Vector3Int.one * 2), curRoomWallBounds.right[0], SplitPosition.Right);
            //    curRoomWallBounds.right.RemoveAt(0);
            //    curRoomWallBounds.right.AddRange(wallRight);

            //    var wallsTop = splitWall(new BoundsInt(Vector3Int.FloorToInt(curRoomWallBounds.top[0].center) - Vector3Int.one, Vector3Int.one * 2), curRoomWallBounds.top[0], SplitPosition.Top);
            //    curRoomWallBounds.top.RemoveAt(0);
            //    curRoomWallBounds.top.AddRange(wallsTop);

            //    var wallsBottom = splitWall(new BoundsInt(Vector3Int.FloorToInt(curRoomWallBounds.bottom[0].center) - Vector3Int.one, Vector3Int.one * 2), curRoomWallBounds.bottom[0], SplitPosition.Bottom);
            //    curRoomWallBounds.bottom.RemoveAt(0);
            //    curRoomWallBounds.bottom.AddRange(wallsBottom);
            //}

            return curRoomWallBounds;

            //getWalls(wallThickness, room);

        }

        private List<BoundsInt> getChunkBounds(int[,] wallBlocks, BoundsInt curWallBounds)
        {
            List<BoundsInt> chunkBoundsList = new List<BoundsInt>();  

            // Loop Over each cell in Array
            for (int x = 0; x < wallBlocks.GetLength(0) - 1; x++)
            {
                for (int y = 0; y < wallBlocks.GetLength(1) - 1; y++)
                {
                    if (wallBlocks[x, y] == 1) // Check if current cell is a block 
                    {

                        // Intialize Search Parameters 
                        var isChunkFound = false;

                        var width = 0;
                        var height = 0;

                        var widthMax = wallBlocks.GetLength(0) - x;
                        var heightMax = 0;

                        Debug.Log($"x: {x}, y: {y}");

                        while (!isChunkFound) // While Full Chunk is not found
                        {

                            if (width < widthMax)
                            {

                                if (wallBlocks[x + width, y + height] == 1 && (x + width + 1) < wallBlocks.GetLength(0))
                                {
                                    width++;
                                }
                                else
                                {
                                    if (height > 0 && widthMax != (width))
                                    {
                                        isChunkFound = true;
                                        heightMax = height;


                                        Debug.Log($"CHUNK FOUND {widthMax}, {heightMax}");

                                        var chunk = new BoundsInt(
                                            curWallBounds.position + new Vector3Int(0, y, x),
                                            new Vector3Int(1, heightMax, widthMax));

                                        chunkBoundsList.Add(chunk);

                                        break; // reset the loop
                                    }
                                    else if (height == 0) // Width found at height 0 will be equal to the max width
                                    {

                                        widthMax = width;

                                    }


                                }


                            }
                            else
                            {

                                // Update the blocks to 0 only if a row has been completedc
                                for (int i = 0; i < widthMax; i++)
                                {
                                    wallBlocks[x + i, y + height] = 0;
                                }


                                if (wallBlocks[x, y + height + 1] == 1 && y + height + 2 < wallBlocks.GetLength(1))
                                {
                                    height++;
                                    width = 0;

                                }
                                else
                                {

                                    isChunkFound = true;
                                    heightMax = height + 1;

                                    var chunk = new BoundsInt(
                                        curWallBounds.position + new Vector3Int(0, y, x),
                                        new Vector3Int(1, heightMax, widthMax));
                                    chunkBoundsList.Add(chunk);

                                    break;

                                }
                            }
                        }
                    }
                }
            }

            return chunkBoundsList;
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

            // vertical case
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

                // top segment of the wall 
                splitWalls.Add(new BoundsInt(
                    new Vector3Int(
                        door.min.x,
                        door.min.y + door.size.y,
                        wall.min.z
                    ),
                    new Vector3Int(
                        door.size.x,
                        (wall.max.y - door.min.y) - door.size.y,
                        dungeonGenerator.wallThickness
                    )
                ));

                // add bottom segment if it exists 
                splitWalls.Add(new BoundsInt(
                  new Vector3Int(
                        door.min.x,
                        wall.min.y,
                        wall.min.z
                  ),
                  new Vector3Int(
                        door.size.x,
                        (door.min.y) - wall.min.y,
                        dungeonGenerator.wallThickness

                  )
                  ));

            }

            // horizontal case
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

                // top segmenet of the wall 
                splitWalls.Add(new BoundsInt(
                    new Vector3Int(
                        wall.min.x,
                        door.min.y + door.size.y,
                        door.min.z
                    ),
                    new Vector3Int(
                        dungeonGenerator.wallThickness,
                        (wall.max.y-door.min.y) - door.size.y,
                        door.size.z
                        
                    )
                ));

                // add bottom segment if it exists 
                splitWalls.Add(new BoundsInt(
                  new Vector3Int(
                      wall.min.x,
                      wall.min.y,
                      door.min.z
                  ),
                  new Vector3Int(
                      dungeonGenerator.wallThickness,
                      (door.min.y) - wall.min.y,
                      door.size.z

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

                BoundsInt wall1 = new BoundsInt(
                        new Vector3Int((int)bounds.min.x, (int)bounds.min.y, (int)bounds.min.z - dungeonGenerator.wallThickness),
                        horizontalWallSize);

                BoundsInt door1 = new BoundsInt(
                       new Vector3Int((int)((bounds.min.x + bounds.max.x) / 2f - dungeonGenerator.corridorWidth / 2f), 0, (int)((bounds.min.z + bounds.min.z) / 2f) - dungeonGenerator.wallThickness),
                       new Vector3Int(dungeonGenerator.corridorWidth, dungeonGenerator.corridorHeight, dungeonGenerator.wallThickness) //  - dungeonGenerator.wallThickness*2
                    );

                var splitWalls = splitWall(door1, wall1, SplitPosition.Top);
                doorBounds.Add(door1);
                wallBounds.bottom.AddRange(splitWalls);

                //wallBounds.bottom.Add(
                //    new BoundsInt(
                //        new Vector3Int((int)bounds.min.x, (int)bounds.min.y, (int)bounds.min.z - dungeonGenerator.wallThickness),
                //        horizontalWallSize)
                //    );


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