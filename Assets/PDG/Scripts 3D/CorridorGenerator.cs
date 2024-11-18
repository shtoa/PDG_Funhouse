using System;
using System.Collections.Generic;
using System.Linq;
using dungeonGenerator;
using UnityEngine;

namespace dungeonGenerator
{
    public class CorridorGenerator
    {  
        public List<Node> CreateCorridors(List<SpaceNode> allNodeSpaces, int corridorWidth, Vector2Int maxDeviation)
        {
            List<Node> corridorList = new List<Node>();
            
            // order spaces deepest first (leaves of the tree) 
            Queue<SpaceNode> spacesToCheck = new Queue<SpaceNode>(allNodeSpaces.OrderByDescending(node => node.TreeLayerIndex).ToList());


            // join the children of the spaces together based on the bsp graph
            while (spacesToCheck.Count > 0)
            {
                var space = spacesToCheck.Dequeue();

                if(space.ChildrenNodeList.Count == 0)
                {
                    // cant join the children together so continue the loop
                    continue;
                }
                else 
                {
                    CorridorNode corridor = new CorridorNode(space.ChildrenNodeList[0], space.ChildrenNodeList[1], corridorWidth, maxDeviation);
                    corridorList.Add(corridor);
                }
            }

            return corridorList;
        }
    }
}