using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.MemoryProfiler;

namespace dungeonGenerator
{

    public class GraphHelper
    {
        
        // genaralize this function for bsp and connections
        static public List<Node> GetLeaves(Node parentNode, bool isBSP = true)
        {
            Queue<Node> nodesToCheck = new Queue<Node>();
            List<Node> leaves = new List<Node>();

            // if rootnode is passed
            if ((isBSP ? parentNode.ChildrenNodeList.Count : parentNode.ConnectionsList.Count) == 0)
            {
                return new List<Node> { parentNode };
            }

            // add all parents children to check for leaves
            foreach (var child in (isBSP ? parentNode.ChildrenNodeList : parentNode.ConnectionsList))
            {
                if (!isBSP)
                {
                    if (parentNode.ConnectionDepthIndex < child.ConnectionDepthIndex)
                    {
                        nodesToCheck.Enqueue(child);
                    }
                }
                else
                {
                    nodesToCheck.Enqueue(child);
                }
            }
            
            while (nodesToCheck.Count > 0)
            {
                Node currentNode = nodesToCheck.Dequeue();

                if((isBSP ? currentNode.ChildrenNodeList.Count : currentNode.ConnectionsList.Count) > 0)
                {
                    foreach (var child in ((isBSP ? currentNode.ChildrenNodeList : currentNode.ConnectionsList)))
                    {

                        if (!isBSP)
                        {
                            if (currentNode.ConnectionDepthIndex < child.ConnectionDepthIndex)
                            {
                                nodesToCheck.Enqueue(child);
                            }
                            else if (currentNode.ConnectionsList.Count == 1)
                            {
                                leaves.Add(currentNode);
                            }
                        } else
                        {
                            nodesToCheck.Enqueue(child);
                        }

                    }
                }
                else
                {
                    leaves.Add(currentNode); // never happens for connectionslist
                }
            }

            return leaves;

        }

        public static void CalculateNodeDepths(Node start)
        {
            // optimize this 

            Queue<List<Node>> layersToCheck = new Queue<List<Node>>();

            if(start.ConnectionsList.Count == 0)
            {
                return; 
            }
            
            layersToCheck.Enqueue(start.ConnectionsList);


            int connectionDepth = 0;
            start.ConnectionDepthIndex = connectionDepth;

            while (layersToCheck.Count > 0)
            {
                connectionDepth++;

                List<Node> curLayer = layersToCheck.Dequeue();
                List<Node> nextLayer = new List<Node>();

                foreach(Node room in curLayer)
                {
                    if (room.ConnectionDepthIndex == -1)
                    {
                        room.ConnectionDepthIndex = connectionDepth;

                        foreach (Node connection in room.ConnectionsList) { 
                            
                            if(connection.ConnectionDepthIndex == -1)
                            {
                                nextLayer.Add(connection);
                            }
                        
                        }
                        
                    }
                }

                if (nextLayer.Count > 0)
                {
                    layersToCheck.Enqueue(nextLayer);
                }
                
            }

        }

        public static List<Node> ChooseStartAndEnd(List<Node> roomSpaces)
        {
            List<Node> startEndRooms = new List<Node>();

            var minY = roomSpaces.Min(room => room.Bounds.yMin);

            var startRoom = roomSpaces.Where(room => room.Bounds.yMin == minY).OrderBy(room => room.Bounds.xMin + room.Bounds.zMin + room.Bounds.yMin).ToList()[0];
            
            startRoom.RoomType = RoomType.Start;

            startEndRooms.Add(startRoom);

            //var endRoom = calculateEndRoom(startRoom);
            CalculateNodeDepths(startRoom);

            var endRoom = roomSpaces.OrderByDescending(room => room.ConnectionDepthIndex).ToList()[0];
            
            endRoom.RoomType = RoomType.End;
            startEndRooms.Add(endRoom);


            return startEndRooms;

        }

        private static Node calculateEndRoom(Node startRoom)
        {
           return startRoom.ConnectionsList[0]; 
        }



    }
}