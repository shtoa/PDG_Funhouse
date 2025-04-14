using Codice.CM.SEIDInfo;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;
using Random = UnityEngine.Random;

namespace dungeonGenerator
{
    // Pathfinding based Corridor Connections
    public class CorridorNodePath : Node

    {
        // nodes to connect
        private Node node1;
        private Node node2;

        // room properties
        private int corridorWidth;
        private int wallThickness;
        private Vector3Int minRoomDim;
        private int corridorHeight;

        // grids to track occupied spaces
        private bool[,,] availableVoxelGrid;
        private bool[,,] corridorGrid;

        // directions surrounding a point
        private List<Vector3Int> possibleDir = new List<Vector3Int> {
           new Vector3Int(1, 0, 0),
           new Vector3Int(1, 0, 1),
           new Vector3Int(-1, 0, 1),
           new Vector3Int(1, 0, -1),
           new Vector3Int(0, 0, -1),
           new Vector3Int(-1, 0, 0),
           new Vector3Int(0, 0, 1),
           new Vector3Int(-1, 0, -1)
        };

        // possible directions for pathfinding 
        private List<Vector3Int> pathPossibleDirections = new List<Vector3Int>{
            Vector3Int.left,
            Vector3Int.right,
            Vector3Int.forward,
            Vector3Int.back,
        };

        public CorridorNodePath(Node node1, Node node2, int corridorWidth, int wallThickness, Vector3Int minRoomDim, int corridorHeight, bool[,,] availableVoxelGrid) : base(null) // null since it doesnt have any parents
        {
            // nodes to connect
            this.node1 = node1;
            this.node2 = node2;

            // corridor properties
            this.corridorWidth = corridorWidth;
            this.wallThickness = wallThickness;
            this.minRoomDim = minRoomDim;
            this.corridorHeight = corridorHeight;

            // grids to track occupied spaces
            this.availableVoxelGrid = availableVoxelGrid;
            this.corridorGrid = getCorridorGrid(availableVoxelGrid);

            // generate corridor bounds for nodes
            GenerateCorridor();
        }

        #region Corridor Pathfinding Methods
        private bool[,,] getCorridorGrid(bool[,,] availableVoxelGrid){

            bool[,,] corridorVoxelGrid = new bool[availableVoxelGrid.GetLength(0), availableVoxelGrid.GetLength(1), availableVoxelGrid.GetLength(2)];

            for (int x = 0; x < availableVoxelGrid.GetLength(0); x++)
            {
                for (int y = 0; y < availableVoxelGrid.GetLength(1); y++)
                {
                    for (int z = 0; z < availableVoxelGrid.GetLength(2); z++)
                    {
                        corridorVoxelGrid[x, y, z] = false;
                    }
                }

            }

            return corridorVoxelGrid;
        }
        private void calculateWallsFromCorridorTest(SplitPosition splitPosition)
        {
            Vector3Int minPos = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
            Vector3Int maxPos = Vector3Int.zero;

            // find Min and Max Positions in the Corridors

            foreach (BoundsInt bound in CorridorBoundsList)
            {

                minPos = Vector3Int.Min(minPos, bound.min);     // find Min
                maxPos = Vector3Int.Max(maxPos, bound.max);     // find Max

            }

            minPos = minPos - new Vector3Int(1, 0, 1); // wall offset
            maxPos = maxPos + new Vector3Int(2, 0, 2); // wall offset

            Vector3Int size = maxPos - minPos;

            // Generate to 
            bool[,] corridorArray = new bool[size.x, size.z];
            bool[,] wallArray = new bool[size.x, size.z];


            foreach (BoundsInt bound in CorridorBoundsList)
            {
                Vector3Int startPos = bound.position;
                Vector3Int mPos = bound.size;

                for (int x = (startPos.x - minPos.x); x < mPos.x + (startPos.x - minPos.x); x++)
                {
                    for (int z = (startPos.z - minPos.z); z < mPos.z + (startPos.z - minPos.z); z++)
                    {

                        corridorArray[x, z] = true;

                    }

                }
            }

            // loop over all corridor Array positions and populate wallArray
            for (int x = 0; x < corridorArray.GetLength(0); x++)
            {
                for (int z = 0; z < corridorArray.GetLength(1); z++)
                {
                    //  check if in corridor space is taken 
                    if (corridorArray[x, z] == true)
                    {

                        // Check all neighbouring cells 
                        foreach (Vector3Int dir in possibleDir)
                        {
                            Debug.Log($"WALL ARRAY NOT WORK {dir.ToString()}");



                            // issue with wall condition not allowing for generation of walls in certain places
                            //bool wallCondition = ((splitPosition == SplitPosition.Left) ? (x + dir.x > 0 && x + dir.x < (corridorArray.GetLength(0) - 2) && z + dir.z >= 0 && (z + dir.z) < corridorArray.GetLength(1)) :
                            //    (x + dir.x >= 0 && x + dir.x < (corridorArray.GetLength(0) - 1) && z + dir.z > 1 && (z + dir.z) < corridorArray.GetLength(1) - 2));


                            // wallCondition = true;

                            if (!this.availableVoxelGrid[x + dir.x + minPos.x, minPos.y, z + dir.z + minPos.z]) // change the y value based on the corriodr height
                            {

                                if (corridorArray[x + dir.x, z + dir.z] == false)
                                {
                                    Debug.Log("WALL ARRAY WORK");
                                    wallArray[x + dir.x, z + dir.z] = true;
                                }
                            }
                        }



                    }

                }
            }


            // convert wall Array to bounds 
            for (int x = 0; x < wallArray.GetLength(0); x++)
            {
                for (int z = 0; z < wallArray.GetLength(1); z++)
                {
                    if (wallArray[x, z] == true)
                    {

                        Vector3Int startPos = new Vector3Int(x, 0, z);
                        Vector3Int curPos = new Vector3Int(x, 0, z);

                        int width = 0;
                        int i = 0;

                        // get maxwidth

                        while (wallArray[curPos.x, curPos.z] == true && i < 100) //  && width < (wallArray.GetLength(0)-1)
                        {
                            wallArray[curPos.x, curPos.z] = false;
                            curPos = curPos + Vector3Int.right; // get Max Width of segmenet 
                            width++;
                            i++;


                            Debug.Log($"CurPos Width: {curPos.ToString()}, Wall Array Size x:{wallArray.GetLength(0)} y: {wallArray.GetLength(1)}");
                        }

                        Debug.Log($"WIDTH OF THE SEGMENT: {width}");

                        // get max height
                        int height = 1;
                        i = 0;
                        bool isWidthFound = true;
                        curPos = new Vector3Int(x, 0, z) + Vector3Int.forward;


                        while (wallArray[curPos.x, curPos.z] == true && i < 100 && isWidthFound)
                        {

                            for (int j = 1; j < width; j++)
                            {
                                curPos = startPos + j * Vector3Int.right + Vector3Int.forward * (height);
                                if (wallArray[curPos.x, curPos.z] == false)
                                {
                                    isWidthFound = false;
                                    break;
                                }
                            }

                            if (isWidthFound == true)
                            {
                                for (int j = 1; j <= width; j++)
                                {

                                    wallArray[curPos.x, curPos.z] = false;
                                    curPos = startPos + j * Vector3Int.right + Vector3Int.forward * (height);

                                }
                            }


                            if (height < (wallArray.GetLength(1) - 1) && isWidthFound)
                            {
                                height++;
                            }
                            else
                            {
                                break;
                            }


                            curPos = startPos + 0 * Vector3Int.right + Vector3Int.forward * height;

                            i++;

                            Debug.Log($"CurPos Height: {curPos.ToString()}, Wall Array Size x:{wallArray.GetLength(0)} y: {wallArray.GetLength(1)}");


                        }

                        //Debug.Log($"FINAL SEGMENT: width: {width}, height: {height}");
                        BoundsInt floorBounds = new BoundsInt(new Vector3Int(x + minPos.x, minPos.y, z + minPos.z), new Vector3Int(width, corridorHeight, height));
                        Debug.Log("THe Wall Segment BOunds" + floorBounds.ToString());
                        CorridorWallBoundsList.Add(floorBounds);

                    }
                }
            }
        }
        private void generateCorridorFloorBounds(int y)
        {
            for (int x = 0; x < this.availableVoxelGrid.GetLength(0); x++)
            {
                //for (int y = 0; y < this.availableVoxelGrid.GetLength(1); y++)
                //{
                for (int z = 0; z < this.availableVoxelGrid.GetLength(2); z++)
                {


                    if (this.corridorGrid[x, y, z] == true)
                    {

                        Vector3Int startPos = new Vector3Int(x, y, z);
                        Vector3Int curPos = new Vector3Int(x, y, z);

                        int width = 0;
                        int i = 0;

                        // get maxwidth

                        while (this.corridorGrid[curPos.x, curPos.y, curPos.z] == true && i < 100)
                        {
                            this.corridorGrid[curPos.x, curPos.y, curPos.z] = false;
                            curPos = curPos + Vector3Int.right; // get Max Width of segmenet 
                            width++;
                            i++;
                        }

                        Debug.Log($"WIDTH OF THE SEGMENT: {width}");

                        // get max height
                        int height = 1;
                        i = 0;
                        bool isWidthFound = true;
                        curPos = new Vector3Int(x, y, z) + Vector3Int.forward;


                        while (this.corridorGrid[curPos.x, curPos.y, curPos.z] == true && i < 100 && isWidthFound)
                        {

                            for (int j = 1; j < width; j++)
                            {
                                this.corridorGrid[curPos.x, curPos.y, curPos.z] = false;
                                curPos = startPos + j * Vector3Int.right + Vector3Int.forward * height;

                                if (this.corridorGrid[curPos.x, curPos.y, curPos.z] == false)
                                {
                                    isWidthFound = false;
                                }
                            }

                            if (isWidthFound == true)
                            {
                                for (int j = 1; j <= width; j++)
                                {
                                    this.corridorGrid[curPos.x, curPos.y, curPos.z] = false;
                                    curPos = startPos + j * Vector3Int.right + Vector3Int.forward * height;

                                }
                            }




                            height++;
                            curPos = startPos + 0 * Vector3Int.right + Vector3Int.forward * height;

                            i++;


                        }

                        Debug.Log($"FINAL SEGMENT: width: {width}, height: {height}");
                        BoundsInt floorBounds = new BoundsInt(new Vector3Int(x, y, z), new Vector3Int(width, corridorHeight, height));
                        Debug.Log("THe floor BOunds" + floorBounds.ToString());
                        CorridorBoundsList.Add(floorBounds);




                    }

                }

                //}

            }
        }
        private void updateAvailableGrid()
        {
            for (int x = 0; x < this.availableVoxelGrid.GetLength(0); x++)
            {
                for (int y = 0; y < this.availableVoxelGrid.GetLength(1); y++)
                {
                    for (int z = 0; z < this.availableVoxelGrid.GetLength(2); z++)
                    {

                        this.availableVoxelGrid[x, y, z] = this.availableVoxelGrid[x, y, z] || this.corridorGrid[x, y, z];

                    }

                }

            }
        }

        #endregion

        /// <summary>
        /// Generate Corridor Based on relative Room Alignment
        /// </summary>
        private void GenerateCorridor()
        {
            switch (this.node1.SplitPosition)
            {
                case SplitPosition.Top:
                    GenerateCorridorTopBottom(this.node1, this.node2);
                    break;
                case SplitPosition.Bottom:
                    GenerateCorridorTopBottom(this.node2, this.node1);
                    break;
                case SplitPosition.Right:
                    GenerateCorridorRightLeft(this.node1, this.node2);
                    break;
                case SplitPosition.Left:
                    GenerateCorridorRightLeft(this.node2, this.node1);
                    break;
                case SplitPosition.Up:
                    GenerateCorridorUpDown(this.node1, this.node2);
                    break;
                case SplitPosition.Down:
                    GenerateCorridorUpDown(this.node2, this.node1);
                    break;

                default:
                    Debug.Log("UNKOWN TYPE");
                    Bounds = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(1, 1, 1));
                    break;
            }
        }

        #region Helper Methods
        /// <summary>
        /// Calculate MidPoint Function
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>

        public Vector3Int CalculateMiddlePoint(Vector3Int a, Vector3Int b)
        {
            var res = (Vector3)(a + b) / 2f;
            return Vector3Int.FloorToInt(res); // FIX ME: does this cause rounding errors?
        }
        #endregion

        #region Left-Right Generation

        #region Left-Right Helper Methods
        private List<Node> ReturnRightMostSpaces(List<Node> leftSpaces)
        {

            var sortedLeftSpaces = leftSpaces.OrderByDescending(space => space.Bounds.max.x).ToList(); // get right most children of left space

            if (sortedLeftSpaces.Count == 1)
            {
                return sortedLeftSpaces; // if only one leftSpace available select it (usually will just join the two deepest children)
            }
            else // select one of the rights most LeftSpaces if there are multiple
            {
                int maxX = sortedLeftSpaces[0].Bounds.max.x; // get the coordinates of the right most bound
                sortedLeftSpaces = sortedLeftSpaces.Where(space => Math.Abs(maxX - space.Bounds.max.x) < minRoomDim.x).ToList(); // deviation less than min room size to not go through rooms
                return sortedLeftSpaces;
            }

        }
        private List<Node> ReturnLeftMostSpaces(List<Node> rightSpaces)
        {

            var sortedRightSpaces = rightSpaces.OrderBy(space => space.Bounds.min.x).ToList(); // get right most children of left space

            if (sortedRightSpaces.Count == 1)
            {
                return sortedRightSpaces; // if only one rightSpace available select it (usually will just join the two deepest children)
            }
            else // select one of the left most RightSpaces if there are multiple
            {
                int minX = sortedRightSpaces[0].Bounds.min.x; // get the coordinates of the right most bound
                sortedRightSpaces = sortedRightSpaces.Where(space => Math.Abs(minX - space.Bounds.min.x) < minRoomDim.x).ToList(); // deviation less than min room size to not go through rooms
                return sortedRightSpaces;
            }

        }

        // FIX ME: Write Function to Infer Relative Position <<<<< Cant use Split position
        // FIX ME: Rewrite in less verbos way
        public LateralCorridorType GetLateralCorridorType(Node node1, Node node2, SplitPosition node1Spit)
        {
            Vector3 pos1 = new Vector3(0, 0, 0);
            Vector3 pos2 = new Vector3(0, 0, 0);

            if (node1.Bounds.min.y == node2.Bounds.min.y) {
                // if node1 and node2 have same min y position return corridor as type of room
                return LateralCorridorType.Corridor;

            } else {

                // get what directions nodes are split in 
                if (node1Spit == SplitPosition.Left || node1Spit == SplitPosition.Right)
                {
                    Debug.Log("SPLIT POSITION LEFT FOR CORRIDOR TYPE CHECK");
                    if (node1Spit == SplitPosition.Left)
                    {
                        // Left - Right 

                        pos1 = new Vector3(node1.Bounds.max.x, node1.Bounds.min.y, 0); // Left
                        pos2 = new Vector3(node2.Bounds.min.x, node2.Bounds.min.y, 0); // Right
                    } else
                    {
                        // Right - Left 
                        pos1 = new Vector3(node1.Bounds.min.x, node1.Bounds.min.y, 0); // Right
                        pos2 = new Vector3(node2.Bounds.max.x, node2.Bounds.min.y, 0); // Left
                    }

                } else
                {

                    if (node1Spit == SplitPosition.Top)
                    {
                        // Top - Bottom
                        pos1 = new Vector3(0, node1.Bounds.min.y, node1.Bounds.min.z); // Top
                        pos2 = new Vector3(0, node2.Bounds.min.y, node2.Bounds.max.z); // Bottom
                    }
                    else
                    {
                        // Bottom - Top
                        pos1 = new Vector3(0, node1.Bounds.min.y, node1.Bounds.max.z); // Bottom
                        pos2 = new Vector3(0, node2.Bounds.min.y, node2.Bounds.min.z); // Top
                    }

                }

            }

            Debug.Log($"Angle {Vector3.Angle(new Vector3(1, 0, 0), (pos2 - pos1).normalized)}");

            Debug.Log($"Pos1 {pos1}");
            Debug.Log($"Pos2 {pos2}");
            Debug.Log($"SplitPosition {node1.SplitPosition}");

            // Return Stairs and Ladders based on angle 
            if (Mathf.Abs(Vector3.Angle(new Vector3(1, 0, 0), (pos2 - pos1).normalized)) <= 30)
            {

                // if angle between node1 and node2 minimum position is less or equal to 45 create stairs
                return LateralCorridorType.Stairs;

            }
            else
            {
                // if angle between node1 and node 2 minimum y position is more than 45 create a ladder connection
                return LateralCorridorType.Ladder;
            }


        }

        #endregion

        private void removeConnectedSpaces(List<Node> Spaces, SplitPosition doorPosition)
        {
            for (int i = Spaces.Count() - 1; i >= 0; i--)
            {
                foreach (var doorPlacement in Spaces[i].DoorPlacements)
                {
                    if (doorPlacement.PositionType == doorPosition)
                    {
                        Spaces.RemoveAt(i);
                    }
                }
            }
        }

        private (Node startNode, Node endNode) getLateralNeighbors(List<Node>startNodes, List<Node> endNodes)
        {
            Node startNode = startNodes[0];

            // Return enumerable to store rightSpaceNeighbors to be lazy evaluated
            IEnumerable<Node> endNodeNeighbors = endNodes.OrderBy(endNode =>
                                                                              Vector3Int.Distance(endNode.Bounds.position, startNode.Bounds.position)
                                                                              ).Where(endNode => (endNode.Bounds.y == startNode.Bounds.y));

            while (endNodeNeighbors.Count() == 0 && startNodes.Count() > 0)
            {
                startNodes.Remove(startNode);
                startNode = startNodes[Random.Range(0, startNodes.Count())];
            }

            if (startNodes.Count() == 0 && endNodeNeighbors.Count() > 0)
            {
                return (null, null);
            }

            Node endNode = endNodeNeighbors.First();
            return (startNode, endNode);
            
        }

        /// <summary>
        /// Generate a corridor between leftNode and rightNode
        /// </summary>
        /// <param name="node1"> leftNode </param>
        /// <param name="node2"> rightNode </param>
        private void GenerateCorridorRightLeft(Node rightNode, Node leftNode)
        {
            // --- Initialize Leaves Arrays and Spaces to Connect ---

            Node leftSpace = null; // left space to connect
            List<Node> leftSpaceLeaves = GraphHelper.GetLeaves(leftNode); // get all the leaves in the left space

            Node rightSpace = null; // right space to connect
            List<Node> rightSpaceLeaves = GraphHelper.GetLeaves(rightNode); // get all the leaves in the right space

            // --- select (right most) LeftSpace and (left most) RightSpace to Connect ---

            var sortedLeftSpaces = ReturnRightMostSpaces(leftSpaceLeaves);
            var sortedRightSpaces = ReturnLeftMostSpaces(rightSpaceLeaves);

            // --- Check already connected spaces 
            removeConnectedSpaces(sortedRightSpaces, SplitPosition.Left);
            removeConnectedSpaces(sortedLeftSpaces, SplitPosition.Right);

            if (sortedLeftSpaces.Count() < 1 || sortedRightSpaces.Count() < 1)
            {
                this.CorridorType = CorridorType.None;
                return;
            }

            // --- Find Neighbor pair in LeftSpaces and RightSpaces ---

            (leftSpace,rightSpace) = getLateralNeighbors(sortedLeftSpaces, sortedRightSpaces);

            if (leftSpace == null && rightSpace == null)
            {
                this.CorridorType = CorridorType.None;
                return;
            }

            // calculate door positions
            Vector3Int leftStartVoxel = new Vector3Int(leftSpace.Bounds.max.x, leftSpace.Bounds.min.y, (leftSpace.Bounds.min.z + leftSpace.Bounds.max.z) / 2);
            this.corridorGrid[leftStartVoxel.x, leftStartVoxel.y, leftStartVoxel.z] = true;

            Vector3Int rightEndVoxel = new Vector3Int(rightSpace.Bounds.min.x - Mathf.CeilToInt(corridorWidth / 2f), rightSpace.Bounds.min.y, (rightSpace.Bounds.min.z + rightSpace.Bounds.max.z) / 2);
            this.corridorGrid[rightEndVoxel.x, rightEndVoxel.y, rightEndVoxel.z] = true;

            leftSpace.calculateDoorPlacement(new BoundsInt(
                    leftStartVoxel + Vector3Int.back,
                    new Vector3Int(1, 1, 1)

                ), SplitPosition.Left, wallThickness);
            rightSpace.calculateDoorPlacement(new BoundsInt(
                  rightEndVoxel + Vector3Int.back,
                  new Vector3Int(1, 1, 1)




                ), SplitPosition.Right, wallThickness);

            this.addDoorPlacement(leftSpace.DoorPlacements.Last());
            this.addDoorPlacement(rightSpace.DoorPlacements.Last());

            if (buildCorridorLateral(leftStartVoxel,rightEndVoxel))
            {
                // update available Grid
                updateAvailableGrid();

                // generate corridorBounds from corridor Grid 
                generateCorridorFloorBounds(rightSpace.Bounds.y);
                calculateWallsFromCorridorTest(SplitPosition.Left);

                // --- Add Neighbours to the Connection List of Respective Nodes --- 
                leftSpace.ConnectionsList.Add(rightSpace);
                rightSpace.ConnectionsList.Add(leftSpace);

                // --- calculate the bounds of the door to be used in mesh generation ---
                this.CorridorType = CorridorType.Horizontal;
            }

        }

        #endregion

        #region Top-Bottom Generation

        #region Top-Bottom Helpers
        private List<Node> ReturnBottomMostSpaces(List<Node> TopSpaces)
        {

            var sortedTopSpace = TopSpaces.OrderBy(space => space.Bounds.min.z).ToList(); // get bottom most children of top space

            if (sortedTopSpace.Count == 1)
            {
                return sortedTopSpace; // if only one topSpace available select it (usually will just join the two deepest children)
            }
            else // select one of the bottom most topSpace if there are multiple
            {
                int minZ = sortedTopSpace[0].Bounds.min.z; // get the coordinates of the bottom most bound
                sortedTopSpace = sortedTopSpace.Where(space => Math.Abs(minZ - space.Bounds.min.z) < minRoomDim.y).ToList(); // deviation less than min room size to not go through rooms
                return sortedTopSpace;
            }

        }
        private List<Node> ReturnTopMostSpaces(List<Node> BottomSpaces)
        {

            var sortedBottomSpace = BottomSpaces.OrderByDescending(space => space.Bounds.max.z).ToList(); // get Top most children of Bottom space

            if (sortedBottomSpace.Count == 1)
            {
                return sortedBottomSpace; // if only one bottomSpace available select it (usually will just join the two deepest children)
            }
            else // select one of the top most BottomSpaces if there are multiple
            {
                int maxZ = sortedBottomSpace[0].Bounds.max.z; // get the coordinates of the top most bound
                sortedBottomSpace = sortedBottomSpace.Where(space => Math.Abs(maxZ - space.Bounds.max.z) < minRoomDim.y).ToList(); // deviation less than min room size to not go through rooms
                return sortedBottomSpace;
            }

        }
        #endregion
        // Return Success
        public bool buildCorridorLateral(Vector3Int startVoxel, Vector3Int endVoxel)
        {

            Vector3Int startPos = startVoxel;
            int i = 0; // prevent looping incase corridor isnt being found

            // build the path by building corridor grid
            while (startPos != endVoxel && i < 100) 
            {
                // get possible postions
                List<Vector3Int> possiblePositions = new List<Vector3Int>();

                foreach (Vector3Int dir in pathPossibleDirections) // loop over possible path direction
                {
                    Vector3Int curPos = startPos + dir; // new possible direction (taken to be the center)
                    bool isOverlapFree = true;

                    // check if dimension is 2 (top-bottom) or 0 (right-left)
                    int dimension = 2 * Mathf.Abs((int)Vector3.Dot(dir, Vector3.forward));
                    Vector3Int perpendicularDir = (dimension != 0) ? Vector3Int.right : Vector3Int.forward; // find perpendicular vector


                    for (int centerOffset = -corridorWidth / 2; centerOffset < corridorWidth / 2; centerOffset++)
                    {

                        Vector3Int testPos = curPos + centerOffset * perpendicularDir; // test position perpendicular to path for crossing

                        // check if position within grid bounds
                        bool isPosInBounds = Vector3.Dot(perpendicularDir, testPos) > 0 
                                             && Vector3.Dot(perpendicularDir, testPos) < this.corridorGrid.GetLength(dimension); 

                        // check if position within dungeon bounds then check if position is occuppied (possible using logical short-ciruiting)
                        if (isPosInBounds && this.availableVoxelGrid[testPos.x, testPos.y, testPos.z])
                        {
                            isOverlapFree = false; break;
                        }
                    }
                   
                    // if all positions within path are free add to possible positions list
                    if (isOverlapFree) possiblePositions.Add(curPos);
                    
                }

                if (possiblePositions.Count() < 1) throw new Exception("Path not possible"); // if no further positions available path fails

                // ---- find closest position to the exit door ---- 

                int closestDistance = int.MaxValue;
                Vector3Int closestPos = new Vector3Int();

                foreach (Vector3Int possiblePosition in possiblePositions)
                {
                    // Compute manhattan distance
                    int curDistance = MeshHelper.ManhattanDistance3(possiblePosition, endVoxel); // find manhattan distance between end and current position
                    if (curDistance < closestDistance)
                    {
                        closestDistance = curDistance;
                        closestPos = possiblePosition;
                    }

                }

                // ---- Update Corridor Grid ---- 

                // remove points around point up to corridor height
                for (int height = 0; height < corridorHeight; height++) {

                    // remove closest position from available positions
                    this.corridorGrid[closestPos.x, closestPos.y+height, closestPos.z] = true;

                    foreach (Vector3Int dir in pathPossibleDirections) {
                        Vector3Int resultingPos = closestPos + dir;
                        this.corridorGrid[resultingPos.x, resultingPos.y + height, resultingPos.z] = true;
                    }
                }

                // change starting position to new position
                startPos = closestPos;
                i++; // increment iterator to prevent loops

            }

            // ---- checks if generation has failed or not ---- 

            if(startPos != endVoxel)
            {
                this.CorridorType = CorridorType.None;
                Debug.LogWarning("Additional Path not completed");
                return false;
            }

            return true;

        }
        private void GenerateCorridorTopBottom(Node topNode, Node bottomNode)
        {
            // --- Initialize Leaves Arrays and Spaces to Connect ---

            Node topSpace = null; // left space to connect
            List<Node> topSpaceLeaves = GraphHelper.GetLeaves(topNode); // get all the leaves in the left space

            Node bottomSpace = null; // right space to connect
            List<Node> bottomSpaceLeaves = GraphHelper.GetLeaves(bottomNode); // get all the leaves in the right space

            // --- select (bottom most) TopSpace and (top most) BottomSpace to Connect ---

            var sortedTopSpaces = ReturnBottomMostSpaces(topSpaceLeaves);
            var sortedBottomSpaces = ReturnTopMostSpaces(bottomSpaceLeaves);

            // --- Remove Already Connected Spaces ---

            removeConnectedSpaces(sortedBottomSpaces, SplitPosition.Top);
            removeConnectedSpaces(sortedTopSpaces, SplitPosition.Bottom);

            if (sortedTopSpaces.Count() < 1 || sortedBottomSpaces.Count() < 1)
            {
                this.CorridorType = CorridorType.None;
                return;
            }

            // --- Find Neighbor pair in TopSpaces and BottomSpaces ---

            topSpace = sortedTopSpaces[0]; // Choose starting space
            IEnumerable<Node> bottomSpaceNeighbors = sortedBottomSpaces.OrderBy(bottomSpace => 
                                                                                Vector3Int.Distance(bottomSpace.Bounds.position, topSpace.Bounds.position)
                                                                                ).Where((bottomSpace) => (topSpace.Bounds.y == bottomSpace.Bounds.y));

            while (bottomSpaceNeighbors.Count() == 0 && sortedTopSpaces.Count() > 0)
            {                         
                sortedTopSpaces.Remove(topSpace);
                topSpace = sortedTopSpaces[Random.Range(0, sortedTopSpaces.Count())];
            }

            if (sortedTopSpaces.Count() == 0)
            {
                this.CorridorType = CorridorType.None;
                return;
            }

            bottomSpace = bottomSpaceNeighbors.First();

            // calculate door positions

            Vector3Int bottomStartVoxel = new Vector3Int((bottomSpace.Bounds.min.x + bottomSpace.Bounds.max.x) / 2, bottomSpace.Bounds.min.y, bottomSpace.Bounds.max.z);
            this.corridorGrid[bottomStartVoxel.x, bottomStartVoxel.y, bottomStartVoxel.z] = true;

            Vector3Int topEndVoxel = new Vector3Int((topSpace.Bounds.min.x + topSpace.Bounds.max.x) / 2, topSpace.Bounds.min.y, topSpace.Bounds.min.z - Mathf.CeilToInt(corridorWidth / 2f));
            this.corridorGrid[topEndVoxel.x, topEndVoxel.y, topEndVoxel.z] = true;

            topSpace.calculateDoorPlacement(new BoundsInt(
                 topEndVoxel + Vector3Int.left,
                 new Vector3Int(1, 1, 1)

             ), SplitPosition.Top, wallThickness);

            bottomSpace.calculateDoorPlacement(new BoundsInt(
                  bottomStartVoxel + Vector3Int.left,
                  new Vector3Int(1, 1, 1)

                ), SplitPosition.Bottom, wallThickness);

            this.addDoorPlacement(topSpace.DoorPlacements.Last());
            this.addDoorPlacement(bottomSpace.DoorPlacements.Last());


            // build corridor between found top and bottomSpace
            if (buildCorridorLateral(bottomStartVoxel, topEndVoxel))
            {
                // update available Grid
                updateAvailableGrid();

                // generate corridorBounds from corridor Grid 
                generateCorridorFloorBounds(bottomSpace.Bounds.y);
                calculateWallsFromCorridorTest(SplitPosition.Top);

                // --- Add Neighbours to the Connection List of Respective Nodes --- 
                topSpace.ConnectionsList.Add(bottomSpace);
                bottomSpace.ConnectionsList.Add(topSpace);

                // --- calculate the bounds of the door to be used in mesh generation ---
                this.CorridorType = CorridorType.Vertical;
            }
        }
      
        #endregion

        #region Up-Down Generation

        #region Up-Down Helpers
        private List<Node> ReturnDownMostSpaces(List<Node> UpSpaces)
        {

            var sortedUpSpaces = UpSpaces.OrderBy(space => space.Bounds.min.y).ToList(); // get bottom most children of top space

            if (sortedUpSpaces.Count == 1)
            {
                return sortedUpSpaces; // if only one topSpace available select it (usually will just join the two deepest children)
            }
            else // select one of the bottom most topSpace if there are multiple
            {
                int minY = sortedUpSpaces[0].Bounds.min.y; // get the coordinates of the bottom most bound
                sortedUpSpaces = sortedUpSpaces.Where(space => Math.Abs(minY - space.Bounds.min.y) < minRoomDim.y).ToList(); // deviation less than min room size to not go through rooms
                return sortedUpSpaces;
            }

        }
        private List<Node> ReturnUpMostSpaces(List<Node> DownSpaces)
        {

            var sortedDownSpaces = DownSpaces.OrderByDescending(space => space.Bounds.max.y).ToList(); // get Top most children of Bottom space

            if (sortedDownSpaces.Count == 1)
            {
                return sortedDownSpaces; // if only one bottomSpace available select it (usually will just join the two deepest children)
            }
            else // select one of the top most BottomSpaces if there are multiple
            {
                int maxY = sortedDownSpaces[0].Bounds.max.y; // get the coordinates of the top most bound
                sortedDownSpaces = sortedDownSpaces.Where(space => Math.Abs(maxY - space.Bounds.max.y) < minRoomDim.y).ToList(); // deviation less than min room size to not go through rooms
                return sortedDownSpaces;
            }

        }
        private List<Node> ReturnPossibleNeighborsDownSpace(Node upSpace, List<Node> downSpaceNeighborCandidates)
        {

            return downSpaceNeighborCandidates.OrderByDescending(downSpace => //downSpace.Bounds.max.y
                                                                              -Vector3.Distance(upSpace.Bounds.center, downSpace.Bounds.center)).ToList(); 
            
            
            
            // order by descending (max) y

            //.Where(downSpace =>
            //   // GetCorridorPositionUpDownXZ(upSpace, downSpace).x != -1 &&
            //    // GetCorridorPositionUpDownXZ(upSpace, downSpace).z != -1




            ////&& Mathf.Abs(downSpace.Bounds.max.y - upSpace.Bounds.min.y) < minRoomDim.z // FIX ME: Rewrite minRoomDim with correct yz switch
            //// otherwise there is a room inbetween // stops from generating updown corridors through rooms
            //// && (upSpace.FloorIndex-1 == downSpace.FloorIndex)
            //)


        }
        private (Node upSpace, Node downSpace) FindNeighborsUpDown(List<Node> sortedUpSpace, List<Node> sortedDownSpace)
        {
            // intialize output nodes
            Node downSpace = null;
            Node upSpace = null;

            upSpace = sortedUpSpace[Random.Range(0, sortedUpSpace.Count())]; // pick a left space from candidates
            var neighborsInDownSpaceList = ReturnPossibleNeighborsDownSpace(upSpace, sortedDownSpace); // get possible neighbors in rightSpace

            if (neighborsInDownSpaceList.Count() > 0)
            {
                downSpace = neighborsInDownSpaceList[0]; // get closest one
                //downSpace = neighborsInDownSpaceList[Random.Range(0, neighborsInDownSpaceList.Count)]; // if neighbors exist choose one at random 
            }
            else
            {
                // --- Added Check if no neighbors are found to reselect leftSpace

                sortedUpSpace.Remove(upSpace);

                foreach (var newUpSpace in sortedUpSpace)
                {
                    Debug.Log($"<color=#FF0000> NEW UP SPACE </color>");

                    neighborsInDownSpaceList = ReturnPossibleNeighborsDownSpace(newUpSpace, sortedDownSpace); // select new LeftSpace and check if it has any neighbors

                    if (neighborsInDownSpaceList.Count() > 0) // if neighbors are found set leftSpace to newly selected leftSpace and select rightSpace randomly
                    {
                        upSpace = newUpSpace;
                        downSpace = neighborsInDownSpaceList[Random.Range(0, neighborsInDownSpaceList.Count())];
                        break;
                    }
                }
            }

            return (upSpace, downSpace);
        }
        public void GenerateCorridorBoundsUpDown(Node upSpace, Node downSpace)
        {
            Vector3Int corridorXZ = upSpace.Bounds.position; //GetCorridorPositionUpDownXZ(upSpace, downSpace);

            #region Debug Corridor
            if (corridorXZ.x == -1 || corridorXZ.z == -1)
            {
                // Incase no Neighbours are Found 
                // TODO: Move this to Unit Testing
                Debug.Log("Wrong Size");
            }

            if (upSpace == null || downSpace == null)
            {
                Debug.Log("Spaces not Found");
            }
            #endregion

            // --- Generate Bounds for the Corridors --- 
            var sizeY = upSpace.Bounds.min.y - downSpace.Bounds.max.y;
            var pos = new Vector3Int(corridorXZ.x, downSpace.Bounds.max.y, corridorXZ.z);

            Bounds = new BoundsInt(
                pos,
                new Vector3Int(6, (upSpace.Bounds.min.y - downSpace.Bounds.max.y), 2) // this.corridorWidth
            );
        }
        #endregion
        private void GenerateCorridorUpDown(Node upNode, Node downNode)
        {
            // --- Initialize Leaves Arrays and Spaces to Connect ---

            Node upSpace = null; // left space to connect
            List<Node> upSpaceLeaves = GraphHelper.GetLeaves(upNode); // get all the leaves in the left space

            Node downSpace = null; // right space to connect
            List<Node> downSpaceLeaves = GraphHelper.GetLeaves(downNode); // get all the leaves in the right space

            // --- select (right most) LeftSpace and (left most) RightSpace to Connect ---

            var sortedUpSpaces = ReturnDownMostSpaces(upSpaceLeaves);
            var sortedDownSpaces = ReturnUpMostSpaces(downSpaceLeaves);




            if (sortedUpSpaces.Count() > 0)
            {
                for (int i = sortedUpSpaces.Count() - 1; i >= 0; i--)
                {
                    foreach (var holePlacement in sortedUpSpaces[i].HolePlacements)
                    {
                        if (holePlacement.PositionType == SplitPosition.Down)
                        {
                            Debug.Log("Removed upSpace");
                            sortedUpSpaces.RemoveAt(i); // bugs out 
                            break;
                        }
                    }
                }
            }


            if (sortedDownSpaces.Count() > 0)
            {
                for (int i = sortedDownSpaces.Count() - 1; i >= 0; i--)
                {
                    foreach (var holePlacement in sortedDownSpaces[i].HolePlacements)
                    {
                        if (holePlacement.PositionType == SplitPosition.Up)
                        {
                            Debug.Log("Removed downSpace");
                            sortedDownSpaces.RemoveAt(i);
                            break;
                        }
                    }
                }
            }


            if (sortedUpSpaces.Count() < 1 || sortedDownSpaces.Count() < 1)
            {
                this.CorridorType = CorridorType.None;
                return;
            }

            // --- Find Neighbor pair in LeftSpaces and RightSpaces ---

            (upSpace, downSpace) = FindNeighborsUpDown(sortedUpSpaces, sortedDownSpaces);

            // --- Generate Corridor Between LeftSpace and RightSpace --- 

            Debug.Log($"Upspace Value {upSpace is null} children in upNode {upNode.ChildrenNodeList.Count}");
            Debug.Log($"DownSpace Value {downSpace is null} children in downNode {downNode.ChildrenNodeList.Count}");

            if (upSpace is null || downSpace is null)
            {
                this.CorridorType = CorridorType.None;
                throw new Exception("Failed up and Down Connection");

            }



            GenerateCorridorBoundsUpDown(upSpace, downSpace);

            // --- Add Neighbours to the Connection List of Respective Nodes --- 

            upSpace.ConnectionsList.Add(downSpace);
            downSpace.ConnectionsList.Add(upSpace);

            // -- Add upSpace and downSpace to corridor connection list for ladder generation ---#
            ConnectionsList.Add(upSpace);
            ConnectionsList.Add(downSpace);

            // --- calculate the bounds of the door to be used in mesh generation ---


            // calculate the bounds of the door for the left space and the right spaces
            upSpace.calculateHolePlacement(this.Bounds, SplitPosition.Up, wallThickness);
            downSpace.calculateHolePlacement(this.Bounds, SplitPosition.Down, wallThickness);

            // add the doorbounds found for the left and right space to the corridor
            this.addHolePlacement(upSpace.HolePlacements.Last());
            this.addHolePlacement(downSpace.HolePlacements.Last());


            this.Bounds = new BoundsInt(
                this.Bounds.position + new Vector3Int(this.wallThickness, 0, this.wallThickness),
                this.Bounds.size - new Vector3Int(this.wallThickness, 0, this.wallThickness) * 2
             ); // make walls flush 


            this.CorridorType = CorridorType.Perpendicular;

        }
        private bool unboundedContains(BoundsInt bounds, Vector3Int position)
        {
            return position.x >= bounds.xMin && position.z >= bounds.zMin && position.x <= bounds.xMax && position.z <= bounds.zMax; // without y component
        }
        private Vector3Int GetCorridorPositionUpDownXZ(Node upSpace, Node downSpace)
        {

            // FIX ME: CASE WHERE NO POINTS ARE CONTAINED

            // Check if the intersection of the upSpace and downSpace planes has an intersection equal to corridorWidth x corridorWidth returning the XZ location

            var upSpacePlane = new BoundsInt(
                new Vector3Int(upSpace.Bounds.position.x, 0, upSpace.Bounds.position.z),
                new Vector3Int(upSpace.Bounds.size.x, 0, upSpace.Bounds.size.z)
            );
            var downSpacePlane = new BoundsInt(
                 new Vector3Int(downSpace.Bounds.position.x, 0, downSpace.Bounds.position.z),
                 new Vector3Int(downSpace.Bounds.size.x, 0, downSpace.Bounds.size.z)
            );

            var intesectionPlane = new BoundsInt(
                upSpace.Bounds.position,
                upSpace.Bounds.size
            );




            var nIntersections = 0;

            // Bottom Points
            if (unboundedContains(upSpacePlane, downSpacePlane.position)) // contains bottomLeftCorner
            {
                // set new bottomLeftCorner
                upSpacePlane = new BoundsInt(
                    downSpacePlane.position,
                    upSpacePlane.position + upSpacePlane.size - downSpacePlane.position
                );

                nIntersections++;
            }

            if (unboundedContains(upSpacePlane, downSpacePlane.position + new Vector3Int(downSpacePlane.size.x, 0, 0))) // contains bottomRightCorner
            {
                var newPos = new Vector3Int(upSpacePlane.x, 0, downSpacePlane.z);
                // set new bottomRightCorner
                upSpacePlane = new BoundsInt(
                    newPos,
                    new Vector3Int(downSpacePlane.position.x + downSpacePlane.size.x, 0, upSpacePlane.zMax) - newPos
                );

                nIntersections++;

            }

            // Top Right
            if (unboundedContains(upSpacePlane, downSpacePlane.position + new Vector3Int(downSpacePlane.size.x, 0, downSpacePlane.size.z))) // contains topRightCorner
            {

                // set new topRightCorner
                upSpacePlane = new BoundsInt(
                   upSpacePlane.position,
                   (downSpacePlane.max - upSpacePlane.position)
                );
                nIntersections++;
            }

            if (unboundedContains(upSpacePlane, downSpacePlane.position + new Vector3Int(0, 0, downSpacePlane.size.z))) // contains topLeftCorner
            {
                var newPos = new Vector3Int(downSpacePlane.x, 0, upSpacePlane.z);

                // set new topLeftCorner
                upSpacePlane = new BoundsInt(
                    newPos,
                    new Vector3Int(upSpacePlane.max.x, 0, downSpacePlane.max.z) - newPos // upspacePlaneMax.x
                );
                nIntersections++;

            }

            Debug.Log($"<color=#00FF00>number of intersections {nIntersections}</color>");


            //if (nIntersections == 0)
            //{
            //    if (!(upSpacePlane.size.x == downSpacePlane.size.x && upSpacePlane.size.z == downSpacePlane.size.z))
            //    {
            //        Debug.Log($"<color=#00FF14> FAILED TO FIND </color>");
            //        upSpacePlane = MeshHelper.planeIntersectBounds(downSpacePlane, upSpacePlane);
            //    }
            //}
            // return the resulting position if its large enough to have a perpendicular corridor geenerated


            bool downSpaceAndupSpaceSame = (upSpacePlane.size.x == downSpacePlane.size.x && upSpacePlane.size.z == downSpacePlane.size.z) &&
                (downSpacePlane.position.x == upSpacePlane.position.x && downSpacePlane.position.z == upSpacePlane.position.z);

            bool spaceSufficientSize = upSpacePlane.size.x > corridorWidth && upSpacePlane.size.z > corridorWidth;

            if (spaceSufficientSize && nIntersections > 0 || (spaceSufficientSize && downSpaceAndupSpaceSame) ||

                (spaceSufficientSize
                && unboundedContains(downSpacePlane, upSpacePlane.position)
                && unboundedContains(downSpacePlane, upSpacePlane.position + new Vector3Int(upSpacePlane.size.x, 0, 0))
                && unboundedContains(downSpacePlane, upSpacePlane.position + new Vector3Int(0, 0, upSpacePlane.size.z))
                && unboundedContains(downSpacePlane, upSpacePlane.position + new Vector3Int(upSpacePlane.size.x, 0, upSpacePlane.size.z)))

                )
            {

                // return random point inside the possible upSpace plane 

                // FIX ME: DOUBLE CHECK

                Vector3Int holePosition = new Vector3Int(
                        Random.Range(upSpacePlane.min.x, upSpacePlane.max.x - corridorWidth),
                        0,
                        Random.Range(upSpacePlane.min.z, upSpacePlane.max.z - corridorWidth)
                    );

                Debug.Log($"holePos Vector {holePosition}");
                Debug.Log($@"holePos is contained {holePosition}:
                        contained in upSpace {unboundedContains(upSpacePlane, holePosition)}
                        contained in downSpace {unboundedContains(downSpacePlane, holePosition)}
                    ");


                if (!(unboundedContains(upSpacePlane, holePosition) && unboundedContains(downSpacePlane, holePosition)))
                {
                    Debug.Log($@"
                        FAILED HOLE:
                        upSpacePlane {upSpacePlane},
                        downSpacePlane {downSpacePlane},
                        Old Upspace Value {intesectionPlane},
                        holePosition {holePosition}
                    ");
                }

                return holePosition;
            }
            else
            {
                if (!(upSpacePlane.size.x == downSpacePlane.size.x && upSpacePlane.size.z == downSpacePlane.size.z))
                {
                    Debug.Log($@"FAILED TO FIND CONNECTION UP DOWN!!
                    {upSpacePlane}
                    {downSpacePlane}
                    Old Upspace Value {intesectionPlane}
                    N Intesections {nIntersections}
                    Corridor Width {corridorWidth}
                -----------------------------------
                ");
                }

                return Vector3Int.one * -1;
            }


        }
        #endregion 
    
}
}