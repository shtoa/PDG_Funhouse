using System;
using System.Collections.Generic;
using System.Linq;
using dungeonGenerator;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace dungeonGenerator
{
    public class CorridorGenerator
    {
        public int nTimes = 0;
        public void CreateCorridor(SpaceNode space1, SpaceNode space2, List<Node> corridorList, int corridorWidth, int wallThickness, Vector3Int minRoomDim, int corridorHeight,
            bool[,,] availableVoxelGrid)
        {

            CorridorNodePath corridor = new CorridorNodePath(space1, space2, corridorWidth, wallThickness, minRoomDim, corridorHeight, availableVoxelGrid); // create new corridor between the childeren

            corridor.RoomType = RoomType.Corridor;
            corridorList.Add(corridor);

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

        public List<Node> CreateCorridors(List<SpaceNode> allNodeSpaces, int corridorWidth, int wallThickness, Vector3Int minRoomDim, int corridorHeight
              , bool[,,] availableVoxelGrid)
        {
            List<Node> corridorList = new List<Node>(); // Create list of Corridors to return
            
            // order spaces deepest first
            Queue<SpaceNode> spacesToCheck = new Queue<SpaceNode>(allNodeSpaces.OrderByDescending(node => node.TreeLayerIndex).ToList());
            connectSpaces(spacesToCheck, corridorWidth, wallThickness, minRoomDim, corridorHeight
              , availableVoxelGrid, corridorList, 1f);

            // second pass to add circular connections 
            spacesToCheck = new Queue<SpaceNode>(allNodeSpaces.OrderByDescending(node => node.TreeLayerIndex).ToList());
            connectSpaces(spacesToCheck, corridorWidth, wallThickness, minRoomDim, corridorHeight
              , availableVoxelGrid, corridorList, 0.5f); // add split chance


            return corridorList; // return generated corridors
        }



        public void connectSpaces(Queue<SpaceNode> spacesToCheck, int corridorWidth, int wallThickness, Vector3Int minRoomDim, int corridorHeight
              , bool[,,] availableVoxelGrid, List<Node> corridorList, float connectionChance)
        {
            // join the children of the spaces together based on the bsp graph
            while (spacesToCheck.Count > 0)
            {
                var spaceToCheck = spacesToCheck.Dequeue(); // dequeue space to check

                if (spaceToCheck.ChildrenNodeList.Count == 0 || spaceToCheck.ChildrenNodeList.Count == 1) // cant join the children together so continue the loop
                {
                    continue;
                }
                else if (spaceToCheck.ChildrenNodeList.Count > 1 && (Random.value < connectionChance)) // check if there are two children to join
                {
                    CreateCorridor((SpaceNode)spaceToCheck.ChildrenNodeList[0], (SpaceNode)spaceToCheck.ChildrenNodeList[1], corridorList, corridorWidth, wallThickness, minRoomDim, corridorHeight,
                        availableVoxelGrid);
                }
            }
        }
    }


    
}