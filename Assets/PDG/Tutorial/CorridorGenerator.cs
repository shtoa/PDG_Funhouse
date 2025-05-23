﻿using System;
using System.Collections.Generic;
using System.Linq;



namespace tutorialGenerator
{
    public class CorridorGenerator
    {
        private List<RoomNode> allNodesCollection;
        private int corridorWidth;
        private object value;

        public CorridorGenerator()
        {

        }

        public List<Node> CreateCorridor(List<RoomNode> allNodesCollection, int corridorWidth)
        {
            List<Node> corridorList = new List<Node>();
            Queue<RoomNode> structuresToCheck = new Queue<RoomNode>(allNodesCollection.OrderByDescending(node => node.TreeLayerIndex).ToList());

            while (structuresToCheck.Count > 0)
            {
                var node = structuresToCheck.Dequeue();
                if (node.ChildrenNodeList.Count == 0)
                {
                    continue;
                }
                else
                {
                    CorridorNode corridor = new CorridorNode(node.ChildrenNodeList[0], node.ChildrenNodeList[1], corridorWidth);
                    corridorList.Add(corridor);
                }


            }

            return corridorList;
        }
    }
}