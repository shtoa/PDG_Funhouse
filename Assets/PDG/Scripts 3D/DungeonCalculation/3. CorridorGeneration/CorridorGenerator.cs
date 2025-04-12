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


            //Debug.Log($"Space1Leaves: {GraphHelper.GetLeaves(space1).Count}, Space2Leaves: {GraphHelper.GetLeaves(space2).Count}");

            CorridorNodePath corridor = new CorridorNodePath(space1, space2, corridorWidth, wallThickness, minRoomDim, corridorHeight, availableVoxelGrid, 0); // create new corridor between the childeren

            corridor.RoomType = RoomType.Corridor;
            corridorList.Add(corridor);


            // check if top and bottom spaces are avaialable
            //if (GraphHelper.GetLeaves(space1).Count > 1 && GraphHelper.GetLeaves(space2).Count > 1 && Random.value < 0.5)
            //{
            //    Debug.Log($"nTimes {nTimes}");
            //    corridor = new CorridorNodePath(space1, space2, corridorWidth, wallThickness, minRoomDim, corridorHeight, availableVoxelGrid, 1); // create new corridor between the childeren

            //    if (corridor.CorridorType != CorridorType.None)
            //    {
            //        corridor.RoomType = RoomType.Corridor;
            //        corridorList.Add(corridor);
            //    }

            //    Debug.Log($"nTimes {nTimes}");

            //    //nTimes++;

            //}

            //if (corridor.CorridorType != CorridorType.None)
            //{
            //    corridor.RoomType = RoomType.Corridor; // set room type to corridor
            //    corridorList.Add(corridor); // add created corridor for list to return 
            //} else
            //{
            //    haveCorridorsFailed = true;
            //}

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

            // join the children of the spaces together based on the bsp graph
            //while (spacesToCheck.Count > 0)
            //{
            //    var spaceToCheck = spacesToCheck.Dequeue(); // dequeue space to check

            //    if(spaceToCheck.ChildrenNodeList.Count == 0 || spaceToCheck.ChildrenNodeList.Count == 1) // cant join the children together so continue the loop
            //    {
            //        continue;
            //    }
            //    else if (spaceToCheck.ChildrenNodeList.Count > 1

            //        #region Testing
            //        //&& spaceToCheck.ChildrenNodeList[0] != null
            //        //&& spaceToCheck.ChildrenNodeList[1] != null

            //        //&& spaceToCheck.ChildrenNodeList[0].ChildrenNodeList.Count > 1
            //        //&& spaceToCheck.ChildrenNodeList[1].ChildrenNodeList.Count > 1

            //        //&& spaceToCheck.ChildrenNodeList[0].ChildrenNodeList != null
            //        //&& spaceToCheck.ChildrenNodeList[1].ChildrenNodeList != null
            //    #endregion

            //        ) // check if there are two children to join
            //    {
            //        CreateCorridor((SpaceNode)spaceToCheck.ChildrenNodeList[0], (SpaceNode)spaceToCheck.ChildrenNodeList[1], corridorList, corridorWidth, wallThickness, minRoomDim, corridorHeight,
            //            availableVoxelGrid);

            //    }
            //}



            return corridorList; // return generated corridors
        }



        public void connectSpaces(Queue<SpaceNode> spacesToCheck, int corridorWidth, int wallThickness, Vector3Int minRoomDim, int corridorHeight
              , bool[,,] availableVoxelGrid, List<Node> corridorList, float splitChance)
        {
            // join the children of the spaces together based on the bsp graph
            while (spacesToCheck.Count > 0)
            {
                var spaceToCheck = spacesToCheck.Dequeue(); // dequeue space to check

                if (spaceToCheck.ChildrenNodeList.Count == 0 || spaceToCheck.ChildrenNodeList.Count == 1) // cant join the children together so continue the loop
                {
                    continue;
                }
                else if (spaceToCheck.ChildrenNodeList.Count > 1 && (Random.value < splitChance)

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
                    CreateCorridor((SpaceNode)spaceToCheck.ChildrenNodeList[0], (SpaceNode)spaceToCheck.ChildrenNodeList[1], corridorList, corridorWidth, wallThickness, minRoomDim, corridorHeight,
                        availableVoxelGrid);

                }
            }
        }
    }


    
}