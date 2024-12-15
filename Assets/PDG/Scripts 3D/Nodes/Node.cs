using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dungeonGenerator
{

    public enum SplitPosition
    {
        Root,Top, Bottom, Left, Right
    }

    public enum RoomType
    {
        None,
        Start,
        End,
        Corridor,
        DeadEnd
    }
    public class Node
    {
        #region structure elements

        // childrenNodeList contains the children of this node
        private List<Node> childrenNodeList;
        public List<Node> ChildrenNodeList { get => childrenNodeList; }


        public Node Parent; // parent of this node, set to null for root node
        public int TreeLayerIndex; // index of depth of this node
        #endregion

        public BoundsInt Bounds { get; set; }
        public List<Node> ConnectionsList { get => connectionsList; set => connectionsList = value; }
        public int ConnectionDepthIndex { get => connectionDepthIndex; set => connectionDepthIndex = value; }

        public SplitPosition SplitPosition;
        public RoomType RoomType;

        private List<Node> connectionsList;
        private int connectionDepthIndex = -1;

        // Pass in parent node to constructor
        public Node(Node parentNode) {
            childrenNodeList = new List<Node>();
            connectionsList = new List<Node>();
            this.Parent = parentNode;
            if(parentNode != null) {

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

        public void removeChild(Node node) { 
            childrenNodeList.Remove(node);
        }
    }
}
