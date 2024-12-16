using System;
using System.Collections.Generic;
using System.Linq;
using dungeonGenerator;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;

namespace dungeonGenerator
{
    public class CorridorGenerator
    {
         
         /// <summary>
         /// Create corridors between a set of nodes.
         /// At present the function relies on the consequence of the BSP tree.
         /// </summary>
         /// <param name="allNodeSpaces"></param>
         /// <param name="corridorWidth"></param>
         /// <returns>corridorList</returns>


        public List<Node> CreateCorridors(List<SpaceNode> allNodeSpaces, int corridorWidth)
        {
            List<Node> corridorList = new List<Node>(); // Create list of Corridors to return
            
            // order spaces deepest first
            Queue<SpaceNode> spacesToCheck = new Queue<SpaceNode>(allNodeSpaces.OrderByDescending(node => node.TreeLayerIndex).ToList());


            // join the children of the spaces together based on the bsp graph
            while (spacesToCheck.Count > 0)
            {
                var spaceToCheck = spacesToCheck.Dequeue(); // dequeue space to check

                if(spaceToCheck.ChildrenNodeList.Count == 0) // cant join the children together so continue the loop
                {
                    continue;
                }
                else if (spaceToCheck.ChildrenNodeList.Count > 1) // check if there are two children to join
                {
                    CorridorNode corridor = new CorridorNode(spaceToCheck.ChildrenNodeList[0], spaceToCheck.ChildrenNodeList[1], corridorWidth); // create new corridor between the childeren
                    corridor.RoomType = RoomType.Corridor; // set room type to corridor
                    corridorList.Add(corridor); // add created corridor for list to return 
                }
            }

            return corridorList; // return generated corridors
        }
    }
}