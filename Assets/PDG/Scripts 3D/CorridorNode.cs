using dungeonGenerator;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Random = UnityEngine.Random;

namespace dungeonGenerator
{
    public class CorridorNode : Node
    {
        private Node node1;
        private Node node2;
        private int corridorWidth;
        private int modifierDistanceFromWall = 3;

        public CorridorNode(Node node1, Node node2, int corridorWidth) : base(null) // null since it doesnt have any parents
        {
            this.node1 = node1;
            this.node2 = node2;
            this.corridorWidth = corridorWidth;

            GenerateCorridor();
        }

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

        private void GenerateCorridorRightLeft(Node node1, Node node2)
        {
            Node leftSpace = null; // left space to connect
            List<Node> leftSpaceLeaves = GraphHelper.GetLeaves(node2);

            Node rightSpace = null; // right space to connect
            List<Node> rightSpaceLeaves = GraphHelper.GetLeaves(node1);

            var sortedLeftSpace = leftSpaceLeaves.OrderByDescending(child => child.Bounds.max.x).ToList(); // get right most children of left space
            if(sortedLeftSpace.Count == 1)
            {
                leftSpace = sortedLeftSpace[0];

            } else
            {
                // add randomness to which rooms are connected 
                int maxX = sortedLeftSpace[0].Bounds.max.x;
                sortedLeftSpace = sortedLeftSpace.Where(Child => Math.Abs(maxX-Child.Bounds.max.x) < 10).ToList(); // find rooms that have the least deviation from maxX

                // select a random room from valid rooms
                leftSpace = sortedLeftSpace[Random.RandomRange(0, sortedLeftSpace.Count)];

            }

            // find possible connection for the most left aligned room in the right space
            var neighborsInLeftSpaceList = rightSpaceLeaves.Where(
                child => GetCorridorPositionLeftRightZ(leftSpace, child) != -1

                ).OrderBy(child => child.Bounds.min.x).ToList(); // order by ascending (smallest) x

            if(neighborsInLeftSpaceList.Count() <= 0)
            {
                rightSpace = node1;
            } else
            {
                // possibly can randomize this
                rightSpace = neighborsInLeftSpaceList[0];
            }

            // potentially add check for not enough clearance
            int corridorZ = GetCorridorPositionLeftRightZ(leftSpace, rightSpace);

            while(corridorZ == -1 && sortedLeftSpace.Count > 1)
            {
                // Remove previous space if it was incorrect
                sortedLeftSpace.Remove(leftSpace);
                
                // Get Next Possible Neightbour
                leftSpace = sortedLeftSpace[0];
                corridorZ = GetCorridorPositionLeftRightZ(leftSpace, rightSpace); // test if neighbour can be connected using a straight corridor
            }

            


            var midPointX = (rightSpace.Bounds.min.x + leftSpace.Bounds.max.x)/2f;
            var sizeX = rightSpace.Bounds.min.x - leftSpace.Bounds.max.x;

            var pos = new Vector3Int((int)midPointX-sizeX/2, 0, corridorZ-this.corridorWidth/2);

            Bounds = new BoundsInt(
                pos,
                new Vector3Int(
                sizeX, 0, (int)(this.corridorWidth))
            );
        }

        private int GetCorridorPositionLeftRightZ(Node leftSpace, Node rightSpace)
        {
            // right space is above left space
            if (leftSpace.Bounds.max.z >= rightSpace.Bounds.min.z && rightSpace.Bounds.min.z > leftSpace.Bounds.min.z){

                if (leftSpace.Bounds.max.z - rightSpace.Bounds.min.z <= this.corridorWidth)
                {

                    return -1;

                }


                return CalculateMiddlePoint(
                    rightSpace.Bounds.min, 
                    leftSpace.Bounds.max 
                    ).z;
            }
            // right space is bellow left space
            if(rightSpace.Bounds.max.z >= leftSpace.Bounds.min.z && leftSpace.Bounds.min.z > rightSpace.Bounds.min.z) // before was >=
            {

                if (rightSpace.Bounds.max.z - leftSpace.Bounds.min.z <= this.corridorWidth)
                {
                    return -1;
                }


                return CalculateMiddlePoint(
                   leftSpace.Bounds.min,
                   rightSpace.Bounds.max
                ).z;
            }
            // right space is within bounds of left space
            if(leftSpace.Bounds.max.z >= rightSpace.Bounds.max.z && rightSpace.Bounds.min.z >= leftSpace.Bounds.min.z)
            {
                return CalculateMiddlePoint(
                    rightSpace.Bounds.min,
                    rightSpace.Bounds.max
                ).z;
            }
            // right space contain bounds of left space
            if(rightSpace.Bounds.max.z >= leftSpace.Bounds.max.z && leftSpace.Bounds.min.z >= rightSpace.Bounds.min.z)
            {
                return CalculateMiddlePoint(
                    leftSpace.Bounds.min,
                    leftSpace.Bounds.max
                ).z;
            }

            return -1;
        }

        public Vector3Int CalculateMiddlePoint(Vector3Int a, Vector3Int b)
        {
            var res = (Vector3)(a + b) / 2f;
            return new Vector3Int((int)res.x, (int)res.y, (int)Mathf.Round(res.z));; // does this cause rounding errors?
        }

        private void GenerateCorridorTopBottom(Node node1, Node node2)
        {
            Node topSpace = null; // left space to connect
            List<Node> topSpaceLeaves = GraphHelper.GetLeaves(node2);

            Node bottomSpace = null; // right space to connect
            List<Node> bottomSpaceLeaves = GraphHelper.GetLeaves(node1);

            var sortedTopSpace = topSpaceLeaves.OrderBy(child => child.Bounds.min.z).ToList(); // get bottom most top children
            if (sortedTopSpace.Count == 1)
            {
                topSpace = sortedTopSpace[0];

            }
            else
            {
                // add randomness to which rooms are connected 
                int minZ = sortedTopSpace[0].Bounds.min.z;
                sortedTopSpace = sortedTopSpace.Where(Child => Math.Abs(minZ - Child.Bounds.min.z) < 10).ToList(); // find rooms that have the least deviation from maxX

                // select a random room from valid rooms
                topSpace = sortedTopSpace[Random.RandomRange(0, sortedTopSpace.Count)];

            }

            // find possible connection for the most left aligned room in the right space
            var neighborsInTopSpaceList = bottomSpaceLeaves.Where(
                child => GetCorridorPositionTopBottomX(topSpace, child) != -1

                ).OrderByDescending(child => child.Bounds.max.z).ToList(); // order by ascending (smallest) x

            if (neighborsInTopSpaceList.Count() <= 0)
            {
                bottomSpace = node1;
            }
            else
            {
                // possibly can randomize this
                bottomSpace = neighborsInTopSpaceList[0];
            }

            // potentially add check for not enough clearance
            int corridorX = GetCorridorPositionTopBottomX(topSpace, bottomSpace);

            while (corridorX == -1 && sortedTopSpace.Count > 1)
            {
                // Remove previous space if it was incorrect
                sortedTopSpace.Remove(topSpace);

                // Get Next Possible Neightbour
                topSpace = sortedTopSpace[0];
                corridorX = GetCorridorPositionLeftRightZ(topSpace, bottomSpace); // test if neighbour can be connected using a straight corridor
            }




            var midPointZ = (topSpace.Bounds.min.z + bottomSpace.Bounds.max.z) / 2f;
            var sizeZ = topSpace.Bounds.min.z - bottomSpace.Bounds.max.z;

            var pos = new Vector3Int(corridorX-this.corridorWidth/2, 0, (int)midPointZ-sizeZ/2);

            Bounds = new BoundsInt(
                pos,
                new Vector3Int(
                (int)(this.corridorWidth),0, sizeZ)
            );
        }

        private int GetCorridorPositionTopBottomX(Node topSpace, Node bottomSpace)
        {
            // right space is above left space
            if (topSpace.Bounds.max.x >= bottomSpace.Bounds.min.x && bottomSpace.Bounds.min.x > topSpace.Bounds.min.x)
            {

                if (topSpace.Bounds.max.x - bottomSpace.Bounds.min.x <= this.corridorWidth)
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

                if (bottomSpace.Bounds.max.x - topSpace.Bounds.min.x <= this.corridorWidth)
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
    }
}