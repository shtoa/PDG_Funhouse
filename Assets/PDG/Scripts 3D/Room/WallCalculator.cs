using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
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
        private DungeonDecorator dungeonDecorator;

        public List<BoundsInt> WallBounds { get => wallBounds; set => wallBounds = value; }
        public List<BoundsInt> DoorBounds { get => doorBounds; set => doorBounds = value; }

        public enum WallPosition
        {
            left,
            right, 
            top, 
            bottom  
                
        }

        public WallCalculator(DungeonGenerator _dungeonGenerator, DungeonDecorator _dungeonDecortator)
        {
            dungeonGenerator = _dungeonGenerator;
            dungeonDecorator = _dungeonDecortator;
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

        // max 32 bits max height 32 
        public UInt32[] getWallIntArray(int sizeX, int sizeY) // generateBased on width
        {
            UInt32[] wallArray = new UInt32[sizeX];

            for (int column = 0; column < wallArray.GetLength(0); column++)
            {

                wallArray[column] = (UInt32)(UInt32.MaxValue >> (32 - sizeY));
                Debug.Log($"WallArrayVal: {wallArray[column]}");
            }

            return wallArray;
        }

        public void removeBlock(UInt32[] wallArray , int xLoc, int yLoc)
        {
            wallArray[xLoc] = (UInt32)(wallArray[xLoc] & ~(1 << yLoc));
            //return wallArray;
        }

        public void addWindows(UInt32[] wallArray, BoundsInt wallBounds, bool isTopBottom) // FIXME add a global and local wallbounds
        {
            Debug.Log($"Byte Array: {getWallIntArray(wallBounds.size.z + 1, wallBounds.size.y + 1)[0]}");

            for (int x = 1; x < wallArray.Length - 2; x++)
            {
                if (x % 2 == 0)
                {
                    for (int y = 3; y < 3 + 2; y++)
                    {
                        removeBlock(wallArray, x, y);
                    }

                    GameObject window = GameObject.Instantiate(dungeonDecorator.windowMesh, dungeonGenerator.gameObject.transform);
                    


                    if (isTopBottom)
                    {
                        window.transform.Rotate(0, 90, 0);
                        window.transform.position = new Vector3(x, 3, 0) + wallBounds.position + dungeonGenerator.transform.position + new Vector3(1.5f, 0, 1.5f);

                       
                    } else
                    {
                        window.transform.position = new Vector3(0, 3, x) + wallBounds.position + dungeonGenerator.transform.position + new Vector3(1.5f, 0, 1.5f);


                    }
                }
            }

        }

        public void addLights(UInt32[] wallArray, BoundsInt wallBounds, WallPosition wallPos) // FIXME add a global and local wallbounds
        {
            Debug.Log($"Byte Array: {getWallIntArray(wallBounds.size.z + 1, wallBounds.size.y + 1)[0]}");

            for (int x = 1; x < wallArray.Length - 2; x++)
            {
                if (x % 2 == 0)
                {

                    GameObject light = GameObject.Instantiate(dungeonDecorator.lightMesh, dungeonGenerator.gameObject.transform);


                    if (wallPos == WallPosition.top || wallPos == WallPosition.bottom)
                    {
                        light.transform.Rotate(0, 0, 90);
                        if (wallPos == WallPosition.bottom) light.transform.Rotate(0, 0, 180);
                        light.transform.position = new Vector3(x, 2, 0) + wallBounds.position + dungeonGenerator.transform.position + ((wallPos == WallPosition.top) ? new Vector3(1.5f, 0, 0.9f) : new Vector3(1.5f, 0, 2.1f));
                    }
                    else
                    {
                        if(wallPos == WallPosition.right) light.transform.Rotate(0, 0, 180); 
                        light.transform.position = new Vector3(0, 2, x) + wallBounds.position + dungeonGenerator.transform.position + ((wallPos == WallPosition.left) ? new Vector3(2.1f, 0, 1.5f)  : new Vector3(0.9f, 0, 1.5f));

                    }
                }
            }
        }

        public void addDoor(UInt32[] wallArray, BoundsInt wallBounds, BoundsInt doorBounds, bool isTopBottom)
        {
            for (int y = 0; y < dungeonGenerator.corridorHeight; y++)
            {
                removeBlock(wallArray, isTopBottom ? (doorBounds.position - wallBounds.position).x : (doorBounds.position - wallBounds.position).z, y);
            }
        }

        public void addDoor(UInt32[] wallArray, int x)
        {
            for (int y = 0; y < dungeonGenerator.corridorHeight; y++)
            {
                removeBlock(wallArray, x, y);
            }
        }
        public WallBounds CalculateWalls(Node room, int wallThickness) // FIX ME: For Chunks larger than 32
        {

            WallBounds curRoomWallBounds = CalculateWallBounds(wallThickness, room);
            bool isIntersected = false;



            ////1.Initialize Wall Array
            //int[,] leftWallBlocks = getWallArray(curRoomWallBounds.left.ElementAt(0).size.z + 1, curRoomWallBounds.left.ElementAt(0).size.y + 1);




            //// 2. Remove Certain Blocks

            //// Hole Test
            //if (room.RoomType != RoomType.Corridor)
            //{
            //    leftWallBlocks[2, 1] = 0;
            //    leftWallBlocks[2, 2] = 0;
            //    leftWallBlocks[5, 2] = 0;
            //    leftWallBlocks[5, 6] = 0;
            //}


            //Debug.Log($@"x: {leftWallBlocks.GetLength(0)}, y: {leftWallBlocks.GetLength(1)},
            //            nBlocks:{leftWallBlocks.Length}
            //");

            ////Left Chunks
            //List<BoundsInt> chunkBounds = getChunkBounds(leftWallBlocks, curRoomWallBounds.left[0]);
            //Debug.Log($"chunkList length {chunkBounds.Count}");

            if (room.RoomType != RoomType.Corridor)
            {

                // left 
                UInt32[] wallArrayLeft = getWallIntArray(curRoomWallBounds.left[0].size.z + 1, curRoomWallBounds.left[0].size.y);
                addWindows(wallArrayLeft, curRoomWallBounds.left[0], false);
                addLights(wallArrayLeft, curRoomWallBounds.left[0], WallPosition.left);
            

                // right 
                UInt32[] wallArrayRight = getWallIntArray(curRoomWallBounds.right[0].size.z + 1, curRoomWallBounds.right[0].size.y);
                addWindows(wallArrayRight, curRoomWallBounds.right[0], false);
                addLights(wallArrayRight, curRoomWallBounds.right[0], WallPosition.right);



                // bottom 
                UInt32[] wallArrayBottom = getWallIntArray(curRoomWallBounds.bottom[0].size.x + 1, curRoomWallBounds.bottom[0].size.y);
                addWindows(wallArrayBottom, curRoomWallBounds.bottom[0], true);
                addLights(wallArrayBottom, curRoomWallBounds.bottom[0], WallPosition.bottom);

                if (room.RoomType == RoomType.Start)
                {
                    addDoor(wallArrayBottom, wallArrayBottom.Length/2);
                }


                //// top 
                UInt32[] wallArrayTop = getWallIntArray(curRoomWallBounds.top[0].size.x + 1, curRoomWallBounds.top[0].size.y);
                addWindows(wallArrayTop, curRoomWallBounds.top[0], true);
                addLights(wallArrayTop, curRoomWallBounds.top[0], WallPosition.top);


                // Add doors
                if (room.RoomType != RoomType.Corridor)
                {
                    foreach (var placement in room.DoorPlacements)
                    {

                        //Debug.Log(placement.PositionType);

                        List<BoundsInt> splitWalls = new List<BoundsInt>();

                        switch (placement.PositionType)
                        {
                            case SplitPosition.Left:

                                addDoor(wallArrayLeft, curRoomWallBounds.left[0], placement.DoorBounds, false);
                                break;

                            case SplitPosition.Right:

                                addDoor(wallArrayRight, curRoomWallBounds.right[0], placement.DoorBounds, false);
                                break;

                            case SplitPosition.Top:

                                addDoor(wallArrayTop, curRoomWallBounds.top[0], placement.DoorBounds, true);
                                break;

                            case SplitPosition.Bottom:

                                addDoor(wallArrayBottom, curRoomWallBounds.bottom[0], placement.DoorBounds, true);
                                break;


                        }
                    }
                }


                // Fix ME: LoopAble Make Enumberatable
                List<BoundsInt> chunkBoundsLeft = getChunkIntBounds(wallArrayLeft, curRoomWallBounds.left[0], false);
                List<BoundsInt> chunkBoundsRight = getChunkIntBounds(wallArrayRight, curRoomWallBounds.right[0], false);
                List<BoundsInt> chunkBoundsBottom = getChunkIntBounds(wallArrayBottom, curRoomWallBounds.bottom[0], true);
                List<BoundsInt> chunkBoundsTop = getChunkIntBounds(wallArrayTop, curRoomWallBounds.top[0], true);

                curRoomWallBounds.left = chunkBoundsLeft;
                curRoomWallBounds.right = chunkBoundsRight;
                curRoomWallBounds.bottom = chunkBoundsBottom;
                curRoomWallBounds.top = chunkBoundsTop;
            }


            if (room.RoomType == RoomType.Corridor)
            {
                foreach (var placement in room.DoorPlacements)
                {

                    //Debug.Log(placement.PositionType);

                    List<BoundsInt> splitWalls = new List<BoundsInt>();

                    switch (placement.PositionType)
                    {
                        case SplitPosition.Left:

                            curRoomWallBounds.left.Clear();
                            curRoomWallBounds.right.Clear();

                            break;

                        case SplitPosition.Right:

                            curRoomWallBounds.left.Clear();
                            curRoomWallBounds.right.Clear();

                            break;

                        case SplitPosition.Top:

                            curRoomWallBounds.top.Clear();
                            curRoomWallBounds.bottom.Clear();

                            break;

                        case SplitPosition.Bottom:

                            curRoomWallBounds.top.Clear();
                            curRoomWallBounds.bottom.Clear();
                            break;


                    }
                }
            }


            // Right Chunks
            //int[,] rightWallBlocks = getWallArray(curRoomWallBounds.right.ElementAt(0).size.z + 1, curRoomWallBounds.right.ElementAt(0).size.y + 1);

            //if (room.RoomType != RoomType.Corridor)
            //{
            //    rightWallBlocks[2, 1] = 0;
            //    rightWallBlocks[2, 2] = 0;
            //    rightWallBlocks[5, 2] = 0;
            //    rightWallBlocks[5, 6] = 0;
            //}

            //chunkBounds = getChunkBounds(rightWallBlocks, curRoomWallBounds.right[0]);
            //curRoomWallBounds.right = chunkBounds;


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

            return curRoomWallBounds;

        }
        private List<BoundsInt> getChunkIntBounds(UInt32[] wallBlocks, BoundsInt curWallBounds, bool isTopBottom)
        {
            List<BoundsInt> chunkBoundsList = new List<BoundsInt>();

            // Loop Over each cell in Array
            for (int column = 0; column < wallBlocks.GetLength(0)-1; column++)
            {
                int maxIterations = 0;

          
                Debug.Log($"theColumn: {column}, Values: {wallBlocks[column]}");

                while (wallBlocks[column] != 0 && maxIterations < 4) { 
                UInt32 rowBlocks = wallBlocks[column];
                // get trailing zeros
                var y = math.tzcnt(rowBlocks);
                var maxHeight = math.tzcnt(~(rowBlocks >> y));

                    maxIterations++;

                    if (maxHeight > 0)
                    {
                        // construct mask of the height
                        var mask = ((UInt32)(UInt32.MaxValue >> (32 - maxHeight))) << y;
                        var width = 0;


                        Debug.Log($"Mask Next Column {wallBlocks[column + 1] & mask}, {mask}");

                        while ((wallBlocks[column + width] & mask) == mask)
                        {
                            if (width + column < wallBlocks.GetLength(0)-1)
                            {
                                wallBlocks[column + width] = ~mask & wallBlocks[column + width];
                                width++;
                                Debug.Log($"Width Contained {wallBlocks[column]}");

                            }
                            else
                            {
                                break;
                            }
                        }
                        var chunk = new BoundsInt(
                            curWallBounds.position + (isTopBottom ? new Vector3Int(column, y, 0) : new Vector3Int(0, y, column)),
                            (isTopBottom ? new Vector3Int(width, maxHeight, 1) : new Vector3Int(1, maxHeight, width)) 
                        );

                        chunkBoundsList.Add(chunk);
                        Debug.Log($"yPos {y},  maxHeight {maxHeight}, mask {mask}, width {width}, curColumn {wallBlocks[column]}");

                    }
                    
                }
            }

            

            return chunkBoundsList;
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
            //if (room.RoomType == RoomType.Start)
            //{

            //    BoundsInt wall1 = new BoundsInt(
            //            new Vector3Int((int)bounds.min.x, (int)bounds.min.y, (int)bounds.min.z - dungeonGenerator.wallThickness),
            //            horizontalWallSize);

            //    BoundsInt door1 = new BoundsInt(
            //           new Vector3Int((int)((bounds.min.x + bounds.max.x) / 2f - dungeonGenerator.corridorWidth / 2f), 0, (int)((bounds.min.z + bounds.min.z) / 2f) - dungeonGenerator.wallThickness),
            //           new Vector3Int(dungeonGenerator.corridorWidth, dungeonGenerator.corridorHeight, dungeonGenerator.wallThickness) //  - dungeonGenerator.wallThickness*2
            //        );

            //    var splitWalls = splitWall(door1, wall1, SplitPosition.Top);
            //    doorBounds.Add(door1);
            //    wallBounds.bottom.AddRange(splitWalls);

            //    //wallBounds.bottom.Add(
            //    //    new BoundsInt(
            //    //        new Vector3Int((int)bounds.min.x, (int)bounds.min.y, (int)bounds.min.z - dungeonGenerator.wallThickness),
            //    //        horizontalWallSize)
            //    //    );


            //}
            //else
            //{
                wallBounds.bottom.Add(
                    new BoundsInt(
                        new Vector3Int((int)bounds.min.x, (int)bounds.min.y, (int)bounds.min.z - dungeonGenerator.wallThickness),
                        horizontalWallSize)
                    );
                    
           // }

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