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
        static public List<Node> GetLeaves(Node parentNode)
        {
            Queue<Node> nodesToCheck = new Queue<Node>();
            List<Node> leaves = new List<Node>();

            // if rootnode is passed
            if (parentNode.ChildrenNodeList.Count == 0)
            {
                return new List<Node> { parentNode };
            }

            // add all parents children to check for leaves
            foreach (var child in parentNode.ChildrenNodeList)
            {
                nodesToCheck.Enqueue(child);
            }
            
            while (nodesToCheck.Count > 0)
            {
                Node currentNode = nodesToCheck.Dequeue();

                if(currentNode.ChildrenNodeList.Count > 0)
                {
                    foreach (var child in currentNode.ChildrenNodeList)
                    {
                        nodesToCheck.Enqueue(child);
                    }
                }
                else
                {
                    leaves.Add(currentNode);
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

            var startRoom = roomSpaces.OrderBy(room => room.Bounds.xMin + room.Bounds.zMin).ToList()[0];
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