using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Random = UnityEngine.Random;

namespace dungeonGenerator
{
    public class CorridorNode : Node

    {
        private Node node1;
        private Node node2;
        private int corridorWidth;
        private int wallThickness;
        private Vector2Int minRoomDim;

        public CorridorNode(Node node1, Node node2, int corridorWidth, int wallThickness, Vector2Int minRoomDim) : base(null) // null since it doesnt have any parents
        {
            this.node1 = node1;
            this.node2 = node2;
            this.corridorWidth = corridorWidth;
            this.wallThickness = wallThickness;
            this.minRoomDim = minRoomDim;

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
            ).OrderBy(rightSpace => rightSpace.Bounds.min.x).ToList(); // order by ascending (smallest) x

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
                rightSpace = neighborsInRightSpaceList[Random.Range(0, neighborsInRightSpaceList.Count)]; // if neighbors exist choose one at random 
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
                        rightSpace = neighborsInRightSpaceList[Random.Range(0, neighborsInRightSpaceList.Count)];
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

            // --- Generate Bounds for the Corridors --- 

            var sizeX = rightSpace.Bounds.min.x - leftSpace.Bounds.max.x;
            var pos = new Vector3Int(leftSpace.Bounds.max.x, 0, corridorZ-Mathf.FloorToInt(this.corridorWidth/2f)); 

            Bounds = new BoundsInt(
                pos,
                new Vector3Int(sizeX, 0, this.corridorWidth)
            );
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

            GenerateCorridorBoundsLeftRight(leftSpace, rightSpace);

            // --- Add Neighbours to the Connection List of Respective Nodes --- 

            leftSpace.ConnectionsList.Add(rightSpace);
            rightSpace.ConnectionsList.Add(leftSpace);

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
            ).OrderByDescending(bottomSpace => bottomSpace.Bounds.max.z).ToList(); // order by descenmding (max) z

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
                bottomSpace = neighborsInBottomSpaceList[Random.Range(0, neighborsInBottomSpaceList.Count)]; // if neighbors exist choose one at random 
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
                        bottomSpace = neighborsInBottomSpaceList[Random.Range(0, neighborsInBottomSpaceList.Count)];
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
            var pos = new Vector3Int(corridorX - Mathf.FloorToInt(this.corridorWidth / 2f), 0, bottomSpace.Bounds.max.z);

            Bounds = new BoundsInt(
                pos,
                new Vector3Int(this.corridorWidth, 0, sizeZ)
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

            GenerateCorridorBoundsTopBottom(topSpace, bottomSpace);

            // --- Add Neighbours to the Connection List of Respective Nodes --- 

            topSpace.ConnectionsList.Add(bottomSpace);
            bottomSpace.ConnectionsList.Add(topSpace);

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
    }
}