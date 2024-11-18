using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

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

        public static List<Node> ChooseStartAndEnd(List<Node> roomSpaces)
        {
            List<Node> startEndRooms = new List<Node>();

            var startRoom = roomSpaces.OrderBy(room => room.Bounds.xMin + room.Bounds.zMin).ToList()[0];
            startRoom.RoomType = RoomType.Start;

            startEndRooms.Add(startRoom);

            var endRoom = roomSpaces.OrderByDescending(room => room.Bounds.xMax + room.Bounds.zMax).ToList()[0];
            endRoom.RoomType = RoomType.End;
            startEndRooms.Add(endRoom);


            return startEndRooms;

        }
    }
}