using dungeonGenerator;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
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
                    //GenerateCorridorTopBottom(this.node1, this.node2);
                    break;
                case SplitPosition.Bottom:
                    //GenerateCorridorTopBottom(this.node2, this.node1);
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
            List<Node> leftSpaceLeaves = GraphHelper.GetLeaves(node1);

            Node rightSpace = null; // right space to connect
            List<Node> rightSpaceLeaves = GraphHelper.GetLeaves(node2);

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
                child => GetCorridorPositionLeftRightY(leftSpace, child) != -1

                ).OrderBy(child => child.Bounds.min.x).ToList(); // order by ascending (smallest) x

            if(neighborsInLeftSpaceList.Count() <= 0)
            {
                rightSpace = node1;
            } else
            {
                // possibly can randomize this
                rightSpace = neighborsInLeftSpaceList[0];
            }

            int corridorY = GetCorridorPositionLeftRightY(leftSpace, rightSpace);

            while(corridorY != -1 && sortedLeftSpace.Count > 1)
            {
                // Remove previous space if it was incorrect
                sortedLeftSpace.Remove(leftSpace);
                
                // Get Next Possible Neightbour
                leftSpace = sortedLeftSpace[0];
                corridorY = GetCorridorPositionLeftRightY(leftSpace, rightSpace); // test if neighbour can be connected using a straight corridor
            }

            // add a failsafe if no neighbor is found ???

            var pos = new Vector3Int(leftSpace.Bounds.max.x, corridorY, leftSpace.Bounds.max.z);

            Bounds = new BoundsInt(
                pos,
                new Vector3Int(rightSpace.Bounds.min.x, y, rightSpace.Bounds.z) - pos
            );
        }

        private int GetCorridorPositionLeftRightY(Node leftSpace, Node rightSpace)
        {
            // right space is above left space
            if (leftSpace.Bounds.max.y >= rightSpace.Bounds.min.y && leftSpace.Bounds.max.y >= rightSpace.Bounds.max.y){
                return CalculateMiddlePoint(
                    leftSpace.Bounds.max + new Vector3Int(0,modifierDistanceFromWall,0), 
                    rightSpace.Bounds.min - new Vector3Int(0, this.corridorWidth + modifierDistanceFromWall, 0)
                    ).y;
            }
            // right space is bellow left space
            if(rightSpace.Bounds.max.y >= leftSpace.Bounds.min.y && leftSpace.Bounds.min.y >= rightSpace.Bounds.min.y)
            {
                return CalculateMiddlePoint(
                   leftSpace.Bounds.min + new Vector3Int(0, modifierDistanceFromWall, 0),
                   rightSpace.Bounds.min - new Vector3Int(0, this.corridorWidth + modifierDistanceFromWall, 0)
                ).y;
            }
            // right space is within bounds of left space
            if(leftSpace.Bounds.max.y >= rightSpace.Bounds.max.y && rightSpace.Bounds.min.y >= leftSpace.Bounds.min.y)
            {
                return CalculateMiddlePoint(
                    rightSpace.Bounds.min + new Vector3Int(0, modifierDistanceFromWall, 0),
                    rightSpace.Bounds.max - new Vector3Int(0, this.corridorWidth - modifierDistanceFromWall, 0)
                ).y;
            }
            // right space contain bounds of left space
            if(rightSpace.Bounds.max.y >= leftSpace.Bounds.max.y && leftSpace.Bounds.min.y >= rightSpace.Bounds.min.y)
            {
                return CalculateMiddlePoint(
                    leftSpace.Bounds.min + new Vector3Int(0, modifierDistanceFromWall, 0),
                    leftSpace.Bounds.max - new Vector3Int(0, this.corridorWidth - modifierDistanceFromWall, 0)
                ).y;
            }

            return -1;
        }

        public Vector3Int CalculateMiddlePoint(Vector3Int a, Vector3Int b)
        {
            var res = (Vector3)(a + b) / 2f;
            return new Vector3Int((int)res.x, (int)res.y, (int)res.z); // does this cause rounding errors?
        }

        private void GenerateCorridorTopBottom(Node node2, Node node1)
        {
            throw new NotImplementedException();
        }
    }
}