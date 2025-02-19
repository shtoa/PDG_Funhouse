using Codice.CM.SEIDInfo;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace dungeonGenerator
{

    public enum LateralCorridorType
    {
        Corridor,
        Stairs,
        Ladder
    }
    public class CorridorNode : Node

    {
        private Node node1;
        private Node node2;
        private int corridorWidth;
        private int wallThickness;
        private Vector3Int minRoomDim;
        private int corridorHeight;

        public CorridorNode(Node node1, Node node2, int corridorWidth, int wallThickness, Vector3Int minRoomDim, int corridorHeight) : base(null) // null since it doesnt have any parents
        {
            this.node1 = node1;
            this.node2 = node2;
            this.corridorWidth = corridorWidth;
            this.wallThickness = wallThickness;
            this.minRoomDim = minRoomDim;
            this.corridorHeight = corridorHeight;

            GenerateCorridor();
        }

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
                //case SplitPosition.Down:
                //    GenerateCorridorUpDown(this.node1, this.node2);
                //    break;

                default:
                    Debug.Log("UNKOWN TYPE");
                    Bounds = new BoundsInt(new Vector3Int(0,0,0), new Vector3Int(1,1,1));
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

        private List<Node> ReturnPossibleNeighborsRightSpace(Node leftSpace, List<Node> rightSpaceNeighborCandidates)
        {
            return rightSpaceNeighborCandidates.Where(rightSpace =>
                GetCorridorPositionLeftRightZ(leftSpace, rightSpace) != -1 
                // && (leftSpace.FloorIndex == rightSpace.FloorIndex)
            ).OrderBy(rightSpace => rightSpace.Bounds.min.x).Where(rightSpace => Mathf.Abs(rightSpace.Bounds.min.y - leftSpace.Bounds.min.y) == 0).ToList(); // order by ascending (smallest) x

            /* FOR NOW NO LADDERS / STAIR CASES
            * 
            * No Ladder Condition: .Where(rightSpace => Mathf.Abs(rightSpace.Bounds.min.y - leftSpace.Bounds.min.y) == 0).ToList()
            * .Where(rightSpace => Mathf.Abs(rightSpace.Bounds.min.y - leftSpace.Bounds.min.y) == 0)
           */
        }

        private (Node leftSpace, Node rightSpace) FindNeighborsLeftRight(List<Node> sortedLeftSpaces, List<Node> sortedRightSpaces)
        {
            // intialize output nodes
            Node rightSpace = null;
            Node leftSpace = null;

            leftSpace = sortedLeftSpaces[Random.Range(0, sortedLeftSpaces.Count)]; // pick a left space from candidates
            var neighborsInRightSpaceList = ReturnPossibleNeighborsRightSpace(leftSpace, sortedRightSpaces); // get possible neighbors in rightSpace

            if (neighborsInRightSpaceList.Count() > 0)
            {
                rightSpace = neighborsInRightSpaceList[Random.Range(0, neighborsInRightSpaceList.Count)]; //[Random.Range(0, neighborsInRightSpaceList.Count)]; // if neighbors exist choose one at random 
            }
            else
            {
                // --- Added Check if no neighbors are found to reselect leftSpace

                sortedLeftSpaces.Remove(leftSpace);

                foreach (var newLeftSpace in sortedLeftSpaces)
                {
                    neighborsInRightSpaceList = ReturnPossibleNeighborsRightSpace(newLeftSpace, sortedRightSpaces); // select new LeftSpace and check if it has any neighbors

                    if (neighborsInRightSpaceList.Count() > 0) // if neighbors are found set leftSpace to newly selected leftSpace and select rightSpace randomly
                    {
                        leftSpace = newLeftSpace;
                        rightSpace = neighborsInRightSpaceList[Random.Range(0, neighborsInRightSpaceList.Count)]; // select closest one //[Random.Range(0, neighborsInRightSpaceList.Count)];
                        break;
                    }
                }
            }

            return (leftSpace, rightSpace);
        }

        public void GenerateCorridorBoundsLeftRight(Node leftSpace, Node rightSpace)
        {
            int corridorZ = GetCorridorPositionLeftRightZ(leftSpace, rightSpace);
            #region Debug Corridor
            if (corridorZ == -1)
            {
                // Incase no Neighbours are Found 
                // TODO: Move this to Unit Testing
                Debug.Log("Wrong Size");
            }

            if (leftSpace == null || rightSpace == null)
            {
                Debug.Log("Spaces not Found");
            }
            #endregion


            // --- Get Lateral Corridor Type --- 
            LateralCorridorType t = GetLateralCorridorType(leftSpace, rightSpace, SplitPosition.Left);

            Debug.Log($"type of corridor {t}");

            // --- Generate Bounds for Corridor Type --- 

            // --- Generate Bounds for the Corridors --- 

            if (t == LateralCorridorType.Corridor) { 

                var sizeX = rightSpace.Bounds.min.x - leftSpace.Bounds.max.x;
                var pos = new Vector3Int(leftSpace.Bounds.max.x, leftSpace.Bounds.min.y, corridorZ-Mathf.FloorToInt(this.corridorWidth/2f)); 

                Bounds = new BoundsInt(
                    pos,
                    new Vector3Int(sizeX, this.corridorHeight, this.corridorWidth) // assumption that both spaces are the same height
                );
            } else // FIX ME: Make More Concise
            {

                var doorHeight = 2;

                if (leftSpace.Bounds.min.y < rightSpace.Bounds.min.y)
                {
                    var sizeX = rightSpace.Bounds.min.x - leftSpace.Bounds.max.x;
                    var pos = new Vector3Int(leftSpace.Bounds.max.x, leftSpace.Bounds.min.y, corridorZ - Mathf.FloorToInt(this.corridorWidth / 2f));

                    Bounds = new BoundsInt(
                        pos,
                        new Vector3Int(sizeX, rightSpace.Bounds.min.y - leftSpace.Bounds.min.y + doorHeight, this.corridorWidth) // assumption that both spaces are the same height
                    );
                }
                else
                {
                    var sizeX = rightSpace.Bounds.min.x - leftSpace.Bounds.max.x;
                    var pos = new Vector3Int(leftSpace.Bounds.max.x, rightSpace.Bounds.min.y, corridorZ - Mathf.FloorToInt(this.corridorWidth / 2f));

                    Bounds = new BoundsInt(
                        pos,
                        new Vector3Int(sizeX, leftSpace.Bounds.min.y - rightSpace.Bounds.min.y + doorHeight, this.corridorWidth) // assumption that both spaces are the same height
                    );
                }
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

            Debug.Log($"Angle {Vector3.Angle(new Vector3(1,0,0), (pos2-pos1).normalized)}");

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

            // --- Find Neighbor pair in LeftSpaces and RightSpaces ---

            (leftSpace,rightSpace) = FindNeighborsLeftRight(sortedLeftSpaces, sortedRightSpaces);

            // --- Generate Corridor Between LeftSpace and RightSpace --- 

            Debug.Log($"LeftSpace Value {leftSpace is null} children in leftNode {leftNode.ChildrenNodeList.Count} leaves {leftSpaceLeaves.Count}");
            Debug.Log($"RightSpace Value {rightSpace is null} children in rightNode {rightNode.ChildrenNodeList.Count} leaves {rightSpaceLeaves.Count}");

            if (leftSpace == null || rightSpace == null)
            {
                this.CorridorType = CorridorType.None;
                return;
            }

            GenerateCorridorBoundsLeftRight(leftSpace, rightSpace);
        
            // --- Add Neighbours to the Connection List of Respective Nodes --- 

            leftSpace.ConnectionsList.Add(rightSpace);
            rightSpace.ConnectionsList.Add(leftSpace);


            // --- calculate the bounds of the door to be used in mesh generation ---
            

            // calculate the bounds of the door for the left space and the right spaces
            leftSpace.calculateDoorPlacement(this.Bounds, SplitPosition.Left, wallThickness);
            rightSpace.calculateDoorPlacement(this.Bounds, SplitPosition.Right, wallThickness);
            
            // add the doorbounds found for the left and right space to the corridor
            this.addDoorPlacement(leftSpace.DoorPlacements.Last());
            this.addDoorPlacement(rightSpace.DoorPlacements.Last());

            this.CorridorType = CorridorType.Horizontal;

        }

        /// <summary>
        /// Get the Position of the Corridor Along the Z-Axis
        /// </summary>
        /// <param name="leftSpace"></param>
        /// <param name="rightSpace"></param>
        /// <returns></returns>

        private int GetCorridorPositionLeftRightZ(Node leftSpace, Node rightSpace)
        {

            // right space is within bounds of left space
            if (leftSpace.Bounds.max.z >= rightSpace.Bounds.max.z && rightSpace.Bounds.min.z >= leftSpace.Bounds.min.z)
            {
                return CalculateMiddlePoint(
                    rightSpace.Bounds.min,
                    rightSpace.Bounds.max
                ).z;
            }
            // right space contain bounds of left space
            if (rightSpace.Bounds.max.z >= leftSpace.Bounds.max.z && leftSpace.Bounds.min.z >= rightSpace.Bounds.min.z)
            {
                return CalculateMiddlePoint(
                    leftSpace.Bounds.min,
                    leftSpace.Bounds.max
                ).z;
            }

            // right space is above left space
            if (leftSpace.Bounds.max.z >= rightSpace.Bounds.min.z && rightSpace.Bounds.min.z > leftSpace.Bounds.min.z) {

                if (leftSpace.Bounds.max.z - rightSpace.Bounds.min.z <= this.corridorWidth + wallThickness)
                {
                    return -1;
                }

                return CalculateMiddlePoint(
                    rightSpace.Bounds.min,
                    leftSpace.Bounds.max
                ).z;
            }
            // right space is bellow left space
            if (rightSpace.Bounds.max.z >= leftSpace.Bounds.min.z && leftSpace.Bounds.min.z > rightSpace.Bounds.min.z) // before was >=
            {

                if (rightSpace.Bounds.max.z - leftSpace.Bounds.min.z <= this.corridorWidth + wallThickness)
                {
                    return -1;
                }

                return CalculateMiddlePoint(
                   leftSpace.Bounds.min,
                   rightSpace.Bounds.max
                ).z;
            }

            return -1;
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
        private List<Node> ReturnPossibleNeighborsBottomSpace(Node topSpace, List<Node> bottomSpaceNeighborCandidates)
        {
            return bottomSpaceNeighborCandidates.Where(bottomSpace =>
                GetCorridorPositionTopBottomX(topSpace, bottomSpace) != -1 
                // && (bottomSpace.FloorIndex == topSpace.FloorIndex)
            ).OrderByDescending(bottomSpace => bottomSpace.Bounds.max.z).Where(bottomSpace => Mathf.Abs(topSpace.Bounds.min.y - bottomSpace.Bounds.min.y) == 0).ToList(); // order by descenmding (max) z

            /* FOR NOW NO LADDERS / STAIR CASES
             * 
             * No Ladder Condition: .Where(bottomSpace => Mathf.Abs(topSpace.Bounds.min.y - bottomSpace.Bounds.min.y) == 0)
            */
        }
        private (Node topSpace, Node bottomSpace) FindNeighborsTopBottom(List<Node> sortedTopSpace, List<Node> sortedBottomSpace)
        {
            // intialize output nodes
            Node bottomSpace = null;
            Node topSpace = null;

            topSpace = sortedTopSpace[Random.Range(0, sortedTopSpace.Count)]; // pick a left space from candidates
            var neighborsInBottomSpaceList = ReturnPossibleNeighborsBottomSpace(topSpace, sortedBottomSpace); // get possible neighbors in rightSpace

            if (neighborsInBottomSpaceList.Count() > 0)
            {
                bottomSpace = neighborsInBottomSpaceList[0]; // [Random.Range(0, neighborsInBottomSpaceList.Count)]; // if neighbors exist choose one at random 
            }
            else
            {
                // --- Added Check if no neighbors are found to reselect leftSpace

                sortedTopSpace.Remove(topSpace);

                foreach (var newTopSpace in sortedTopSpace)
                {
                    neighborsInBottomSpaceList = ReturnPossibleNeighborsBottomSpace(newTopSpace, sortedBottomSpace); // select new LeftSpace and check if it has any neighbors

                    if (neighborsInBottomSpaceList.Count() > 0) // if neighbors are found set leftSpace to newly selected leftSpace and select rightSpace randomly
                    {
                        topSpace = newTopSpace;
                        bottomSpace = neighborsInBottomSpaceList[0]; //[Random.Range(0, neighborsInBottomSpaceList.Count)];
                        break;
                    }
                }
            }

            return (topSpace, bottomSpace);
        }
        public void GenerateCorridorBoundsTopBottom(Node topSpace, Node bottomSpace)
        {
            int corridorX = GetCorridorPositionTopBottomX(topSpace, bottomSpace);

            #region Debug Corridor
            if (corridorX == -1)
            {
                // Incase no Neighbours are Found 
                // TODO: Move this to Unit Testing
                Debug.Log("Wrong Size");
            }

            if (topSpace == null || bottomSpace == null)
            {
                Debug.Log("Spaces not Found");
            }
            #endregion

            // --- Generate Bounds for the Corridors --- 
            var sizeZ = topSpace.Bounds.min.z - bottomSpace.Bounds.max.z;
            var pos = new Vector3Int(corridorX - Mathf.FloorToInt(this.corridorWidth / 2f), topSpace.Bounds.min.y, bottomSpace.Bounds.max.z);

            Bounds = new BoundsInt(
                pos,
                new Vector3Int(this.corridorWidth, this.corridorHeight, sizeZ) // assumption that both spaces are the same height
            );
        }
        #endregion
        private void GenerateCorridorTopBottom(Node topNode, Node bottomNode)
        {
            // --- Initialize Leaves Arrays and Spaces to Connect ---

            Node topSpace = null; // left space to connect
            List<Node> topSpaceLeaves = GraphHelper.GetLeaves(topNode); // get all the leaves in the left space

            Node bottomSpace = null; // right space to connect
            List<Node> bottomSpaceLeaves = GraphHelper.GetLeaves(bottomNode); // get all the leaves in the right space

            // --- select (right most) LeftSpace and (left most) RightSpace to Connect ---

            var sortedTopSpaces = ReturnBottomMostSpaces(topSpaceLeaves);
            var sortedBottomSpaces = ReturnTopMostSpaces(bottomSpaceLeaves);

            // --- Find Neighbor pair in LeftSpaces and RightSpaces ---

            (topSpace, bottomSpace) = FindNeighborsTopBottom(sortedTopSpaces, sortedBottomSpaces);

            // --- Generate Corridor Between LeftSpace and RightSpace --- 

            Debug.Log($"TopSpace Value {topSpace is null} children in topNode {topNode.ChildrenNodeList.Count}");
            Debug.Log($"BottomSpace Value {bottomSpace is null} children in bottomNode {bottomNode.ChildrenNodeList.Count}");

            if (topSpace == null || bottomSpace == null)
            {
                this.CorridorType = CorridorType.None;
                return;
            }

            GenerateCorridorBoundsTopBottom(topSpace, bottomSpace);

            // --- Add Neighbours to the Connection List of Respective Nodes --- 

            topSpace.ConnectionsList.Add(bottomSpace);
            bottomSpace.ConnectionsList.Add(topSpace);

            // --- calculate the bounds of the door to be used in mesh generation ---


            // calculate the bounds of the door for the left space and the right spaces
            topSpace.calculateDoorPlacement(this.Bounds, SplitPosition.Top, wallThickness);
            bottomSpace.calculateDoorPlacement(this.Bounds, SplitPosition.Bottom, wallThickness);

            // add the doorbounds found for the left and right space to the corridor
            this.addDoorPlacement(topSpace.DoorPlacements.Last());
            this.addDoorPlacement(bottomSpace.DoorPlacements.Last());

            this.CorridorType = CorridorType.Vertical;


        }
        private int GetCorridorPositionTopBottomX(Node topSpace, Node bottomSpace)
        {
            // right space is above left space
            if (topSpace.Bounds.max.x >= bottomSpace.Bounds.min.x && bottomSpace.Bounds.min.x > topSpace.Bounds.min.x)
            {
                if (topSpace.Bounds.max.x - bottomSpace.Bounds.min.x <= this.corridorWidth + wallThickness)
                {
                    return -1;
                }

                return CalculateMiddlePoint(
                    bottomSpace.Bounds.min,
                    topSpace.Bounds.max
                    ).x;
            }
            // right space is bellow left space
            if (bottomSpace.Bounds.max.x >= topSpace.Bounds.min.x && topSpace.Bounds.min.x > bottomSpace.Bounds.min.x) // before was >=
            {

                if (bottomSpace.Bounds.max.x - topSpace.Bounds.min.x <= this.corridorWidth + wallThickness)
                {
                    return -1;
                }

                return CalculateMiddlePoint(
                   topSpace.Bounds.min,
                   bottomSpace.Bounds.max
                ).x;
            }
            // right space is within bounds of left space
            if (topSpace.Bounds.max.x >= bottomSpace.Bounds.max.x && bottomSpace.Bounds.min.x >= topSpace.Bounds.min.x)
            {
                return CalculateMiddlePoint(
                    bottomSpace.Bounds.min,
                    bottomSpace.Bounds.max
                ).x;
            }
            // right space contain bounds of left space
            if (bottomSpace.Bounds.max.x >= topSpace.Bounds.max.x && topSpace.Bounds.min.x >= bottomSpace.Bounds.min.x)
            {
                return CalculateMiddlePoint(
                    topSpace.Bounds.min,
                    topSpace.Bounds.max
                ).x;
            }

            return -1;
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
            return downSpaceNeighborCandidates.Where(downSpace =>
                GetCorridorPositionUpDownXZ(upSpace, downSpace).x != -1 &&
                GetCorridorPositionUpDownXZ(upSpace, downSpace).z != -1 
                
                //&& Mathf.Abs(downSpace.Bounds.max.y - upSpace.Bounds.min.y) < minRoomDim.z // FIX ME: Rewrite minRoomDim with correct yz switch
                
                // otherwise there is a room inbetween // stops from generating updown corridors through rooms



                // && (upSpace.FloorIndex-1 == downSpace.FloorIndex)
            ).OrderByDescending(downSpace => downSpace.Bounds.max.y).ToList(); // order by descending (max) y

        }
        private (Node upSpace, Node downSpace) FindNeighborsUpDown(List<Node> sortedUpSpace, List<Node> sortedDownSpace)
        {
            // intialize output nodes
            Node downSpace = null;
            Node upSpace = null;

            upSpace = sortedUpSpace[Random.Range(0, sortedUpSpace.Count)]; // pick a left space from candidates
            var neighborsInDownSpaceList = ReturnPossibleNeighborsDownSpace(upSpace, sortedDownSpace); // get possible neighbors in rightSpace

            if (neighborsInDownSpaceList.Count() > 0)
            {
                downSpace = neighborsInDownSpaceList[Random.Range(0, neighborsInDownSpaceList.Count)]; // if neighbors exist choose one at random 
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
                        downSpace = neighborsInDownSpaceList[Random.Range(0, neighborsInDownSpaceList.Count)];
                        break;
                    }
                }
            }

            return (upSpace, downSpace);
        }
        public void GenerateCorridorBoundsUpDown(Node upSpace, Node downSpace)
        {
            Vector3Int corridorXZ = GetCorridorPositionUpDownXZ(upSpace, downSpace);

            #region Debug Corridor
            if (corridorXZ.x == -1  || corridorXZ.z == -1)
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
                new Vector3Int(this.corridorWidth, (upSpace.Bounds.min.y-downSpace.Bounds.max.y), this.corridorWidth)
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

            // --- Find Neighbor pair in LeftSpaces and RightSpaces ---

            (upSpace, downSpace) = FindNeighborsUpDown(sortedUpSpaces, sortedDownSpaces);

            // --- Generate Corridor Between LeftSpace and RightSpace --- 

            Debug.Log($"Upspace Value {upSpace is null} children in upNode {upNode.ChildrenNodeList.Count}");
            Debug.Log($"DownSpace Value {downSpace is null} children in downNode {downNode.ChildrenNodeList.Count}");


           if(upSpace is null || downSpace is null)
           {

                this.CorridorType = CorridorType.None;
                return;
                
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
                this.Bounds.size - new Vector3Int(this.wallThickness, 0, this.wallThickness)*2
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
                new Vector3Int(upSpace.Bounds.size.x,0, upSpace.Bounds.size.z)
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

            if(unboundedContains(upSpacePlane, downSpacePlane.position + new Vector3Int(downSpacePlane.size.x, 0,0))) // contains bottomRightCorner
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

            if (spaceSufficientSize && nIntersections>0 || (spaceSufficientSize && downSpaceAndupSpaceSame) ||
               
                (spaceSufficientSize
                && unboundedContains(downSpacePlane,upSpacePlane.position)
                && unboundedContains(downSpacePlane, upSpacePlane.position + new Vector3Int(upSpacePlane.size.x,0,0))
                && unboundedContains(downSpacePlane, upSpacePlane.position + new Vector3Int(0, 0, upSpacePlane.size.z))
                && unboundedContains(downSpacePlane, upSpacePlane.position + new Vector3Int(upSpacePlane.size.x, 0, upSpacePlane.size.z)))

                )
            {




                // return random point inside the possible upSpace plane 


                // FIX ME: DOUBLE CHECK

                Vector3Int holePosition = new Vector3Int(
                        Random.Range(upSpacePlane.min.x, upSpacePlane.max.x - corridorWidth),
                        0,
                        Random.Range(upSpacePlane.min.z,  upSpacePlane.max.z - corridorWidth)
                    );

                Debug.Log($"holePos Vector {holePosition}");
                Debug.Log($@"holePos is contained {holePosition}:
                        contained in upSpace {unboundedContains(upSpacePlane, holePosition)}
                        contained in downSpace {unboundedContains(downSpacePlane, holePosition)}
                    ");


                if(!(unboundedContains(upSpacePlane, holePosition) && unboundedContains(downSpacePlane, holePosition)))
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