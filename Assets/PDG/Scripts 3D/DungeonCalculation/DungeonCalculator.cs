using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
using Codice.CM.SEIDInfo;

namespace dungeonGenerator
{
    public class DungeonCalculator
    {
        // Dungeon Properties 
        private int dungeonWidth;
        private int dungeonLength;
        private int dungeonHeight;
        public List<Node> RoomSpaces { get => roomSpaces; }

        private List<Node> roomSpaces;

        public void assignVoxelGrid(bool[,,] voxelGrid)
        {
            for(int x = 0; x < voxelGrid.GetLength(0); x++)
            {
                for(int y = 0; y < voxelGrid.GetLength(1); y++)
                {
                    for (int z = 0; z  < voxelGrid.GetLength(2); z++)
                    {
                        voxelGrid[x, y, z] = false;
                    }
                }
            }
        }
        public DungeonCalculator(BoundsInt dungeonDimensions)
        {

            // TODO: Further Refactor this
            this.dungeonWidth = dungeonDimensions.size.x;
            this.dungeonHeight = dungeonDimensions.size.y;
            this.dungeonLength = dungeonDimensions.size.z;
            
            
            Debug.Log($"Dungeon Height: {dungeonHeight}");
        }


        /* <summary>
        
            Calculates the Dungeon Floor Bounds given the Dungeon Width and Length
        
        </summary>*/

        public List<Node> CalculateDungeon(int maxIterations, BoundsInt roomBoundsMin, Vector3 splitCenterDeviationPercent, int corridorWidth, int wallThickness, Vector3Int roomOffset, int corridorHeight)
        {
            // Calculate the Dungeon Floor Bounds:

            // FIX ME:
            var roomWidthMin = roomBoundsMin.size.x;
            var roomLengthMin = roomBoundsMin.size.z;
            var roomHeightMin = roomBoundsMin.size.y;

            #region 1. Space Partitioning

            // 1.1 Generate BSP graph based on minRoomWidth, minRoomLength and maxIterations
            BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength, dungeonHeight); // initialize BSP class

            // Vector of Minimum Space need to accomodate given Room Size

            // TODO: Add check if this value is impossible
            var minSpaceDim = new Vector3Int(
                roomWidthMin + roomOffset.x + wallThickness * 2,
                roomLengthMin + roomOffset.y + wallThickness * 2,
                roomHeightMin + roomOffset.z

            );

            var allNodeSpaces = bsp.PartitionSpace(maxIterations, minSpaceDim, splitCenterDeviationPercent);  // include roomOffset and wallThickness to have correct placement


            // 1.2 Extract the leaves, which represent the possible room positions (BSP Step May lead to bsp overrliance)
            var roomSpaces = GraphHelper.GetLeaves(bsp.RootNode);
            this.roomSpaces = roomSpaces;


            #endregion

            #region 2. Room Placement

            // TODO: Make this be passed into the generator
            var minRoomDim = new Vector3Int(
                roomWidthMin,
                roomLengthMin,
                roomHeightMin
            );

            // FIXME: Check if this value is correct
            var totalRoomOffset = roomOffset + new Vector3Int(1, 1, 0) * wallThickness * 2;

            // 2.1 Place rooms within the possible room bounds taking into account room offset
            RoomCalculator roomGenerator = new RoomCalculator(minRoomDim, totalRoomOffset, corridorWidth); // FIXME: Make more general remove corriodr width dependency
            var rooms = roomGenerator.PlaceRoomsInSpaces(roomSpaces);

            #endregion

            #region 3. Generate Corridors

            CorridorGenerator corridorGenerator = new CorridorGenerator();



            bool[,,] availableVoxelGrid = new bool[dungeonWidth, dungeonHeight, dungeonLength];
            // 0 is available space
            // 1 is taken space

            // assign voxel grid
            assignVoxelGrid(availableVoxelGrid);
            Debug.Log($"Voxel Grid: X: {availableVoxelGrid.GetLength(0)}, Y: {availableVoxelGrid.GetLength(1)}, Z: {availableVoxelGrid.GetLength(2)}");

            // remove room spaces
            removeRoomsFromVoxelGrid(availableVoxelGrid, rooms);
            //visualizeVoxelGrid(availableVoxelGrid);





            var corridorList = corridorGenerator.CreateCorridors(allNodeSpaces, corridorWidth, wallThickness, minRoomDim, corridorHeight, availableVoxelGrid);
            visualizeVoxelGrid(availableVoxelGrid);


            #endregion

            // return a list of bounds on which the floor will be (Rooms and Corridors) 
            return new List<Node>(rooms).ToList(); // .Concat(corridorList)
        }

        private void removeRoomsFromVoxelGrid(bool[,,] availableVoxelGrid, List<SpaceNode> rooms)
        {

            foreach(var room in rooms)
            {
                Vector3Int startPos = room.Bounds.position;
                Vector3Int maxPos = room.Bounds.max;

                for(int x = startPos.x; x < maxPos.x; x++)
                {
                    for(int y = startPos.y; y < maxPos.y; y++)
                    {
                        for (int z = startPos.z; z < maxPos.z; z++)
                        {
                            availableVoxelGrid[x, y, z] = true;
                        }

                    }

                }
            }
        }


        private void visualizeVoxelGrid(bool[,,] availableVoxelGrid)
        {

           

            GameObject voxelHolder = new GameObject("voxelGridVisualizer");
            voxelHolder.transform.parent = GameObject.Find("DungeonGen").transform;
                for (int x = 0; x < availableVoxelGrid.GetLength(0); x++)
                {
                    for (int y = 0; y < availableVoxelGrid.GetLength(1); y++)
                    {
                        for (int z = 0; z < availableVoxelGrid.GetLength(2); z++)
                        {

                            if (availableVoxelGrid[x, y, z])
                            {
                                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                cube.transform.parent = voxelHolder.transform;
                                cube.transform.localPosition = new Vector3Int(x, y, z);
                            }

                        }

                    }

                }
            }
        }
}


