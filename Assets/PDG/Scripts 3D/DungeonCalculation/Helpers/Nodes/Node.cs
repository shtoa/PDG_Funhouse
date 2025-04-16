using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dungeonGenerator
{
    public static class NodeEnumExtensions
    {
        public static Vector3Int toV3I(this SplitPosition splitPosition)
        {
            switch (splitPosition)
            {
                case (SplitPosition.Left):
                    return Vector3Int.left;
                case (SplitPosition.Right):
                    return Vector3Int.right;
                case (SplitPosition.Top):
                    return Vector3Int.forward;
                case (SplitPosition.Bottom):
                    return Vector3Int.back;
                case (SplitPosition.Up):
                    return Vector3Int.up;
                case (SplitPosition.Down):
                    return Vector3Int.down;
                case (SplitPosition.Root):
                    return Vector3Int.zero;
                default:
                    throw new Exception("SplitPosition does not exist");
            }
           
        }

        public static CorridorType toCorridorType(this SplitPosition splitPosition)
        {
            if (Mathf.Abs(Vector3.Dot(Vector3.left, splitPosition.toV3I())) == 1) return CorridorType.Horizontal;
            if (Mathf.Abs(Vector3.Dot(Vector3.forward, splitPosition.toV3I())) == 1) return CorridorType.Vertical;
            if (Mathf.Abs(Vector3.Dot(Vector3.up, splitPosition.toV3I())) == 1) return CorridorType.Perpendicular;
            
            return CorridorType.None;

        }
    }
    public enum SplitPosition
    {
        Root, Top, Bottom, Left, Right, Up, Down
    }
    public enum RoomType
    {
        None,
        Room,
        Start,
        End,
        Corridor,
        DeadEnd
    }

    public enum CorridorType
    {
        None,
        Horizontal,
        Vertical,
        Perpendicular
    }

    public struct DoorPlacement
    {

        public BoundsInt DoorBounds;
        public SplitPosition PositionType;
        public DoorPlacement(BoundsInt _doorBounds, SplitPosition _positionType)
        {
            DoorBounds = _doorBounds;
            PositionType = _positionType;
        }
       

    }

    public struct HolePlacement
    {

        public BoundsInt HoleBounds;
        public SplitPosition PositionType;
        public HolePlacement(BoundsInt _holeBounds, SplitPosition _holePosition)
        {
            HoleBounds = _holeBounds;
            PositionType = _holePosition;
        }


    }

    public class Node
    {
        #region structure elements

        // childrenNodeList contains the children of this node
        private List<Node> childrenNodeList;
        public List<Node> ChildrenNodeList { get => childrenNodeList; }


        public Node Parent; // parent of this node, set to null for root node
        public int TreeLayerIndex; // index of depth of this node

        public int FloorIndex;
        #endregion

        public BoundsInt Bounds { get; set; }

        public List<BoundsInt> CorridorBoundsList { get; set; } = new List<BoundsInt>();
        public List<BoundsInt> CorridorWallBoundsList { get; set; } = new List<BoundsInt>();
        public List<Node> ConnectionsList { get => connectionsList; set => connectionsList = value; }
        public int ConnectionDepthIndex { get => connectionDepthIndex; set => connectionDepthIndex = value; }

        List<DoorPlacement> doorPlacements = new List<DoorPlacement>();
        List<HolePlacement> holePlacements = new List<HolePlacement>();
        public List<DoorPlacement> DoorPlacements { get => doorPlacements; set => doorPlacements = value; }
        public CorridorType CorridorType { get => corridorType; set => corridorType = value; }
        public List<HolePlacement> HolePlacements { get => holePlacements; set => holePlacements = value; }
        public List<Vector3> StairWaypoints { get => stairWaypoints; set => stairWaypoints = value; }
        public Vector3 StairSize { get => stairSize; set => stairSize = value; }
        public Vector3 StairPosition { get => stairPosition; set => stairPosition = value; }

        public SplitPosition SplitPosition;
        public RoomType RoomType;
        private CorridorType corridorType;

        private List<Node> connectionsList;
        private int connectionDepthIndex = -1;

        private List<Vector3> stairWaypoints;
        private Vector3 stairSize;
        private Vector3 stairPosition;


        // Pass in parent node to constructor
        public Node(Node parentNode)
        {
            childrenNodeList = new List<Node>();
            connectionsList = new List<Node>();
            this.Parent = parentNode;
            if (parentNode != null)
            {

                // add this node to the parent if the parent of the current node is not null
                // this allows to build a list of children per node
                parentNode.addChild(this);

            }

            RoomType = RoomType.None;

        }

        public void addChild(Node node)
        {
            childrenNodeList.Add(node);
        }

        public void addConnection(Node node)
        {
            connectionsList.Add(node);
        }

        public void removeChild(Node node)
        {
            childrenNodeList.Remove(node);
        }

        public void calculateDoorPlacement(BoundsInt corridorBounds, SplitPosition splitPosition, int wallThickness)
        {
            // Using the corridor boundaries calculated the bounds of the doors in the room
            DoorPlacement doorPlacement = new DoorPlacement(new BoundsInt(), SplitPosition.Root);

            switch (splitPosition)
            {
                case SplitPosition.Left:

                    doorPlacement = new DoorPlacement(new BoundsInt(

                            new Vector3Int(corridorBounds.min.x, corridorBounds.min.y, corridorBounds.min.z + wallThickness),
                            new Vector3Int(1, corridorBounds.size.y, corridorBounds.size.z - 2 * wallThickness)
                        ),
                        SplitPosition.Right
                    );

                    break;

                case SplitPosition.Right:
                    doorPlacement = new DoorPlacement(new BoundsInt(
                            new Vector3Int(corridorBounds.max.x-wallThickness, corridorBounds.min.y, corridorBounds.min.z + wallThickness),
                            new Vector3Int(1, corridorBounds.size.y, corridorBounds.size.z - 2 * wallThickness)
                        ),
                        SplitPosition.Left
                    );

                    break;

                case SplitPosition.Top:

                    doorPlacement = new DoorPlacement(new BoundsInt(
                            new Vector3Int(corridorBounds.min.x + wallThickness, corridorBounds.min.y, corridorBounds.max.z-wallThickness),
                            new Vector3Int(corridorBounds.size.x - 2 * wallThickness, corridorBounds.size.y, 1)

                        ),
                        SplitPosition.Bottom
                    );

                    break;

                case SplitPosition.Bottom:

                    doorPlacement = new DoorPlacement(new BoundsInt(
                            new Vector3Int(corridorBounds.min.x + wallThickness, corridorBounds.min.y, corridorBounds.min.z),
                            new Vector3Int(corridorBounds.size.x - 2 * wallThickness, corridorBounds.size.y, 1)
                      ),
                      SplitPosition.Top
                  );

                    break;

            }

            doorPlacements.Add(doorPlacement);

        }

        public void addDoorPlacement(DoorPlacement doorPlacement)
        {
            doorPlacements.Add(doorPlacement);
        }


        public void addHolePlacement(HolePlacement holePlacement)
        {
            holePlacements.Add(holePlacement);
        }


        public void calculateHolePlacement(BoundsInt holeBounds, SplitPosition splitPosition, int wallThickness)
        {

            // Using the corridor boundaries calculated the bounds of the doors in the room
            HolePlacement holePlacement = new HolePlacement(new BoundsInt(), SplitPosition.Root);

            if (splitPosition == SplitPosition.Up)
            {
                holePlacement = new HolePlacement(new BoundsInt(
                      new Vector3Int(holeBounds.min.x, holeBounds.max.y, holeBounds.min.z ),
                      new Vector3Int(holeBounds.size.x, 0, holeBounds.size.z )
                      //new Vector3Int(holeBounds.min.x + wallThickness, holeBounds.max.y, holeBounds.min.z + wallThickness),
                      //new Vector3Int(holeBounds.size.x - 2 * wallThickness, 0, holeBounds.size.z - 2 * wallThickness)
                      ),
                      SplitPosition.Down
                  );
            }
            else if (splitPosition == SplitPosition.Down)
            {
                holePlacement = new HolePlacement(new BoundsInt(
                        new Vector3Int(holeBounds.min.x + wallThickness, holeBounds.max.y, holeBounds.min.z + wallThickness),
                        new Vector3Int(holeBounds.size.x - 2 * wallThickness, 0, holeBounds.size.z - 2 * wallThickness)
                  ),
                  SplitPosition.Up
              );
            }
            else
            {
                throw new System.Exception("calculateHolePlacement(): splitPosition Passed");
            }

            holePlacements.Add( holePlacement );
        }
    }
}
