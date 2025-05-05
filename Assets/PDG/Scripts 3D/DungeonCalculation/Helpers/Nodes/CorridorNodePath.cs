using Codice.CM.Common.Tree;
using Codice.CM.SEIDInfo;
using System;
using System.CodeDom.Compiler;
using System.Collections;
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
        private System.Random _randomGenerator;

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

        public CorridorNodePath(Node node1, Node node2, int corridorWidth, int wallThickness, Vector3Int minRoomDim, int corridorHeight, bool[,,] availableVoxelGrid, System.Random randomGenerator) : base(null) // null since it doesnt have any parents
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

            this._randomGenerator = randomGenerator;

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
        private void calculateWallsFromCorridorGrid()
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
                        BoundsInt wallBounds = new BoundsInt(new Vector3Int(x + minPos.x, minPos.y, z + minPos.z), 
                                                             new Vector3Int(width, corridorHeight, height)
                                                             );
                        Debug.Log("THe Wall Segment BOunds" + wallBounds.ToString());
                        CorridorWallBoundsList.Add(wallBounds);

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
                    GenerateCorridorLateral(this.node1, this.node2);
                    break;
                case SplitPosition.Bottom:
                    GenerateCorridorLateral(this.node2, this.node1);
                    break;
                case SplitPosition.Right:
                    GenerateCorridorLateral(this.node1, this.node2); // sorted start and end node
                    break;
                case SplitPosition.Left:
                    GenerateCorridorLateral(this.node2, this.node1);
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

        #region Lateral Corridor Generation

        #region Lateral Helper Methods
        /// <summary>
        /// Returns closest aligned nodes to the bsp split position
        /// </summary>
        /// <param name="spaces"></param>
        /// <param name="alignment"></param>
        /// <returns>closest aligned nodes to split</Nove></returns>
        private List<Node> ReturnAlignedSpaces(List<Node> spaces, SplitPosition alignment, bool isRestrictedY = true)
        {

            bool isStartNode = alignment.toV3I().x < 0 || alignment.toV3I().y < 0 || alignment.toV3I().z < 0;

            Func<Node, float> posCompareNode = space => {
                return Mathf.Abs(Vector3.Dot(alignment.toV3I(),(isStartNode ? space.Bounds.max : space.Bounds.min)));
            };

            Func<Vector3Int, float> posCompareV3I = v3i => {
                return Mathf.Abs(Vector3.Dot(alignment.toV3I(), v3i));
            };

            var sortedSpaces = spaces.OrderByDescending(space => (isStartNode ? 1 : -1) * posCompareNode(space)).ToList(); // get right most children of left space

            return sortedSpaces;

            //if (sortedSpaces.Count == 1)
            //{
            //    return sortedSpaces; // if only one leftSpace available select it (usually will just join the two deepest children)
            //}
            //else // select one of the rights most LeftSpaces if there are multiple
            //{
            //    int pos = Mathf.Abs((int)Vector3.Dot(alignment.toV3I(), (isStartNode ? sortedSpaces[0].Bounds.max : sortedSpaces[0].Bounds.min)));

            //    //sortedSpaces[0].Bounds.max.x; // get the coordinates of the right most bound
            //    sortedSpaces = sortedSpaces.Where(space => Math.Abs(pos - posCompareNode(space)) < 2*posCompareV3I(this.minRoomDim)).ToList(); // deviation less than 2x min room size to not go through rooms
            //    return sortedSpaces;
            //}

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

        /// <summary>
        /// Removes spaces from possible rooms list that have already been connected (useful for cyclic connections)
        /// </summary>
        /// <param name="Spaces"></param>
        /// <param name="doorPosition"></param>
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

        /// <summary>
        /// Gets Closest neighboring Rooms to Connect
        /// </summary>
        /// <param name="startNodes"></param>
        /// <param name="endNodes"></param>
        /// <returns></returns>
        private (Node startNode, Node endNode) getLateralNeighbors(List<Node> startNodes, List<Node> endNodes)
        {
            Node startNode = startNodes[0];

            Func<Node, Node, float> getNodeDistance = (node1, node2) =>
                                                      Vector3.Distance(node1.Bounds.center,
                                                                       node2.Bounds.center);

            Func<Node, Node, bool> isSameY = (node1, node2) => node1.Bounds.y == node2.Bounds.y;

            // Return enumerable to store rightSpaceNeighbors to be lazy evaluated
            IEnumerable<Node> endNodeNeighbors = endNodes.OrderBy(endNode => getNodeDistance(startNode, endNode))
                                                         .Where(endNode => isSameY(startNode, endNode));

            while (endNodeNeighbors.Count() == 0 && startNodes.Count() > 0)
            {

                startNode = startNodes[this._randomGenerator.Next(0, startNodes.Count())]; //[Random.Range(0, startNodes.Count())];
                startNodes.Remove(startNode);
            }

            if (startNodes.Count() == 0 && endNodeNeighbors.Count() == 0)
            {
                return (null, null);
            }

            Node endNode = endNodeNeighbors.First();
            return (startNode, endNode);

        }

        /// <summary>
        /// Generates start and end voxel positions for pathfinding
        /// </summary>
        /// <param name="startSpace"></param>
        /// <param name="endSpace"></param>
        /// <param name="startSplit"></param>
        /// <param name="endSplit"></param>
        /// <returns> Start and End Voxel Positions </returns>
        private (Vector3Int startVoxel, Vector3Int endVoxel) getTargetVoxels(Node startSpace, Node endSpace, SplitPosition startSplit, SplitPosition endSplit)
        {
            // figure out relative placement
            Vector3Int connectionAxis = new Vector3Int(Mathf.Abs(startSplit.toV3I().x), Mathf.Abs(startSplit.toV3I().y), Mathf.Abs(startSplit.toV3I().z));
            Vector3Int perpendicularAxis = new Vector3Int(1, 0, 1) - connectionAxis;


            // Calculate Bounds for Starting Voxels (Positions of the Doors)
            Vector3Int startVoxel = Vector3Int.Scale(startSpace.Bounds.max, connectionAxis)
                                    + Vector3Int.Scale(Vector3Int.CeilToInt(startSpace.Bounds.center), perpendicularAxis)
                                    + Vector3Int.up * startSpace.Bounds.min.y;
            Vector3Int endVoxel = Vector3Int.Scale(endSpace.Bounds.min - Mathf.CeilToInt(corridorWidth / 2f) * Vector3Int.one, connectionAxis)
                                    + Vector3Int.Scale(Vector3Int.CeilToInt(endSpace.Bounds.center), perpendicularAxis)
                                    + Vector3Int.up * endSpace.Bounds.min.y;

            // update grid

            // remove points around point up to corridor height
            foreach(var voxelPos in new Vector3Int[] { startVoxel+connectionAxis, endVoxel }) { 
            for (int height = 0; height < corridorHeight; height++)
            {

                // remove closest position from available positions
                this.corridorGrid[voxelPos.x, voxelPos.y + height, voxelPos.z] = true;

                foreach (Vector3Int dir in possibleDir)
                {
                    Vector3Int resultingPos = voxelPos + dir;
                    this.corridorGrid[resultingPos.x, resultingPos.y + height, resultingPos.z] = true;
                }
            }
            }
            //this.corridorGrid[startVoxel.x, startVoxel.y, startVoxel.z] = true;
            //this.corridorGrid[endVoxel.x, endVoxel.y, endVoxel.z] = true;

            return (startVoxel, endVoxel);
        }
        
        /// <summary>
        /// If path is successsfull adds doorplacements to room and corridor to make sure openings are generated
        /// </summary>
        /// <param name="startVoxel"></param>
        /// <param name="endVoxel"></param>
        /// <param name="startSpace"></param>
        /// <param name="endSpace"></param>
        /// <param name="startSplit"></param>
        /// <param name="endSplit"></param>
        private void addDoorPlacements(Vector3Int startVoxel, Vector3Int endVoxel, Node startSpace, Node endSpace, SplitPosition startSplit, SplitPosition endSplit)
        {
            // figure out relative placement
            Vector3Int connectionAxis = new Vector3Int(Mathf.Abs(startSplit.toV3I().x), Mathf.Abs(startSplit.toV3I().y), Mathf.Abs(startSplit.toV3I().z));
            Vector3Int perpendicularAxis = new Vector3Int(1, 0, 1) - connectionAxis;

            // Start and Endspace the Door Placements
            startSpace.calculateDoorPlacement(new BoundsInt(
                    startVoxel - perpendicularAxis,
                    new Vector3Int(1, 1, 1)
                ), startSplit, this.wallThickness);

            endSpace.calculateDoorPlacement(new BoundsInt(
                  endVoxel - perpendicularAxis,
                  new Vector3Int(1, 1, 1)
                ), endSplit, this.wallThickness); // error due to split position

            // add the door placemetns to the node
            this.addDoorPlacement(startSpace.DoorPlacements.Last());
            this.addDoorPlacement(endSpace.DoorPlacements.Last());

        }
        #endregion

        /// <summary>
        /// Generate a corridor between startNode (maxPos) and endNode (minPos)
        /// </summary>
        /// <param name="endNode"> endNode </param>
        /// <param name="startNode"> startNode </param>
        private void GenerateCorridorLateral (Node endNode, Node startNode)
        {
            // --- Initialize Leaves Arrays and Spaces to Connect ---

            Node startSpace = null; // start space to connect
            List<Node> startNodeLeaves = GraphHelper.GetLeaves(startNode); // get all the leaves in the start space

            Node endSpace = null; // right end to connect
            List<Node> endNodeLeaves = GraphHelper.GetLeaves(endNode); // get all the leaves in the end space

            // --- select (max most) startSpace and (min most) endSpace to Connect ---

            var sortedStartSpaces = ReturnAlignedSpaces(startNodeLeaves, startNode.SplitPosition);
            var sortedEndSpaces = ReturnAlignedSpaces(endNodeLeaves, endNode.SplitPosition);

            // --- Check already connected spaces 
            removeConnectedSpaces(sortedStartSpaces, endNode.SplitPosition);
            removeConnectedSpaces(sortedEndSpaces, startNode.SplitPosition);

            if (sortedStartSpaces.Count() < 1 || sortedEndSpaces.Count() < 1)
            {
                Debug.LogWarning("No Possible Rooms to Connect found");
                this.CorridorType = CorridorType.None;
                return;
            }

            // --- Find Neighbor pair in LeftSpaces and RightSpaces ---

            (startSpace, endSpace) = getLateralNeighbors(sortedStartSpaces, sortedEndSpaces);

            if (startSpace == null && endSpace == null)
            {
                Debug.LogWarning("Null Lateral Returned");
                this.CorridorType = CorridorType.None;
                return;
            }

            // --- Calculate Door Positions ---- 

            (Vector3Int startVoxel, Vector3Int endVoxel) = getTargetVoxels(startSpace, endSpace, startNode.SplitPosition, endNode.SplitPosition);

            // --- Build Corridor Between found Spaces ---

            if (buildCorridorLateral(startVoxel, endVoxel)) // generate door placements only after corridor has been found ... 
            {
                // update available Grid
                updateAvailableGrid();

                // generate corridorBounds from corridor Grid 
                generateCorridorFloorBounds(startSpace.Bounds.y); // doesnt matter which one
                calculateWallsFromCorridorGrid();

                // --- Add Neighbours to the Connection List of Respective Nodes --- 
                startSpace.ConnectionsList.Add(endSpace);
                endSpace.ConnectionsList.Add(startSpace);

                // --- Calculcate Door Placements ---
                addDoorPlacements(startVoxel, endVoxel, startSpace, endSpace, startNode.SplitPosition, endNode.SplitPosition);

                // --- select type of corridor ---
                this.CorridorType = startNode.SplitPosition.toCorridorType();
            }

        }

        /// <summary>
        /// Builds corridor between start and end voxels, adding result into corridor grid
        /// </summary>
        /// <param name="startVoxel"></param>
        /// <param name="endVoxel"></param>
        /// <returns>status: has building path succeeded</returns>
        /// <exception cref="Exception">path failure</exception>
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

                if (possiblePositions.Count() < 1) return false; // throw new Exception("Path not possible"); // if no further positions available path fails

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
                for (int height = 0; height < corridorHeight; height++)
                {

                    // remove closest position from available positions
                    this.corridorGrid[closestPos.x, closestPos.y + height, closestPos.z] = true;

                    foreach (Vector3Int dir in possibleDir)
                    {
                        Vector3Int resultingPos = closestPos + dir;
                        this.corridorGrid[resultingPos.x, resultingPos.y + height, resultingPos.z] = true;
                    }
                }

                // change starting position to new position
                startPos = closestPos;
                i++; // increment iterator to prevent loops

            }

            // ---- checks if generation has failed or not ---- 

            if (startPos != endVoxel)
            {

                this.CorridorType = CorridorType.None;
                Debug.LogWarning("Additional Path not completed");
                return false;
            }

            return true;

        }
        #endregion

        #region Perpendicular Corridor Generation 

        #region perpendicular corridor helpers
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

            upSpace = sortedUpSpace[this._randomGenerator.Next(0, sortedUpSpace.Count())]; // pick a left space from candidates // Random.Range(0, sortedUpSpace.Count())
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
                        downSpace = neighborsInDownSpaceList[this._randomGenerator.Next(0, neighborsInDownSpaceList.Count())]; //Random.Range(0, neighborsInDownSpaceList.Count())];
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
            generateStairWaypoints(downSpace,upSpace);

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

            // update available Grid
            updateAvailableGrid();

            this.CorridorType = CorridorType.Perpendicular;

        }

        private void generateStairWaypoints(Node downNode, Node upNode)
        {

            List<Node> connectedRoomsOrderedByY = new List<Node> { downNode, upNode };
            Vector3 planeSize = new Vector3(2, 0, 2); // stair segment size

            // define start and target positions 
            var startPos = connectedRoomsOrderedByY[0].Bounds.position + planeSize * 0.5f + new Vector3(1, 0, 1) + new Vector3(1f,0,1f); // added slight 1,0,1 offset

            var endPos = new Vector3(connectedRoomsOrderedByY[1].Bounds.position.x + planeSize.x,
                                     connectedRoomsOrderedByY[1].Bounds.position.y,
                                     connectedRoomsOrderedByY[1].Bounds.position.z + planeSize.x);

            var preEndPos = new Vector3(connectedRoomsOrderedByY[1].Bounds.position.x + planeSize.x + 6,
                                     connectedRoomsOrderedByY[1].Bounds.position.y - 1,
                                     connectedRoomsOrderedByY[1].Bounds.position.z + planeSize.x);

            // update taken up poisitions by the corridor

            //updateCorridorGrid(endPos-Vector3.up, new Vector3(4,0,0), true);
            //updateCorridorGrid(endPos - Vector3.up+ new Vector3(4,-1,0), new Vector3(2, -1, 0), true);

            // --- single spiral around room --- (can do different types of rooms)

            // check with correct angle... 
            float roomSpiralIncrement = 1f; //Mathf.Tan(2 * Mathf.PI / 24) * (minRoomDim.z - planeSize.z); // also make it be a multiple of the height


            var isWidthSmaller = connectedRoomsOrderedByY[0].Bounds.size.z > connectedRoomsOrderedByY[0].Bounds.size.x; //

            List<Vector3> planeOffsets = new List<Vector3>
                {
                    new Vector3(0f, roomSpiralIncrement, 8f-planeSize.z), // connectedRoomsOrderedByY[0].Bounds.size
                    new Vector3(planeSize.x, 0, 0f), // connectedRoomsOrderedByY[0].Bounds.size.x
                    new Vector3(0f, roomSpiralIncrement, -8f+planeSize.z),
                    new Vector3(-planeSize.x, 0, 0f), // -connectedRoomsOrderedByY[0].Bounds.size.x+
                };

            var endOffset = endPos - startPos - planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);

            var preEndPosOffset = preEndPos - startPos - planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);
            var curPosOffset = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);

            // get to correct height

            var maxIter = 100;
            int i = 0;

            #region get to correct y level
            while (curPosOffset.y + startPos.y < connectedRoomsOrderedByY[0].Bounds.max.y && i < maxIter)
            {
                planeOffsets.Add(planeOffsets[i]);
                curPosOffset = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);

                i++;
            }
            #endregion

            // get to correct position
            preEndPosOffset = preEndPos - startPos - planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);

            #region connect Paths

            //List<Vector3> possDirections = new List<Vector3>() {
            //    Vector3.forward*planeSize.z*2+Vector3.up*1f,
            //    Vector3.back*planeSize.z*2+Vector3.up*1f,
            //    Vector3.left*planeSize.x*2+Vector3.up*1f,
            //    Vector3.right*planeSize.x*2+Vector3.up*1f,
            //    Vector3.forward*planeSize.z*3+Vector3.up*1f,
            //    Vector3.back*planeSize.z*3+Vector3.up*1f,
            //    Vector3.left*planeSize.x*3+Vector3.up*1f,
            //    Vector3.right*planeSize.x*3+Vector3.up*1f,
            //    Vector3.forward*planeSize.z*(3+0.5f)+Vector3.up*1f,
            //    Vector3.back*planeSize.z*(3+0.5f)+Vector3.up*1f,
            //    Vector3.left*planeSize.x*(3+0.5f)+Vector3.up*1f,
            //    Vector3.right*planeSize.x*(3+0.5f)+Vector3.up*1f,
            //};
            //var curPos = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);



            //List<Vector3> failedDirections = new List<Vector3>();

            //while (i < maxIter && (((curPos.y + startPos.y) < (preEndPos.y))
            //    || (Mathf.Abs(curPos.x+startPos.x - preEndPos.x) < 2)
            //    || (Mathf.Abs(curPos.z + startPos.z - preEndPos.z) < 2))

            //    )

            //{
            //    // check if angle is ok
            //    Vector3 closestOffset = Vector3.zero;
            //    foreach (var possDirection in possDirections)
            //    {

            //        curPos = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2) + possDirection;

            //        // check if  there is enough space between positions 
            //        var curPosOverlapCheck = Vector3.zero;
            //        bool isOverlapingHeight = false;

            //        foreach (var offset in planeOffsets) // looping over backwards might be better
            //        {
            //            curPosOverlapCheck += offset;

            //            //Debug.Log($"dotP: {Vector3.Dot((curPosOverlapCheck - curPos), Vector3.up)}");

            //            if (Vector3.Dot((curPos - curPosOverlapCheck), Vector3.up) < 2
            //                && (Mathf.Abs((curPos - curPosOverlapCheck).x) <= planeSize.x) && (Mathf.Abs((curPos - curPosOverlapCheck).z) <= planeSize.z)
            //                )
            //            {

            //                Debug.Log($"Overlap: curPos {curPos}, overLapCheckPos {curPosOverlapCheck}, possDir: {possDirection}, dotP {Vector3.Dot((curPos - curPosOverlapCheck), Vector3.up)}, secondaryCheck {Vector3.Dot((curPos - curPosOverlapCheck), new Vector3(1, 0, 1))}");
            //                isOverlapingHeight = true;
            //                //break;

            //            }
            //        }

            //        if (
            //            Vector3.Dot((curPos - endPos), Vector3.up) < 2f
            //            && ((curPos - endPos + startPos).x <= planeSize.x) && ((curPos - endPos + startPos).z <= planeSize.z)
            //            )
            //        {

            //            isOverlapingHeight = true;
            //        }



            //        curPos = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2) + possDirection;



            //        Debug.Log($"curPos eq: {curPos.z + startPos.z == (preEndPos.z)}, curPos.z: {curPos.z + startPos.z}, preEndPos.z: {preEndPos.z}");

            //        bool isWithinBounds = curPos.x + startPos.x <= (Mathf.Max(connectedRoomsOrderedByY[1].Bounds.max.x, connectedRoomsOrderedByY[0].Bounds.max.x)) //+ GameObject.Find("DungeonGen").transform.position.x)
            //                        && curPos.x + startPos.x >= (Mathf.Min(connectedRoomsOrderedByY[1].Bounds.min.x, connectedRoomsOrderedByY[0].Bounds.min.x)+2) //+ GameObject.Find("DungeonGen").transform.position.x)
            //                        && curPos.z + startPos.z <= (Mathf.Max(connectedRoomsOrderedByY[1].Bounds.max.z, connectedRoomsOrderedByY[0].Bounds.max.z)) //+ GameObject.Find("DungeonGen").transform.position.z)
            //                        && curPos.z + startPos.z >= (Mathf.Min(connectedRoomsOrderedByY[1].Bounds.min.z, connectedRoomsOrderedByY[0].Bounds.min.z)+2); //+ GameObject.Find("DungeonGen").transform.position.z) ;

            //        Debug.Log($"New Pos:--- possDir{possDirection}, planeOffsetLast {planeOffsets.Last()}, projected: {Vector3.ProjectOnPlane(planeOffsets.Last(), Vector3.up)}, dotProduct {Vector3.Dot(Vector3.Normalize(Vector3.ProjectOnPlane(possDirection, Vector3.up)), Vector3.Normalize(Vector3.ProjectOnPlane(planeOffsets.Last(), Vector3.up)))}, distance {MeshHelper.ManhattanDistance3(planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2) + possDirection + startPos, preEndPos)}" +
            //          $"posinBounds:  {isWithinBounds}, isOverlapingHeight {isOverlapingHeight}");


            //        bool isDirectionBackwards = Vector3.Dot(
            //                        Vector3.Normalize(Vector3.ProjectOnPlane(possDirection, Vector3.up)),
            //                        Vector3.Normalize(Vector3.ProjectOnPlane(planeOffsets.Last(), Vector3.up))
            //                        ) == -1;
            //        Vector3 curPs = curPos + startPos;


            //        Debug.Log($"New Pos:--- {curPos}, Start Pos {startPos}");

            //        bool isSpaceFree = isWithinBounds && !this.corridorGrid[Mathf.FloorToInt(curPs.x) - 1, Mathf.FloorToInt(curPs.y), Mathf.FloorToInt(curPs.z) - 1] &&
            //        !this.corridorGrid[Mathf.FloorToInt(curPs.x) - 2, Mathf.FloorToInt(curPs.y), Mathf.FloorToInt(curPs.z) - 2] &&
            //        !this.corridorGrid[Mathf.FloorToInt(curPs.x) - 2, Mathf.FloorToInt(curPs.y), Mathf.FloorToInt(curPs.z) - 1] &&
            //        !this.corridorGrid[Mathf.FloorToInt(curPs.x) - 1, Mathf.FloorToInt(curPs.y), Mathf.FloorToInt(curPs.z) - 2];


            //        if (!isDirectionBackwards && isSpaceFree && !failedDirections.Contains(possDirection))
            //        {

            //            // find closest position to preEndPos that is within the bounds of the top room
            //            var distanceFromNewPos = MeshHelper.ManhattanDistance3(planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2) + possDirection + startPos, preEndPos);
            //            var distanceFromClosestPos = MeshHelper.ManhattanDistance3(planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2) + closestOffset + startPos, preEndPos); ;


            //            if (distanceFromNewPos > distanceFromClosestPos || closestOffset == Vector3.zero)
            //            {
            //                Debug.Log($"New Pos: {closestOffset}, {distanceFromClosestPos}, Best Pos {possDirection}, distance {distanceFromNewPos}, Prev Offset {planeOffsets.Last()}");
            //                closestOffset = possDirection;
            //            }
            //        }
            //        else
            //        {
            //            Debug.Log($"Moving on backwards");
            //        }

            //    }

            //    //Debug.Log($"New Pos: ---- Closest Pos Stairs {closestOffset}");
            //    //planeOffsets.Add(closestOffset);
            //    //preEndPosOffset = preEndPos - startPos - planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);
            //    //curPos = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);

            //    //i++;

            //    //Vector3 curPosition = curPos+startPos;

            //    //this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 1, Mathf.FloorToInt(curPosition.y), Mathf.FloorToInt(curPosition.z) - 1] = true;
            //    //this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 2, Mathf.FloorToInt(curPosition.y), Mathf.FloorToInt(curPosition.z) - 2] = true;
            //    //this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 2, Mathf.FloorToInt(curPosition.y), Mathf.FloorToInt(curPosition.z) - 1] = true;
            //    //this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 1, Mathf.FloorToInt(curPosition.y), Mathf.FloorToInt(curPosition.z) - 2] = true;


            //    if (closestOffset != Vector3.zero)
            //    {
            //        Debug.Log("Not Failed Direction");
            //        planeOffsets.Add(closestOffset);
            //        failedDirections.Clear();




            //        preEndPosOffset = preEndPos - startPos - planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);
            //        curPos = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);


            //        Vector3 curPosition = curPos + startPos - planeOffsets.Last();

            //        for (int x = 0; x <= Mathf.Abs(planeOffsets.Last().x); x++)
            //        {
            //            for (int y = 0; y <= Mathf.Abs(planeOffsets.Last().y); y++)
            //            {
            //                for (int z = 0; z <= Mathf.Abs(planeOffsets.Last().z); z++)
            //                {

            //                    Debug.Log($"offsetLoop: {x}{y}{z}");

            //                    this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 1 + x * Math.Sign(planeOffsets.Last().x), Mathf.FloorToInt(curPosition.y) + y * Math.Sign(planeOffsets.Last().y), Mathf.FloorToInt(curPosition.z) + z * Math.Sign(planeOffsets.Last().z) - 1] = true;
            //                    this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 2 + x * Math.Sign(planeOffsets.Last().x), Mathf.FloorToInt(curPosition.y) + y * Math.Sign(planeOffsets.Last().y), Mathf.FloorToInt(curPosition.z) + z * Math.Sign(planeOffsets.Last().z) - 2] = true;
            //                    this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 2 + x * Math.Sign(planeOffsets.Last().x), Mathf.FloorToInt(curPosition.y) + y * Math.Sign(planeOffsets.Last().y), Mathf.FloorToInt(curPosition.z) + z * Math.Sign(planeOffsets.Last().z) - 1] = true;
            //                    this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 1 + x * Math.Sign(planeOffsets.Last().x), Mathf.FloorToInt(curPosition.y) + y * Math.Sign(planeOffsets.Last().y), Mathf.FloorToInt(curPosition.z) + z * Math.Sign(planeOffsets.Last().z) - 2] = true;

            //                }


            //            }


            //        }
            //    }
            //    else if (failedDirections.Count() == possDirections.Count()) // 
            //    {
            //        Debug.Log("Moving Failed Direction");
            //        planeOffsets.Remove(planeOffsets.Last());
            //        failedDirections.Add(planeOffsets.Last());
            //        curPos = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);
            //        Vector3 curPosition = curPos + startPos - planeOffsets.Last();

            //        for (int x = 0; x <= Mathf.Abs(planeOffsets.Last().x); x++)
            //        {
            //            for (int y = 0; y <= Mathf.Abs(planeOffsets.Last().y); y++)
            //            {
            //                for (int z = 0; z <= Mathf.Abs(planeOffsets.Last().z); z++)
            //                {

            //                    Debug.Log($"offsetLoop: {x}{y}{z}");

            //                    this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 1 + x * Math.Sign(planeOffsets.Last().x), Mathf.FloorToInt(curPosition.y) + y * Math.Sign(planeOffsets.Last().y), Mathf.FloorToInt(curPosition.z) + z * Math.Sign(planeOffsets.Last().z) - 1] = false;
            //                    this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 2 + x * Math.Sign(planeOffsets.Last().x), Mathf.FloorToInt(curPosition.y) + y * Math.Sign(planeOffsets.Last().y), Mathf.FloorToInt(curPosition.z) + z * Math.Sign(planeOffsets.Last().z) - 2] = false;
            //                    this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 2 + x * Math.Sign(planeOffsets.Last().x), Mathf.FloorToInt(curPosition.y) + y * Math.Sign(planeOffsets.Last().y), Mathf.FloorToInt(curPosition.z) + z * Math.Sign(planeOffsets.Last().z) - 1] = false;
            //                    this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 1 + x * Math.Sign(planeOffsets.Last().x), Mathf.FloorToInt(curPosition.y) + y * Math.Sign(planeOffsets.Last().y), Mathf.FloorToInt(curPosition.z) + z * Math.Sign(planeOffsets.Last().z) - 2] = false;

            //                }


            //            }


            //        }
            //        planeOffsets.Remove(planeOffsets.Last());
            //    }
            //    else
            //    {
            //        Debug.Log($"--- Failed Direction {failedDirections.Count()} possdirs {possDirections.Count()}");

            //        failedDirections.Add(planeOffsets.Last());

            //        curPos = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);
            //        Vector3 curPosition = curPos + startPos - planeOffsets.Last();

            //        for (int x = 0; x <= Mathf.Abs(planeOffsets.Last().x); x++)
            //        {
            //            for (int y = 0; y <= Mathf.Abs(planeOffsets.Last().y); y++)
            //            {
            //                for (int z = 0; z <= Mathf.Abs(planeOffsets.Last().z); z++)
            //                {

            //                    Debug.Log($"offsetLoop: {x}{y}{z}");

            //                    this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 1 + x * Math.Sign(planeOffsets.Last().x), Mathf.FloorToInt(curPosition.y) + y * Math.Sign(planeOffsets.Last().y), Mathf.FloorToInt(curPosition.z) + z * Math.Sign(planeOffsets.Last().z) - 1] = false;
            //                    this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 2 + x * Math.Sign(planeOffsets.Last().x), Mathf.FloorToInt(curPosition.y) + y * Math.Sign(planeOffsets.Last().y), Mathf.FloorToInt(curPosition.z) + z * Math.Sign(planeOffsets.Last().z) - 2] = false;
            //                    this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 2 + x * Math.Sign(planeOffsets.Last().x), Mathf.FloorToInt(curPosition.y) + y * Math.Sign(planeOffsets.Last().y), Mathf.FloorToInt(curPosition.z) + z * Math.Sign(planeOffsets.Last().z) - 1] = false;
            //                    this.corridorGrid[Mathf.FloorToInt(curPosition.x) - 1 + x * Math.Sign(planeOffsets.Last().x), Mathf.FloorToInt(curPosition.y) + y * Math.Sign(planeOffsets.Last().y), Mathf.FloorToInt(curPosition.z) + z * Math.Sign(planeOffsets.Last().z) - 2] = false;

            //                }


            //            }


            //        }


            //        planeOffsets.Remove(planeOffsets.Last());
            //    }

            //    i++;


            //}
            #endregion

            // loop through more positions till bounds end... 
            List<Vector3> possDirections2 = new List<Vector3>() { 
                Vector3.forward*(planeSize.z),
                Vector3.back*(planeSize.z),
                Vector3.left*(planeSize.x),
                Vector3.right*(planeSize.x),
                Vector3.forward*(planeSize.z+1),
                Vector3.back*(planeSize.z+1),
                Vector3.left*(planeSize.x+1),
                Vector3.right*(planeSize.x+1),
                Vector3.forward*(planeSize.z+2),
                Vector3.back*(planeSize.z+2),
                Vector3.left*(planeSize.x+2),
                Vector3.right*(planeSize.x+2),
            };


            List<Vector3> possDirections = new List<Vector3>() {
                Vector3.forward*planeSize.z*3f+Vector3.up*1f,
                Vector3.back*planeSize.z*3+Vector3.up*1f,
                Vector3.left*planeSize.x*3+Vector3.up*1f,
                Vector3.right*planeSize.x*3+Vector3.up*1f,
                Vector3.forward*planeSize.z*(3+0.5f)+Vector3.up*1f,
                Vector3.back*planeSize.z*(3+0.5f)+Vector3.up*1f,
                Vector3.left*planeSize.x*(3+0.5f)+Vector3.up*1f,
                Vector3.right*planeSize.x*(3+0.5f)+Vector3.up*1f,

            };

            List<Vector3> curPossDirList = new List<Vector3>();

            List<Vector3> failedDirections = new List<Vector3>(); // track failed directions for backtracking
            Dictionary<int, List<Vector3>> failedDirectionsAtDepth = new Dictionary<int, List<Vector3>>();


            var curPos = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);
            preEndPosOffset = preEndPos - startPos - planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);

            maxIter = i+40; // run for 100 iterations
            int depth = 0;
            failedDirectionsAtDepth[0] = failedDirections;

            curPos = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2); // be carefull how curPos is calculated
            Vector3 curVoxelPos3 = curPos + startPos;

            bool isWithinTopBounds =
                curVoxelPos3.x <= Mathf.Max(connectedRoomsOrderedByY[1].Bounds.max.x+1)
                && curVoxelPos3.x >= (Mathf.Min(connectedRoomsOrderedByY[1].Bounds.min.x + 2))
                && curVoxelPos3.z <= (Mathf.Max(connectedRoomsOrderedByY[1].Bounds.max.z+1))
                && curVoxelPos3.z >= (Mathf.Min(connectedRoomsOrderedByY[1].Bounds.min.z) + 2);


            // pathfind to closest edges?

            while (i < maxIter
                   //&&
                   //( ((curPos.x + startPos.x != preEndPos.x) // || (Mathf.Abs(curPos.z + startPos.z - preEndPos.z) >= 4))
                   //&& (((curPos.z + startPos.z != preEndPos.z)) //|| (Mathf.Abs(curPos.x + startPos.x - preEndPos.x) >= 4)

                   //))
             
                   && (((curPos.y + startPos.y) < (preEndPos.y))

                   //|| Vector3.Distance(curPos + startPos, preEndPos) > 4f
                   
                   || !isWithinTopBounds

                   || (Vector3.Distance(connectedRoomsOrderedByY[1].Bounds.min + new Vector3(1, 0, 1), curPos + startPos) < 4) // prevents from forming at entrance

                   )
                   
                //|| (isOverlapingCorridorGrid(curPos + startPos, preEndPosOffset))
                //)

                 


                )
            {

                Vector3 closestOffset = Vector3.zero; // closest position to be added to list of possible positions 
                curPossDirList = ((curPos.y + startPos.y) < (preEndPos.y) ? possDirections: possDirections2); // choose list based on height
        
                foreach (var possDirection in curPossDirList)
                {
                    curPos = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2); // be carefull how curPos is calculated
                    Vector3 curVoxelPos = curPos + possDirection + startPos;

                    // check if new moved to position
                    bool isWithinBounds =
                        curVoxelPos.x <= (Mathf.Max(connectedRoomsOrderedByY[1].Bounds.max.x, connectedRoomsOrderedByY[0].Bounds.max.x)+1)
                        && curVoxelPos.x >= (Mathf.Min(connectedRoomsOrderedByY[1].Bounds.min.x, connectedRoomsOrderedByY[0].Bounds.min.x) + 2)
                        && curVoxelPos.z <= (Mathf.Max(connectedRoomsOrderedByY[1].Bounds.max.z, connectedRoomsOrderedByY[0].Bounds.max.z)+1)
                        && curVoxelPos.z >= (Mathf.Min(connectedRoomsOrderedByY[1].Bounds.min.z, connectedRoomsOrderedByY[0].Bounds.min.z) + 2);

                    // check if direction is moving backwards back into the waypoints (prevents overlaps)
                    bool isDirectionBackwards = Vector3.Dot(
                                    Vector3.Normalize(Vector3.ProjectOnPlane(possDirection, Vector3.up)),
                                    Vector3.Normalize(Vector3.ProjectOnPlane(planeOffsets.Last(), Vector3.up))
                                    ) == -1;



                    // checks if the space moving to is free to build a waypoint  // make sure doesnt overlap within path (change)

                    bool isSpaceFree = isWithinBounds && !isOverlapingCorridorGrid(curPos + startPos, possDirection) && !isOverlapingAvailableGrid(curPos + startPos, possDirection);
                        //&& !this.corridorGrid[Mathf.FloorToInt(curVoxelPos.x) - 1, Mathf.FloorToInt(curVoxelPos.y), Mathf.FloorToInt(curVoxelPos.z) - 1]
                        //&& !this.corridorGrid[Mathf.FloorToInt(curVoxelPos.x) - 2, Mathf.FloorToInt(curVoxelPos.y), Mathf.FloorToInt(curVoxelPos.z) - 2]
                        //&& !this.corridorGrid[Mathf.FloorToInt(curVoxelPos.x) - 2, Mathf.FloorToInt(curVoxelPos.y), Mathf.FloorToInt(curVoxelPos.z) - 1]
                        //&& !this.corridorGrid[Mathf.FloorToInt(curVoxelPos.x) - 1, Mathf.FloorToInt(curVoxelPos.y), Mathf.FloorToInt(curVoxelPos.z) - 2];

                    // --- find closest position to preEndPos that is within the bounds of the top room ---
                    if (!isDirectionBackwards && isSpaceFree && !failedDirectionsAtDepth[depth].Contains(possDirection))
                    {
                        // calculcate position due to the closestOffset and possibleOffset
                        var testPos_ = curPos + possDirection + startPos;
                        var closestPos_ = curPos + closestOffset + startPos;

                        // get Manhattan distance from positions
                        var distanceFromNewPos = MeshHelper.ManhattanDistance3(testPos_, preEndPos); 
                        var distanceFromClosestPos = MeshHelper.ManhattanDistance3(closestPos_, preEndPos);


                        var preEndPosOffsetTest = preEndPos - startPos - (planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2)+possDirection);

                        // additional break condition (as path doesnt have to be shortest but just reachable to the pre end position)
                        if ((testPos_.z == preEndPos.z || testPos_.x == preEndPos.x) && ((curPos.y + startPos.y) == (preEndPos.y)) 
                            
                            //&& !isOverlapingCorridorGrid(testPos_, preEndPosOffsetTest)
                            )// && ((curPos.y + startPos.y) == (preEndPos.y)))
                        {
                            //closestOffset = possDirection;
                            //break;
                        }

                        if (distanceFromNewPos < distanceFromClosestPos || closestOffset == Vector3.zero) //  && !isOverlapingCorridorGrid(testPos_, preEndPosOffsetTest)
                        {
                            closestOffset = possDirection; // update closest position
                        }
                    }
                   
                }

                Debug.Log($"ClosestOffset {closestOffset}");

                if (closestOffset != Vector3.zero)
                {
                    planeOffsets.Add(closestOffset); // add closest position as a new waypoint for stair generation

                    curPos = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);
                    Vector3 curPosition = curPos + startPos - planeOffsets.Last();

                    updateCorridorGrid(curPosition, planeOffsets.Last(), true);

                    depth++;

                    if (!failedDirectionsAtDepth.ContainsKey(depth))
                    {
                        failedDirectionsAtDepth[depth] = new List<Vector3>();
                    }
                }
                //else if (failedDirectionsAtDepth[depth].Count() == curPossDirList.Count() && !(depth==0)) // 
                //{
                //    planeOffsets.Remove(planeOffsets.Last());
                //    depth--;
            

                //    curPos = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);
                //    Vector3 curPosition = curPos + startPos - planeOffsets.Last();

                //    updateCorridorGrid(curPosition, planeOffsets.Last(), false);

                //    failedDirectionsAtDepth[depth].Add(planeOffsets.Last());

                //    planeOffsets.Remove(planeOffsets.Last());
                //    depth--;


                //}
                //else if(!(depth == 0))
                //{

                //    curPos = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);
                //    Vector3 curPosition = curPos + startPos - planeOffsets.Last();

                //    updateCorridorGrid(curPosition, planeOffsets.Last(), false);
             
                //    failedDirectionsAtDepth[depth].Add(planeOffsets.Last());

                //    planeOffsets.Remove(planeOffsets.Last());
                //   // depth--;


                //} 
                else
                {
                    Debug.Log("Failed to Connect Vertical Corridor");
                }

                i++;

                Vector3 curVoxelPos5 = startPos + planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);
                isWithinTopBounds =
                   curVoxelPos5.x <= Mathf.Max(connectedRoomsOrderedByY[1].Bounds.max.x + 1)
                   && curVoxelPos5.x >= (Mathf.Min(connectedRoomsOrderedByY[1].Bounds.min.x + 2))
                   && curVoxelPos5.z <= (Mathf.Max(connectedRoomsOrderedByY[1].Bounds.max.z)+1)
                   && curVoxelPos5.z >= (Mathf.Min(connectedRoomsOrderedByY[1].Bounds.min.z) + 2);

                curPos = planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);
                preEndPosOffset = preEndPos - startPos - planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);
                Debug.Log($"Depth: {depth}");
            }

            #region endOffset
            // add offset right before the end positions 
            //preEndPosOffset = preEndPos - startPos - planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);
            //planeOffsets.Add(preEndPosOffset);



            // find largest direction for end point 



            // add the stiars end point
            //endOffset = endPos - startPos - planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);


            endOffset = Vector3.zero;

            maxIter = i + 20;
            bool isFound = false; 
                

            while (i < maxIter && (endOffset == Vector3.zero 
                ))
            {
                foreach (var dir in pathPossibleDirections)
                {
                    var testEndOffset = 5 * dir + Vector3.up;

                    var curVoxelPos2 = startPos + testEndOffset + planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2);

                    bool isWithinBounds =
                     curVoxelPos2.x <= Mathf.Max(connectedRoomsOrderedByY[1].Bounds.max.x)
                     && curVoxelPos2.x >= (Mathf.Min(connectedRoomsOrderedByY[1].Bounds.min.x + 2))
                     && curVoxelPos2.z <= (Mathf.Max(connectedRoomsOrderedByY[1].Bounds.max.z))
                     && curVoxelPos2.z >= (Mathf.Min(connectedRoomsOrderedByY[1].Bounds.min.z) + 2);

                    bool isDirectionBackwards = Vector3.Dot(
                     Vector3.Normalize(new Vector3(testEndOffset.x, 0, testEndOffset.z)),
                     Vector3.Normalize(new Vector3(planeOffsets.Last().x, 0, planeOffsets.Last().z))
                     ) == -1;

                    Debug.Log($"TestPos --- : {isDirectionBackwards}, {testEndOffset}, {planeOffsets.Last()}, {curVoxelPos2}");

                    if (isWithinBounds && !isOverlapingCorridorGrid(startPos + planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2), testEndOffset) && !isDirectionBackwards


                       &&  Vector3.Distance(connectedRoomsOrderedByY[1].Bounds.min + new Vector3(1, 0, 1), testEndOffset + startPos + planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2)) > 4
                       // && Vector3.Distance(connectedRoomsOrderedByY[1].Bounds.min + new Vector3(1, 0, 1), startPos + endOffset + planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2)) > 20
                        )
                    {
                        endOffset = testEndOffset;
                        isFound = true;
                    }

                    

                }
                if (isFound)
                {
                    break;
                }

                i++;

                if (planeOffsets.Last().y == 0)
                {
                    planeOffsets.RemoveAt(planeOffsets.Count()-1);
                }
            }


            planeOffsets.Add(endOffset);


            #endregion

            // update corridorNode properties
            this.StairPosition = startPos;
            this.StairSize = planeSize;
            this.StairWaypoints = planeOffsets;



            // --- Generate Bounds for the Corridors --- 
            var pos1 = Vector3Int.FloorToInt(startPos + planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2)- planeSize - planeOffsets.Last());
            var pos2 = Vector3Int.FloorToInt(startPos + planeOffsets.Aggregate((vec1, vec2) => vec1 + vec2) - planeSize);

            var posRes = pos2 - pos1;
            var absPosRes = Vector3.Normalize(new Vector3Int(Mathf.Abs(posRes.x), 0, Mathf.Abs(posRes.z)));

            Bounds = new BoundsInt(
                (pos2.x > pos1.x || pos2.z > pos1.z) ? pos1 : pos2,
                Vector3Int.Scale(Vector3Int.FloorToInt(absPosRes), new Vector3Int(6, 0, 6)) + Vector3Int.Scale(new Vector3Int(1,0,1) - Vector3Int.FloorToInt(absPosRes), new Vector3Int(2, 0, 2)) //new Vector3Int(6, 2, 2) // this.corridorWidth
            );

            //MeshHelper.VisualizeVoxelGrid(this.corridorGrid);
        }

        private void updateCorridorGrid(Vector3 startPosition, Vector3 offset, bool value)
        {
            for (int x = 0; x <= Mathf.Abs(offset.x); x++)
            {
                for (int y = 0; y <= Mathf.Abs(offset.y); y++)
                {
                    for (int z = 0; z <= Mathf.Abs(offset.z); z++)
                    {
                        this.corridorGrid[Mathf.FloorToInt(startPosition.x) - 1 + x * Math.Sign(offset.x), Mathf.FloorToInt(startPosition.y) + y * Math.Sign(offset.y), Mathf.FloorToInt(startPosition.z) + z * Math.Sign(offset.z) - 1] = value;
                        this.corridorGrid[Mathf.FloorToInt(startPosition.x) - 2 + x * Math.Sign(offset.x), Mathf.FloorToInt(startPosition.y) + y * Math.Sign(offset.y), Mathf.FloorToInt(startPosition.z) + z * Math.Sign(offset.z) - 2] = value;
                        this.corridorGrid[Mathf.FloorToInt(startPosition.x) - 2 + x * Math.Sign(offset.x), Mathf.FloorToInt(startPosition.y) + y * Math.Sign(offset.y), Mathf.FloorToInt(startPosition.z) + z * Math.Sign(offset.z) - 1] = value;
                        this.corridorGrid[Mathf.FloorToInt(startPosition.x) - 1 + x * Math.Sign(offset.x), Mathf.FloorToInt(startPosition.y) + y * Math.Sign(offset.y), Mathf.FloorToInt(startPosition.z) + z * Math.Sign(offset.z) - 2] = value;

                    }

                }

            }
        }

        private bool isOverlapingCorridorGrid(Vector3 startPosition, Vector3 offset)
        {
            //Debug.Log($"Is Overlapping Path {offset}");


            Vector3Int nOff = new Vector3Int((Math.Abs(offset.x) > 0) ? 1 : 0, (Math.Abs(offset.y) > 0) ? 1 : 0, (Math.Abs(offset.z) > 0) ? 1 : 0); // to prevent duplicate checking start direction

            for (int x = 2*nOff.x; x <= Mathf.Abs(offset.x); x++)
            {
                for (int y = 0; y <= Mathf.Abs(offset.y); y++)
                {
                    for (int z = 2* nOff.z; z <= Mathf.Abs(offset.z); z++) // at least as wide as offset
                    {


                        var curVoxelPos = startPosition + offset;

                        bool isOverlapping =

                        //this.corridorGrid[Mathf.FloorToInt(curVoxelPos.x) - 1 + x * Math.Sign(offset.x), Mathf.FloorToInt(curVoxelPos.y), Mathf.FloorToInt(curVoxelPos.z) - 1]
                        //&& this.corridorGrid[Mathf.FloorToInt(curVoxelPos.x) - 2, Mathf.FloorToInt(curVoxelPos.y), Mathf.FloorToInt(curVoxelPos.z) - 2]
                        //&& this.corridorGrid[Mathf.FloorToInt(curVoxelPos.x) - 2, Mathf.FloorToInt(curVoxelPos.y), Mathf.FloorToInt(curVoxelPos.z) - 1]
                        //&& this.corridorGrid[Mathf.FloorToInt(curVoxelPos.x) - 1, Mathf.FloorToInt(curVoxelPos.y), Mathf.FloorToInt(curVoxelPos.z) - 2];


                        this.corridorGrid[Mathf.FloorToInt(startPosition.x) - 1 + x * Math.Sign(offset.x), Mathf.FloorToInt(startPosition.y) + y * Math.Sign(offset.y), Mathf.FloorToInt(startPosition.z) + z * Math.Sign(offset.z) - 1]
                        || this.corridorGrid[Mathf.FloorToInt(startPosition.x) - 2 + x * Math.Sign(offset.x), Mathf.FloorToInt(startPosition.y) + y * Math.Sign(offset.y), Mathf.FloorToInt(startPosition.z) + z * Math.Sign(offset.z) - 2]
                        || this.corridorGrid[Mathf.FloorToInt(startPosition.x) - 1 + x * Math.Sign(offset.x), Mathf.FloorToInt(startPosition.y) + y * Math.Sign(offset.y), Mathf.FloorToInt(startPosition.z) + z * Math.Sign(offset.z) - 1]
                        || this.corridorGrid[Mathf.FloorToInt(startPosition.x) - 2 + x * Math.Sign(offset.x), Mathf.FloorToInt(startPosition.y) + y * Math.Sign(offset.y), Mathf.FloorToInt(startPosition.z) + z * Math.Sign(offset.z) - 2];

                        Debug.Log($"Is Overlapping Path {isOverlapping}, {offset}");

                        return isOverlapping;
                    }

                }

            }

            return false;
        }

        private bool isOverlapingAvailableGrid(Vector3 startPosition, Vector3 offset)
        {
            //Debug.Log($"Is Overlapping Path {offset}");


            Vector3Int nOff = new Vector3Int((Math.Abs(offset.x) > 0) ? 1 : 0, (Math.Abs(offset.y) > 0) ? 1 : 0, (Math.Abs(offset.z) > 0) ? 1 : 0); // to prevent duplicate checking start direction

            for (int x = 2 * nOff.x; x <= Mathf.Abs(offset.x); x++)
            {
                for (int y = 0; y <= Mathf.Abs(offset.y); y++)
                {
                    for (int z = 2 * nOff.z; z <= Mathf.Abs(offset.z); z++) // at least as wide as offset
                    {


                        var curVoxelPos = startPosition + offset;

                        bool isOverlapping =

                        //this.corridorGrid[Mathf.FloorToInt(curVoxelPos.x) - 1 + x * Math.Sign(offset.x), Mathf.FloorToInt(curVoxelPos.y), Mathf.FloorToInt(curVoxelPos.z) - 1]
                        //&& this.corridorGrid[Mathf.FloorToInt(curVoxelPos.x) - 2, Mathf.FloorToInt(curVoxelPos.y), Mathf.FloorToInt(curVoxelPos.z) - 2]
                        //&& this.corridorGrid[Mathf.FloorToInt(curVoxelPos.x) - 2, Mathf.FloorToInt(curVoxelPos.y), Mathf.FloorToInt(curVoxelPos.z) - 1]
                        //&& this.corridorGrid[Mathf.FloorToInt(curVoxelPos.x) - 1, Mathf.FloorToInt(curVoxelPos.y), Mathf.FloorToInt(curVoxelPos.z) - 2];


                        this.availableVoxelGrid[Mathf.FloorToInt(startPosition.x) - 1 + x * Math.Sign(offset.x), Mathf.FloorToInt(startPosition.y) + y * Math.Sign(offset.y), Mathf.FloorToInt(startPosition.z) + z * Math.Sign(offset.z) - 1]
                        || this.availableVoxelGrid[Mathf.FloorToInt(startPosition.x) - 2 + x * Math.Sign(offset.x), Mathf.FloorToInt(startPosition.y) + y * Math.Sign(offset.y), Mathf.FloorToInt(startPosition.z) + z * Math.Sign(offset.z) - 2]
                        || this.availableVoxelGrid[Mathf.FloorToInt(startPosition.x) - 2 + x * Math.Sign(offset.x), Mathf.FloorToInt(startPosition.y) + y * Math.Sign(offset.y), Mathf.FloorToInt(startPosition.z) + z * Math.Sign(offset.z) - 1]
                        || this.availableVoxelGrid[Mathf.FloorToInt(startPosition.x) - 1 + x * Math.Sign(offset.x), Mathf.FloorToInt(startPosition.y) + y * Math.Sign(offset.y), Mathf.FloorToInt(startPosition.z) + z * Math.Sign(offset.z) - 2];

                        Debug.Log($"Is Overlapping Path {isOverlapping}, {offset}");

                        if (isOverlapping)
                        {
                            //Debug.Log("Is Overlapping Path");
                            return true;
                        }
                    }

                }

            }

            return false;
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