using System;
using System.Collections.Generic;
using System.Linq;
using dungeonGenerator;
using Unity.VisualScripting;
using UnityEngine;

namespace dungeonGenerator
{
    public class CorridorGenerator
    {
        public void CreateCorridor(SpaceNode space1, SpaceNode space2, List<Node> corridorList, int corridorWidth, int wallThickness, Vector3Int minRoomDim, int corridorHeight)
        {
            CorridorNode corridor = new CorridorNode(space1, space2, corridorWidth, wallThickness, minRoomDim, corridorHeight); // create new corridor between the childeren

            if (corridor.CorridorType != CorridorType.None)
            {
                corridor.RoomType = RoomType.Corridor; // set room type to corridor
                corridorList.Add(corridor); // add created corridor for list to return 
            } else
            {
                haveCorridorsFailed = true;
            }
     
        }

        public bool haveCorridorsFailed;

        /// <summary>
        /// Create corridors between a set of nodes.
        /// At present the function relies on the consequence of the BSP tree.
        /// </summary>
        /// <param name="allNodeSpaces"></param>
        /// <param name="corridorWidth"></param>
        /// <param name="minRoomDim"></param>
        /// <returns>corridorList</returns>

        public List<Node> CreateCorridors(List<SpaceNode> allNodeSpaces, int corridorWidth, int wallThickness, Vector3Int minRoomDim, int corridorHeight)
        {
            List<Node> corridorList = new List<Node>(); // Create list of Corridors to return
            
            // order spaces deepest first
            Queue<SpaceNode> spacesToCheck = new Queue<SpaceNode>(allNodeSpaces.OrderByDescending(node => node.TreeLayerIndex).ToList());


            // join the children of the spaces together based on the bsp graph
            while (spacesToCheck.Count > 0)
            {
                var spaceToCheck = spacesToCheck.Dequeue(); // dequeue space to check

                if(spaceToCheck.ChildrenNodeList.Count == 0 || spaceToCheck.ChildrenNodeList.Count == 1) // cant join the children together so continue the loop
                {
                    continue;
                }
                else if (spaceToCheck.ChildrenNodeList.Count > 1
                
                    #region Testing
                    //&& spaceToCheck.ChildrenNodeList[0] != null
                    //&& spaceToCheck.ChildrenNodeList[1] != null

                    //&& spaceToCheck.ChildrenNodeList[0].ChildrenNodeList.Count > 1
                    //&& spaceToCheck.ChildrenNodeList[1].ChildrenNodeList.Count > 1

                    //&& spaceToCheck.ChildrenNodeList[0].ChildrenNodeList != null
                    //&& spaceToCheck.ChildrenNodeList[1].ChildrenNodeList != null
                #endregion

                    ) // check if there are two children to join
                {
                    CreateCorridor((SpaceNode)spaceToCheck.ChildrenNodeList[0], (SpaceNode)spaceToCheck.ChildrenNodeList[1], corridorList, corridorWidth, wallThickness, minRoomDim, corridorHeight);
                }
            }

            return corridorList; // return generated corridors
        }
    }
}