﻿using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
namespace tutorialGenerator
{
    public class CorridorNode : Node
    {
        private Node structure1;
        private Node structure2;
        private int corridorWidth;
        private int modifierDistanceFromWall = 3;

        public CorridorNode(Node node1, Node node2, int corridorWidth) : base(null)
        {
            this.structure1 = node1;
            this.structure2 = node2;
            this.corridorWidth = corridorWidth;

            GenerateCorridor();
        }

        private void GenerateCorridor()
        {
            var relativePositionOfStructure2 = CheckPositionStructure2AgainstPosition1();
            switch (relativePositionOfStructure2)
            {
                case RelativePosition.Up:
                    ProcessRoomInRelationUpOrDown(this.structure1, this.structure2);
                    break;
                case RelativePosition.Down:
                    ProcessRoomInRelationUpOrDown(this.structure2, this.structure1);
                    break;
                case RelativePosition.Right:
                    ProcessRoomInRelationRightOrLeft(this.structure1, this.structure2);
                    break;

                case RelativePosition.Left:
                    ProcessRoomInRelationRightOrLeft(this.structure2, this.structure1);
                    break;

                default:
                    break;
            }
        }

        private void ProcessRoomInRelationRightOrLeft(Node structure1, Node structure2)
        {
            Node leftStructure = null;
            List<Node> leftStructureChildren = StructureHelper.TraverseGraphToExtractLowestLeaves(structure1);


            Node rightStructure = null;
            List<Node> rightStructureChildren = StructureHelper.TraverseGraphToExtractLowestLeaves(structure2);

            // most right aligned structures in left
            var sortedLeftStructure = leftStructureChildren.OrderByDescending(child => child.TopRightAreaCorner.x).ToList();
            if (sortedLeftStructure.Count() == 1)
            {
                leftStructure = sortedLeftStructure[0];
            }
            else
            {
                int maxX = sortedLeftStructure[0].TopRightAreaCorner.x;
                sortedLeftStructure = sortedLeftStructure.Where(child => Mathf.Abs(maxX - child.TopRightAreaCorner.x) < 10).ToList();
                int index = UnityEngine.Random.Range(0, sortedLeftStructure.Count());
                leftStructure = sortedLeftStructure[index];
            }

            // most left aligned in right
            var possibleNeighboursInRightStructureList = rightStructureChildren.Where(
                child => GetValidYForNeighbourLeftRight(
                leftStructure.TopRightAreaCorner,
                leftStructure.BottomRightAreaCorner,
                child.TopLeftAreaCorner,
                child.BottomLeftAreaCorner
                ) != -1 // check this might be wrong ... 

            ).OrderBy(child => child.BottomRightAreaCorner.x).ToList();

            if (possibleNeighboursInRightStructureList.Count() <= 0)
            {
                rightStructure = structure2;
            }
            else
            {
                rightStructure = possibleNeighboursInRightStructureList[0];
            }

            int y = GetValidYForNeighbourLeftRight(leftStructure.TopLeftAreaCorner, leftStructure.BottomRightAreaCorner,
                rightStructure.TopLeftAreaCorner, rightStructure.BottomLeftAreaCorner
            );

            while (y == -1 && sortedLeftStructure.Count() > 1)
            {
                sortedLeftStructure = sortedLeftStructure.Where(
                    child => child.TopLeftAreaCorner.y != leftStructure.TopLeftAreaCorner.y).ToList();
                leftStructure = sortedLeftStructure[0];
                y = GetValidYForNeighbourLeftRight(leftStructure.TopLeftAreaCorner, leftStructure.BottomRightAreaCorner,
                rightStructure.TopLeftAreaCorner, rightStructure.BottomLeftAreaCorner
                );
            }
            // add condition for -1

            if (y == -1)
            {
                Debug.Log("Y ERROR ON THE GENERATION");
            }

            BottomLeftAreaCorner = new Vector2Int(leftStructure.BottomRightAreaCorner.x, y);
            TopRightAreaCorner = new Vector2Int(rightStructure.TopLeftAreaCorner.x, y + this.corridorWidth);


        }

        private int GetValidYForNeighbourLeftRight(Vector2Int leftNodeUp, Vector2Int leftNodeDown, Vector2Int rightNodeUp, Vector2Int rightNodeDown)
        {
            if (rightNodeUp.y >= leftNodeUp.y && leftNodeDown.y >= rightNodeDown.y)
            {
                return StructureHelper.CalculateMiddlePoint(
                    leftNodeDown + new Vector2Int(0, modifierDistanceFromWall),
                    leftNodeUp - new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)
                    ).y;

            }
            if (rightNodeUp.y <= leftNodeUp.y && leftNodeDown.y <= rightNodeDown.y)
            {
                return StructureHelper.CalculateMiddlePoint(
                    rightNodeDown + new Vector2Int(0, modifierDistanceFromWall),
                    rightNodeUp - new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)
                    ).y;

            }
            if (leftNodeUp.y >= rightNodeDown.y && leftNodeUp.y <= rightNodeUp.y)
            {
                return StructureHelper.CalculateMiddlePoint(
                    rightNodeDown + new Vector2Int(0, modifierDistanceFromWall),
                    leftNodeUp - new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)
                    ).y;

            }
            if (leftNodeDown.y >= rightNodeDown.y && leftNodeDown.y <= rightNodeUp.y)
            {
                return StructureHelper.CalculateMiddlePoint(
                     leftNodeDown + new Vector2Int(0, modifierDistanceFromWall),
                     rightNodeUp - new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)
                     ).y;

            }

            return -1;
        }

        private void ProcessRoomInRelationUpOrDown(Node structure1, Node structure2)
        {
            Node bottomStructure = null;
            List<Node> structureBottomChildren = StructureHelper.TraverseGraphToExtractLowestLeaves(structure1);


            Node topStructure = null;
            List<Node> structureAboveChildren = StructureHelper.TraverseGraphToExtractLowestLeaves(structure2);

            // most right aligned structures in left
            var sortedBottomStructure = structureBottomChildren.OrderByDescending(child => child.TopRightAreaCorner.y).ToList();
            if (sortedBottomStructure.Count() == 1)
            {
                bottomStructure = sortedBottomStructure[0];
            }
            else
            {
                int maxY = sortedBottomStructure[0].TopLeftAreaCorner.y;
                sortedBottomStructure = sortedBottomStructure.Where(child => Mathf.Abs(maxY - child.TopLeftAreaCorner.y) < 10).ToList();
                int index = UnityEngine.Random.Range(0, sortedBottomStructure.Count());
                bottomStructure = sortedBottomStructure[index];
            }

            // most left aligned in right
            var possibleNeighboursInTopStructureList = structureAboveChildren.Where(
                child => GetValidXForNeighbourUpDown(
                bottomStructure.TopLeftAreaCorner,
                bottomStructure.TopRightAreaCorner,
                child.BottomLeftAreaCorner,
                child.BottomRightAreaCorner
                ) != -1 // check this might be wrong ... 

            ).OrderBy(child => child.BottomRightAreaCorner.y).ToList();

            if (possibleNeighboursInTopStructureList.Count() <= 0)
            {
                topStructure = structure2;
            }
            else
            {
                topStructure = possibleNeighboursInTopStructureList[0];
            }

            int x = GetValidXForNeighbourUpDown(bottomStructure.TopLeftAreaCorner, bottomStructure.TopRightAreaCorner,
                topStructure.BottomLeftAreaCorner, topStructure.BottomRightAreaCorner
            );

            while (x == -1 && sortedBottomStructure.Count() > 1)
            {
                sortedBottomStructure = sortedBottomStructure.Where(
                child => child.TopLeftAreaCorner.x != topStructure.TopLeftAreaCorner.x).ToList();
                bottomStructure = sortedBottomStructure[0];
                x = GetValidXForNeighbourUpDown(bottomStructure.TopLeftAreaCorner, bottomStructure.TopRightAreaCorner,
                topStructure.BottomLeftAreaCorner, topStructure.BottomRightAreaCorner
                );
            }
            // add condition for -1

            BottomLeftAreaCorner = new Vector2Int(x, bottomStructure.TopLeftAreaCorner.y);
            TopRightAreaCorner = new Vector2Int(x + this.corridorWidth, topStructure.BottomLeftAreaCorner.y);

            if (x == -1)
            {
                Debug.Log("ERROR ON THE X CONNECTION");
            }
        }

        private int GetValidXForNeighbourUpDown(Vector2Int bottomNodeLeft, Vector2Int bottomNodeRight, Vector2Int topNodeLeft, Vector2Int topNodeRight)
        {

            if (topNodeLeft.x < bottomNodeLeft.x && bottomNodeRight.x < topNodeRight.x)
            {

                return StructureHelper.CalculateMiddlePoint(
                    bottomNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
                    bottomNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall, 0)
                    ).x;

            }
            if (topNodeLeft.x >= bottomNodeLeft.x && bottomNodeRight.x >= topNodeRight.x)
            {
                return StructureHelper.CalculateMiddlePoint(
                  topNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
                  topNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall, 0)
                  ).x;
            }
            if (bottomNodeLeft.x >= (topNodeLeft.x) && bottomNodeLeft.x <= (topNodeRight.x))
            {
                return StructureHelper.CalculateMiddlePoint(
                  bottomNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
                  topNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall, 0)
                  ).x;
            }
            if (bottomNodeRight.x <= topNodeRight.x && bottomNodeRight.x >= topNodeLeft.x)
            {
                return StructureHelper.CalculateMiddlePoint(
                  topNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
                  bottomNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall, 0)
                  ).x;
            }

            return -1;
        }

        private object CheckPositionStructure2AgainstPosition1()
        {
            Vector2 middlePointStructure1Temp = ((Vector2)structure1.TopRightAreaCorner + structure1.BottomLeftAreaCorner) / 2;
            Vector2 middlePointStructure2Temp = ((Vector2)structure2.TopRightAreaCorner + structure2.BottomLeftAreaCorner) / 2;

            float angle = CalculateAngle(middlePointStructure1Temp, middlePointStructure2Temp);

            if ((angle < 45 && angle >= 0) || (angle > -45 && angle < 0))
            {
                return RelativePosition.Right;

            }
            else if ((angle > 45 && angle < 135))
            {

                return RelativePosition.Up;

            }
            else if ((angle > -135 && angle < -45))
            {
                return RelativePosition.Down;

            }
            else
            {
                return RelativePosition.Left;
            }
        }

        private float CalculateAngle(Vector2 middlePointStructure1Temp, Vector2 middlePointStructure2Temp)
        {
            return Mathf.Atan2(
                middlePointStructure2Temp.y - middlePointStructure1Temp.y,
                middlePointStructure2Temp.x - middlePointStructure1Temp.x
           ) * Mathf.Rad2Deg;
        }
    }
}